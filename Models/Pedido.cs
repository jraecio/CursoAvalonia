using System;
using System.Collections.Generic;
using System.Text;

namespace CursoAvalonia.Models
{
    public class Pedido
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime Data { get; set; } = DateTime.Now;

        public decimal Total { get; set; }

        public decimal Desconto { get; set; }

        public decimal SubTotal { get; set; }

        public bool Sincronizado { get; set; } = false;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime AtualizadoEm { get; set; } = DateTime.Now;

        public List<ItemPedido> Itens { get; set; } = new();

    }
}
