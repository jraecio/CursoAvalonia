using CursoAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class SoftcomShopAuthService
{
    private readonly HttpClient _httpClient;

    public SoftcomShopAuthService()
    {
        _httpClient = new HttpClient();
    }


    // =========================================================
    // VINCULAR DISPOSITIVO
    // =========================================================

    public async Task<string> CadastrarDispositivoAsync(
        ConfiguracaoApi config)
    {
        if (string.IsNullOrWhiteSpace(config.BaseUrl))
            throw new Exception("BaseUrl não informada.");

        if (string.IsNullOrWhiteSpace(config.ClientId))
            throw new Exception("ClientId não informado.");

        if (string.IsNullOrWhiteSpace(config.EmpresaNome))
            throw new Exception("Empresa não informada.");

        if (string.IsNullOrWhiteSpace(config.EmpresaCnpj))
            throw new Exception("CNPJ não informado.");

        if (string.IsNullOrWhiteSpace(config.DeviceName))
            throw new Exception("Nome do dispositivo não informado.");

        if (string.IsNullOrWhiteSpace(config.DeviceId))
            throw new Exception("DeviceId não informado.");


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(config.BaseUrl);


        Dictionary<string, string> dados =
            new Dictionary<string, string>
            {
                ["client_id"] =
                    config.ClientId,

                ["empresa_name"] =
                    config.EmpresaNome,

                ["empresa_cnpj"] =
                    config.EmpresaCnpj,

                ["device_name"] =
                    config.DeviceName,

                ["device_id"] =
                    config.DeviceId
            };


        using FormUrlEncodedContent content =
            new FormUrlEncodedContent(dados);


        HttpResponseMessage response =
            await _httpClient.PostAsync(
                routes.Device,
                content
            );


        string retorno =
            await response.Content.ReadAsStringAsync();


        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Erro ao vincular dispositivo. HTTP {(int)response.StatusCode}: {retorno}"
            );
        }


        using JsonDocument json =
            JsonDocument.Parse(retorno);


        JsonElement root =
            json.RootElement;


        if (!root.TryGetProperty("data", out JsonElement data))
        {
            throw new Exception(
                "A API não retornou o objeto 'data'."
            );
        }


        if (!data.TryGetProperty(
                "client_secret",
                out JsonElement clientSecretElement))
        {
            throw new Exception(
                "A API não retornou o client_secret."
            );
        }


        string clientSecret =
            clientSecretElement.GetString()
            ?? string.Empty;


        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new Exception(
                "ClientSecret retornado vazio."
            );
        }


        return clientSecret;
    }


    // =========================================================
    // OBTER TOKEN
    // =========================================================

    public async Task<string> ObterTokenAsync(
        ConfiguracaoApi config)
    {
        if (string.IsNullOrWhiteSpace(config.BaseUrl))
            throw new Exception("BaseUrl não informada.");

        if (string.IsNullOrWhiteSpace(config.ClientId))
            throw new Exception("ClientId não informado.");

        if (string.IsNullOrWhiteSpace(config.ClientSecret))
            throw new Exception("ClientSecret não informado.");


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(config.BaseUrl);


        Dictionary<string, string> dados =
            new Dictionary<string, string>
            {
                ["client_secret"] =
                    config.ClientSecret,

                ["client_id"] =
                    config.ClientId,

                ["grant_type"] =
                    "client_credentials"
            };


        using FormUrlEncodedContent content =
            new FormUrlEncodedContent(dados);


        HttpResponseMessage response =
            await _httpClient.PostAsync(
                routes.Token,
                content
            );


        string retorno =
            await response.Content.ReadAsStringAsync();


        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Erro ao obter token. HTTP {(int)response.StatusCode}: {retorno}"
            );
        }


        using JsonDocument json =
            JsonDocument.Parse(retorno);


        JsonElement root =
            json.RootElement;


        if (!root.TryGetProperty(
                "data",
                out JsonElement data))
        {
            throw new Exception(
                "A API não retornou o objeto 'data'."
            );
        }


        if (!data.TryGetProperty(
                "token",
                out JsonElement tokenElement))
        {
            throw new Exception(
                "A API não retornou o token."
            );
        }


        string token =
            tokenElement.GetString()
            ?? string.Empty;


        if (string.IsNullOrWhiteSpace(token))
        {
            throw new Exception(
                "Token retornado vazio."
            );
        }


        config.UltimaAutenticacao =
            DateTime.Now;


        return token;
    }
}