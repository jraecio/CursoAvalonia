using System;
using System.Collections.Generic;
using System.Text;

namespace CursoAvalonia.Models
{
    public class LogAuditoria
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PedidoId { get; set; }

        public string Acao { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public DateTime DataHora { get; set; } = DateTime.Now;

        public Pedido? Pedido { get; set; }
    }
}
