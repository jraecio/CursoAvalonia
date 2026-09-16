using CursoAvalonia.Data;
using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class VendaService
{
    // =========================================================
    // LISTAR PEDIDOS
    // =========================================================

    public async Task<List<Pedido>> ListarPedidosAsync()
    {
        await using var db = new LocalDbContext();

        return await db.Pedidos
            .Include(p => p.Itens)
            .OrderByDescending(p => p.NumeroPedido)
            .ToListAsync();
    }


    // =========================================================
    // BUSCAR PEDIDO POR NÚMERO
    // =========================================================

    public async Task<Pedido?> BuscarPedidoAsync(int numeroPedido)
    {
        await using var db = new LocalDbContext();

        return await db.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(
                p => p.NumeroPedido == numeroPedido
            );
    }


    // =========================================================
    // GERAR PRÓXIMO NÚMERO
    // =========================================================

    private async Task<int> GerarProximoNumeroPedidoAsync(
        LocalDbContext db)
    {
        int ultimoNumero = await db.Pedidos
            .MaxAsync(p => (int?)p.NumeroPedido)
            ?? 0;

        return ultimoNumero + 1;
    }


    // =========================================================
    // FINALIZAR NOVA VENDA
    // =========================================================

    public async Task<Pedido> FinalizarVendaAsync(
        IEnumerable<ItemVenda> itensVenda,
        int? clienteId,
        decimal total,
        decimal desconto,
        decimal subTotal,
        string formaPagamento,
        decimal valorPago,
        decimal troco)
    {
        await using var db = new LocalDbContext();

        await using var transacao =
            await db.Database.BeginTransactionAsync();

        try
        {
            int numeroPedido =
                await GerarProximoNumeroPedidoAsync(db);

            Pedido pedido = new Pedido
            {
                NumeroPedido = numeroPedido,

                Data = DateTime.Now,

                ClienteId = clienteId,

                Total = total,

                Desconto = desconto,

                SubTotal = subTotal,

                FormaPagamento = formaPagamento,

                ValorPago = valorPago,

                Troco = troco,

                Lacrado = true,

                Cancelado = false,

                Sincronizado = false,

                CriadoEm = DateTime.Now,

                AtualizadoEm = DateTime.Now
            };


            foreach (ItemVenda item in itensVenda)
            {
                pedido.Itens.Add(
                    new ItemPedido
                    {
                        ProdutoId = item.Codigo,

                        DescricaoProduto =
                            item.Descricao,

                        Quantidade =
                            item.Quantidade,

                        ValorUnitario =
                            item.Valor,

                        Total =
                            item.Total
                    }
                );
            }


            db.Pedidos.Add(pedido);


            db.LogsAuditoria.Add(
                new LogAuditoria
                {
                    PedidoId = pedido.Id,

                    Acao =
                        "PEDIDO_FINALIZADO",

                    Descricao =
                        $"Pedido {numeroPedido} finalizado e lacrado.",

                    DataHora =
                        DateTime.Now
                }
            );


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


    // =========================================================
    // ATUALIZAR PEDIDO
    // =========================================================

    public async Task<Pedido> AtualizarPedidoAsync(
        Pedido pedido,
        IEnumerable<ItemVenda> itensVenda)
    {
        await using var db = new LocalDbContext();

        await using var transacao =
            await db.Database.BeginTransactionAsync();

        try
        {
            Pedido? pedidoBanco =
                await db.Pedidos
                    .Include(p => p.Itens)
                    .FirstOrDefaultAsync(
                        p => p.Id == pedido.Id
                    );


            if (pedidoBanco == null)
            {
                throw new Exception(
                    "Pedido não encontrado."
                );
            }


            if (pedidoBanco.Cancelado)
            {
                throw new Exception(
                    "Pedido cancelado não pode ser alterado."
                );
            }


            pedidoBanco.ClienteId =
                pedido.ClienteId;

            pedidoBanco.Total =
                pedido.Total;

            pedidoBanco.Desconto =
                pedido.Desconto;

            pedidoBanco.SubTotal =
                pedido.SubTotal;

            pedidoBanco.FormaPagamento =
                pedido.FormaPagamento;

            pedidoBanco.ValorPago =
                pedido.ValorPago;

            pedidoBanco.Troco =
                pedido.Troco;

            pedidoBanco.Lacrado =
                true;

            pedidoBanco.Sincronizado =
                false;

            pedidoBanco.AtualizadoEm =
                DateTime.Now;


            // Remove itens antigos
            db.ItensPedido.RemoveRange(
                pedidoBanco.Itens
            );


            // Insere itens atualizados
            pedidoBanco.Itens.Clear();

            foreach (ItemVenda item in itensVenda)
            {
                pedidoBanco.Itens.Add(
                    new ItemPedido
                    {
                        ProdutoId =
                            item.Codigo,

                        DescricaoProduto =
                            item.Descricao,

                        Quantidade =
                            item.Quantidade,

                        ValorUnitario =
                            item.Valor,

                        Total =
                            item.Total
                    }
                );
            }


            // Os GUIDs já são preenchidos no modelo; explicita que são linhas novas.
            db.ItensPedido.AddRange(pedidoBanco.Itens);

            db.LogsAuditoria.Add(
                new LogAuditoria
                {
                    PedidoId =
                        pedidoBanco.Id,

                    Acao =
                        "PEDIDO_ALTERADO",

                    Descricao =
                        $"Pedido {pedidoBanco.NumeroPedido} alterado e lacrado novamente.",

                    DataHora =
                        DateTime.Now
                }
            );


            await db.SaveChangesAsync();

            await transacao.CommitAsync();

            return pedidoBanco;
        }
        catch
        {
            await transacao.RollbackAsync();

            throw;
        }
    }


    // =========================================================
    // CANCELAR PEDIDO
    // =========================================================

    public async Task CancelarPedidoAsync(
        Guid pedidoId)
    {
        await using var db = new LocalDbContext();

        Pedido? pedido =
            await db.Pedidos
                .FirstOrDefaultAsync(
                    p => p.Id == pedidoId
                );


        if (pedido == null)
        {
            throw new Exception(
                "Pedido não encontrado."
            );
        }


        if (pedido.Cancelado)
        {
            throw new Exception(
                "Pedido já está cancelado."
            );
        }


        pedido.Cancelado = true;

        pedido.Lacrado = true;

        pedido.Sincronizado = false;

        pedido.AtualizadoEm =
            DateTime.Now;


        db.LogsAuditoria.Add(
            new LogAuditoria
            {
                PedidoId =
                    pedido.Id,

                Acao =
                    "PEDIDO_CANCELADO",

                Descricao =
                    $"Pedido {pedido.NumeroPedido} cancelado.",

                DataHora =
                    DateTime.Now
            }
        );


        await db.SaveChangesAsync();
    }
}
