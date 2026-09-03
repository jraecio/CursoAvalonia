using System;
using System.Collections.Generic;
using System.Text;

namespace CursoAvalonia.Models
{
    public class Produto
    {
        public int Codigo { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public decimal Valor { get; set; }
    }
}
