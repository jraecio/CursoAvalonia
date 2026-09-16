using CursoAvalonia.Data;
using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class SincronizacaoPedidosService
{
    private static readonly SemaphoreSlim Exclusao = new(1, 1);
    private readonly Func<DbContext> _criarLocal;
    private readonly Func<Task<DbContext>> _criarRemoto;
    private readonly Func<DbContext, Task> _prepararRemoto;

    public SincronizacaoPedidosService()
        : this(() => new LocalDbContext(),
            async () => await new RemoteDbContextFactory().CriarAsync()) { }

    public SincronizacaoPedidosService(
        Func<DbContext> criarLocal, Func<Task<DbContext>> criarRemoto,
        Func<DbContext, Task>? prepararRemoto = null)
    {
        _criarLocal = criarLocal;
        _criarRemoto = criarRemoto;
        _prepararRemoto = prepararRemoto ?? (db => db.Database.MigrateAsync());
    }

    public async Task<(int Enviados, List<string> Erros)> SincronizarAsync()
    {
        await Exclusao.WaitAsync();
        try
        {
            // Fixa o destino durante todo o envio e aplica somente as migrations
            // desse contexto antes de consultar as tabelas, mesmo sem pedidos locais.
            await using var remoto = await _criarRemoto();
            try
            {
                await _prepararRemoto(remoto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Não foi possível preparar as tabelas do banco {remoto.Database.GetDbConnection().Database}. " +
                    "Nenhum pedido foi enviado. " + (ex.InnerException?.Message ?? ex.Message), ex);
            }
            await using var local = _criarLocal();
            var ids = await local.Set<Pedido>().AsNoTracking()
                .Where(p => !p.Sincronizado).OrderBy(p => p.NumeroPedido)
                .Select(p => p.Id).ToListAsync();
            int enviados = 0;
            List<string> erros = new();
            foreach (var id in ids)
            {
                try
                {
                    remoto.ChangeTracker.Clear();
                    if (await EnviarPedidoAsync(id, remoto)) enviados++;
                }
                catch (Exception ex)
                {
                    erros.Add($"Pedido {id}: {ex.InnerException?.Message ?? ex.Message}");
                    // Evita repetir o tempo de espera por cada pedido quando o servidor está offline.
                    if (ex is not ConflitoPedidoException) break;
                }
            }
            return (enviados, erros);
        }
        catch (Exception ex)
        {
            return (0, new List<string> { ex.Message });
        }
        finally { Exclusao.Release(); }
    }

    private async Task<bool> EnviarPedidoAsync(Guid id, DbContext remoto)
    {
        await using var local = _criarLocal();
        Pedido? pedido;
        List<LogAuditoria> logs;
        // Lê pedido, itens e auditoria da mesma versão local.
        await using (var leitura = await local.Database.BeginTransactionAsync())
        {
            pedido = await local.Set<Pedido>().AsNoTracking().Include(p => p.Itens)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (pedido == null || pedido.Sincronizado) return false;
            logs = await local.Set<LogAuditoria>().AsNoTracking()
                .Where(l => l.PedidoId == id).ToListAsync();
            await leitura.CommitAsync();
        }

        await using (var transacao = await remoto.Database.BeginTransactionAsync())
        {
            bool numeroEmUso = await remoto.Set<Pedido>().AnyAsync(
                p => p.NumeroPedido == pedido.NumeroPedido && p.Id != pedido.Id);
            if (numeroEmUso)
                throw new ConflitoPedidoException(
                    $"O número {pedido.NumeroPedido} já pertence a outro pedido no SQL Server. O pedido local permanece pendente.");

            var destino = await remoto.Set<Pedido>().Include(p => p.Itens)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (destino != null && destino.AtualizadoEm > pedido.AtualizadoEm)
                throw new ConflitoPedidoException(
                    $"O pedido {pedido.NumeroPedido} possui uma versão mais recente no SQL Server.");

            if (destino == null)
            {
                destino = new Pedido { Id = pedido.Id };
                remoto.Set<Pedido>().Add(destino);
            }
            remoto.Entry(destino).CurrentValues.SetValues(pedido);
            destino.Sincronizado = true;

            var idsItens = pedido.Itens.Select(i => i.Id).ToHashSet();
            foreach (var antigo in destino.Itens.Where(i => !idsItens.Contains(i.Id)).ToList())
                remoto.Set<ItemPedido>().Remove(antigo);
            foreach (var item in pedido.Itens)
            {
                var existente = destino.Itens.SingleOrDefault(i => i.Id == item.Id);
                if (existente == null)
                {
                    existente = new ItemPedido { Id = item.Id, PedidoId = pedido.Id };
                    destino.Itens.Add(existente);
                    remoto.Set<ItemPedido>().Add(existente);
                }
                remoto.Entry(existente).CurrentValues.SetValues(item);
            }

            var idsLogs = (await remoto.Set<LogAuditoria>()
                .Where(l => l.PedidoId == id).Select(l => l.Id).ToListAsync()).ToHashSet();
            foreach (var log in logs.Where(l => !idsLogs.Contains(l.Id)))
            {
                var copia = new LogAuditoria();
                remoto.Entry(copia).CurrentValues.SetValues(log);
                remoto.Set<LogAuditoria>().Add(copia);
            }
            await remoto.SaveChangesAsync();
            await transacao.CommitAsync();
        }

        // Uma edição realizada durante o envio deve permanecer pendente.
        await local.Set<Pedido>()
            .Where(p => p.Id == id && p.AtualizadoEm == pedido.AtualizadoEm)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Sincronizado, true));
        return true;
    }

    private sealed class ConflitoPedidoException(string mensagem) : Exception(mensagem);
}
