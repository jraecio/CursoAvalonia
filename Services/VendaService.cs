using CursoAvalonia.Data;
using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class VendaService
{
    public async Task<Pedido> FinalizarVendaAsync(
        IEnumerable<ItemVenda> itensVenda,
        decimal total,
        decimal desconto,
        decimal subTotal)
    {
        await using var db = new LocalDbContext();

        await using var transacao = await db.Database.BeginTransactionAsync();

        try
        {
            Pedido pedido = new Pedido
            {
                Total = total,
                Desconto = desconto,
                SubTotal = subTotal,
                Sincronizado = false,
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            };

            foreach (var item in itensVenda)
            {
                pedido.Itens.Add(new ItemPedido
                {
                    ProdutoId = item.Codigo,
                    DescricaoProduto = item.Descricao,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.Valor,
                    Total = item.Total
                });
            }

            db.Pedidos.Add(pedido);

            db.LogsAuditoria.Add(new LogAuditoria
            {
                PedidoId = pedido.Id,
                Acao = "PEDIDO_FINALIZADO",
                Descricao = "Pedido finalizado e salvo no banco local.",
                DataHora = DateTime.Now
            });

            await db.SaveChangesAsync();

            await transacao.CommitAsync();

            return pedido;
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }
}