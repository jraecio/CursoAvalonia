using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CursoAvalonia.DTOs;

public class ClientesApiResponseDto
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("human")]
    public string? Human { get; set; }

    [JsonPropertyName("data")]
    public List<ClienteApiDto> Data { get; set; } = new();
}