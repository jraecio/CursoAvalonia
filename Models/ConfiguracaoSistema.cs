using System;

namespace CursoAvalonia.Models;

public class ConfiguracaoSistema
{
    // API
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string EmpresaNome { get; set; } = string.Empty;
    public string EmpresaCnpj { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;

    // AUTENTICAÇÃO
    public string Token { get; set; } = string.Empty;
    public DateTime? UltimaAutenticacao { get; set; }

    // SQL SERVER
    public string SqlServidor { get; set; } = string.Empty;
    public string SqlBanco { get; set; } = string.Empty;
    public string SqlUsuario { get; set; } = "sa";
    public string SqlSenha { get; set; } = "qaz@123";

    // SINCRONIZAÇÃO
    public DateTime? UltimaSincronizacao { get; set; }
}