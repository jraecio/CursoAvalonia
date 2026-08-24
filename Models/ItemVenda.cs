using System;
using System.Collections.Generic;
using System.Text;

namespace CursoAvalonia.Models
{
    public class ItemVenda
    {
        public int Codigo { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public decimal Quantidade { get; set; }

        public decimal Valor { get; set; }

        public decimal Total
        {
            get
            {
                return Quantidade * Valor;
            }
        }
    }
}
