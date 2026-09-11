using System;
using System.Collections.Generic;

namespace CursoAvalonia.Models
{
    public class Pedido
    {
        // =====================================================
        // IDENTIFICAÇÃO
        // =====================================================

        public Guid Id { get; set; } = Guid.NewGuid();

        public int NumeroPedido { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;


        // =====================================================
        // CLIENTE
        // =====================================================

        public int? ClienteId { get; set; }


        // =====================================================
        // TOTAIS
        // =====================================================

        public decimal Total { get; set; }

        public decimal Desconto { get; set; }

        public decimal SubTotal { get; set; }


        // =====================================================
        // PAGAMENTO
        // =====================================================

        public string FormaPagamento { get; set; } = "Pix";

        public decimal ValorPago { get; set; }

        public decimal Troco { get; set; }


        // =====================================================
        // STATUS DO PEDIDO
        // =====================================================

        public bool Lacrado { get; set; } = true;

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