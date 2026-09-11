using System;

namespace CursoAvalonia.Models;

public class ConfiguracaoApi
{
    public int Id { get; set; }

    public string BaseUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string EmpresaNome { get; set; } = string.Empty;

    public string EmpresaCnpj { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;

    public DateTime? UltimaAutenticacao { get; set; }
}