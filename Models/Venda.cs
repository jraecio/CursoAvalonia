using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CursoAvalonia.Models
{
    public class Venda
    {
        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();

        public decimal Desconto { get; set; }

        public decimal SubTotal
        {
            get
            {
                return Itens.Sum(item => item.Total);
            }
        }

        public decimal Total
        {
            get
            {
                return SubTotal - Desconto;
            }
        }
    }
}
