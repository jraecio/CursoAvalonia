using System;
using System.Collections.Generic;

namespace CursoAvalonia.Models
{
    public class Pedido
    {
        // Chave interna única
        public Guid Id { get; set; } = Guid.NewGuid();

        // Número visível do pedido
        public int NumeroPedido { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;

        public decimal Total { get; set; }

        public decimal Desconto { get; set; }

        public decimal SubTotal { get; set; }


        // =====================================================
        // STATUS DO PEDIDO
        // =====================================================

        // Ao finalizar a venda, o pedido fica lacrado
        public bool Lacrado { get; set; } = true;

        // Pedido cancelado nunca mais poderá ser editado
        public bool Cancelado { get; set; } = false;


        // =====================================================
        // SINCRONIZAÇÃO
        // =====================================================

        public bool Sincronizado { get; set; } = false;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime AtualizadoEm { get; set; } = DateTime.Now;


        // =====================================================
        // ITENS
        // =====================================================

        public List<ItemPedido> Itens { get; set; } = new();
    }
}