using System;

namespace CursoAvalonia.Models
{
    public class ItemPedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PedidoId { get; set; }

        public int ProdutoId { get; set; }

        public string DescricaoProduto { get; set; } = string.Empty;

        public decimal Quantidade { get; set; }

        public decimal ValorUnitario { get; set; }

        public decimal Total { get; set; }

        public Pedido? Pedido { get; set; }
    }
}