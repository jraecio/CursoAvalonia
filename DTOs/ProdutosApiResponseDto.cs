using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CursoAvalonia.DTOs;

public class ProdutosApiResponseDto
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("data")]
    public List<ProdutoApiDto> Data { get; set; } = new();
}