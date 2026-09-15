using System.Text.Json;
using System.Text.Json.Serialization;

namespace CursoAvalonia.DTOs;

public class ProdutoApiDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("produto_id")]
    public int ProdutoId { get; set; }

    [JsonPropertyName("produto_empresa_id")]
    public int ProdutoEmpresaId { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("codigo_barras")]
    public string? CodigoBarras { get; set; }

    [JsonPropertyName("referencia")]
    public string? Referencia { get; set; }

    [JsonPropertyName("unidade_medida")]
    public string? UnidadeMedida { get; set; }

    [JsonPropertyName("estoque")]
    public string? Estoque { get; set; }

    [JsonPropertyName("preco_venda")]
    public string? PrecoVenda { get; set; }

    [JsonPropertyName("preco_compra")]
    public string? PrecoCompra { get; set; }

    [JsonPropertyName("tipo_produto")]
    public string? TipoProduto { get; set; }


    // =========================================================
    // A API PODE RETORNAR ESTE CAMPO EM FORMATOS DIFERENTES
    // =========================================================

    [JsonPropertyName("sku_atributo")]
    public JsonElement? SkuAtributo { get; set; }
}