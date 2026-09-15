using System;

namespace CursoAvalonia.Models;

public class Produto
{
    // =========================================================
    // IDENTIFICAÇÃO LOCAL / API
    // =========================================================

    public int Codigo { get; set; }

    public int ProdutoEmpresaId { get; set; }


    // =========================================================
    // DADOS DO PRODUTO
    // =========================================================

    public string Descricao { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string CodigoBarras { get; set; } = string.Empty;

    public string Referencia { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public string TipoProduto { get; set; } = string.Empty;


    // =========================================================
    // VALORES
    // =========================================================

    public decimal Valor { get; set; }

    public decimal PrecoCompra { get; set; }

    public decimal Estoque { get; set; }


    // =========================================================
    // SINCRONIZAÇÃO
    // =========================================================

    public DateTime AtualizadoEm { get; set; } = DateTime.Now;
}