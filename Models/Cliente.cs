using System;
using System.Collections.Generic;
using System.Text;

namespace CursoAvalonia.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string CPF { get; set; } = string.Empty;
    }
}
