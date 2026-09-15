using CursoAvalonia.DTOs;
using CursoAvalonia.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class SoftcomShopApiService
{
    // =========================================================
    // SERVIÇOS
    // =========================================================

    private readonly HttpClient _httpClient;

    private readonly SoftcomShopAuthService _authService;

    private readonly ConfiguracaoService _configuracaoService;


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public SoftcomShopApiService()
    {
        _httpClient =
            new HttpClient();

        _authService =
            new SoftcomShopAuthService();

        _configuracaoService =
            new ConfiguracaoService();
    }


    // =========================================================
    // GET GENÉRICO COM TOKEN
    // =========================================================

    private async Task<string> GetAsync(
        string url,
        bool permitirRenovarToken = true)
    {
        ConfiguracaoSistema? config =
            await _configuracaoService
                .CarregarAsync();


        if (config == null)
        {
            throw new Exception(
                "Configuração do sistema não encontrada."
            );
        }


        if (string.IsNullOrWhiteSpace(
            config.BaseUrl))
        {
            throw new Exception(
                "Base URL da API não configurada."
            );
        }


        // =====================================================
        // GARANTIR TOKEN
        // =====================================================

        if (string.IsNullOrWhiteSpace(
            config.Token))
        {
            await RenovarTokenAsync(
                config
            );
        }


        using HttpRequestMessage request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url
            );


        // =====================================================
        // BEARER TOKEN
        // =====================================================

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                config.Token
            );


        // =====================================================
        // VERSÃO DA API
        // =====================================================

        request.Headers.Add(
            "Api-Version",
            "v2"
        );


        HttpResponseMessage response =
            await _httpClient
                .SendAsync(
                    request
                );


        // =====================================================
        // TOKEN EXPIRADO
        // =====================================================

        if (response.StatusCode ==
                HttpStatusCode.Unauthorized &&
            permitirRenovarToken)
        {
            response.Dispose();


            // GERA TOKEN NOVO
            await RenovarTokenAsync(
                config
            );


            // REPETE SOMENTE UMA VEZ
            return await GetAsync(
                url,
                false
            );
        }


        string retorno =
            await response.Content
                .ReadAsStringAsync();


        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Erro na API. HTTP {(int)response.StatusCode}: {retorno}"
            );
        }


        return retorno;
    }


    // =========================================================
    // RENOVAR TOKEN
    // =========================================================

    private async Task RenovarTokenAsync(
        ConfiguracaoSistema config)
    {
        if (string.IsNullOrWhiteSpace(
                config.ClientId) ||
            string.IsNullOrWhiteSpace(
                config.ClientSecret))
        {
            throw new Exception(
                "ClientId ou ClientSecret não configurados."
            );
        }


        ConfiguracaoApi configApi =
            new ConfiguracaoApi
            {
                BaseUrl =
                    config.BaseUrl,

                ClientId =
                    config.ClientId,

                ClientSecret =
                    config.ClientSecret,

                EmpresaNome =
                    config.EmpresaNome,

                EmpresaCnpj =
                    config.EmpresaCnpj,

                DeviceName =
                    config.DeviceName,

                DeviceId =
                    config.DeviceId
            };


        string novoToken =
            await _authService
                .ObterTokenAsync(
                    configApi
                );


        config.Token =
            novoToken;


        config.UltimaAutenticacao =
            DateTime.Now;


        await _configuracaoService
            .SalvarAsync(
                config
            );
    }


    // =========================================================
    // EMPRESA
    // =========================================================

    public async Task<string>
        ObterEmpresaJsonAsync()
    {
        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );


        return await GetAsync(
            routes.Empresa
        );
    }


    // =========================================================
    // PRODUTOS V2 - JSON BRUTO
    // =========================================================

    public async Task<string>
        ObterProdutosJsonAsync()
    {
        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );


        return await GetAsync(
            routes.ProdutosV2
        );
    }


    // =========================================================
    // PRODUTOS V2 - DESSERIALIZADOS / TODAS AS PÁGINAS
    // =========================================================

    public async Task<List<ProdutoApiDto>>
        ObterProdutosAsync()
    {
        List<ProdutoApiDto> produtos =
            new List<ProdutoApiDto>();


        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );


        int paginaAtual =
            1;

        int ultimaPagina =
            1;


        do
        {
            // =================================================
            // MONTA URL COM PAGINAÇÃO
            // =================================================

            string separador =
                routes.ProdutosV2.Contains("?")
                    ? "&"
                    : "?";


            string url =
                $"{routes.ProdutosV2}{separador}page={paginaAtual}";


            // =================================================
            // BUSCA PÁGINA
            // =================================================

            string json =
                await GetAsync(
                    url
                );


            // =================================================
            // DESSERIALIZA
            // =================================================

            ProdutosApiResponseDto? response =
                JsonSerializer
                    .Deserialize<ProdutosApiResponseDto>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive =
                                true
                        }
                    );


            if (response == null)
            {
                break;
            }


            // =================================================
            // ADICIONA PRODUTOS
            // =================================================

            if (response.Data != null &&
                response.Data.Count > 0)
            {
                produtos.AddRange(
                    response.Data
                );
            }


            // =================================================
            // CONTROLE DA PAGINAÇÃO
            // =================================================

            if (response.CurrentPage > 0)
            {
                paginaAtual =
                    response.CurrentPage + 1;
            }
            else
            {
                paginaAtual++;
            }


            if (response.LastPage > 0)
            {
                ultimaPagina =
                    response.LastPage;
            }
            else
            {
                ultimaPagina =
                    response.CurrentPage > 0
                        ? response.CurrentPage
                        : 1;
            }


        }
        while (paginaAtual <= ultimaPagina);


        return produtos;
    }


    // =========================================================
    // PRODUTOS V1
    // =========================================================

    public async Task<string>
        ObterProdutosV1JsonAsync()
    {
        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );


        return await GetAsync(
            routes.Produtos
        );
    }


    // =========================================================
    // PROMOÇÕES
    // =========================================================

    public async Task<string>
        ObterPromocaoJsonAsync()
    {
        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );


        return await GetAsync(
            routes.Promocao
        );
    }


    // =========================================================
    // VENDAS
    // =========================================================

    public async Task<string>
        ObterVendasJsonAsync()
    {
        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();


        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );


        return await GetAsync(
            routes.Vendas
        );
    }


    // =========================================================
    // CONFIGURAÇÃO OBRIGATÓRIA
    // =========================================================

    private async Task<ConfiguracaoSistema>
        ObterConfiguracaoObrigatoriaAsync()
    {
        ConfiguracaoSistema? config =
            await _configuracaoService
                .CarregarAsync();


        if (config == null)
        {
            throw new Exception(
                "Configuração não encontrada."
            );
        }


        if (string.IsNullOrWhiteSpace(
            config.BaseUrl))
        {
            throw new Exception(
                "Base URL não configurada."
            );
        }


        return config;
    }
    public async Task<string> ObterClientesJsonAsync()
    {
        ConfiguracaoSistema config =
            await ObterConfiguracaoObrigatoriaAsync();

        SoftcomShopRoutes routes =
            new SoftcomShopRoutes(
                config.BaseUrl
            );

        return await GetAsync(
            routes.Clientes
        );


    }
    // =========================================================
    // CLIENTES - DESSERIALIZADOS
    // =========================================================

    public async Task<List<ClienteApiDto>> ObterClientesAsync()
    {
        string json =
            await ObterClientesJsonAsync();

        ClientesApiResponseDto? response =
            JsonSerializer.Deserialize<ClientesApiResponseDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

        if (response == null)
        {
            return new List<ClienteApiDto>();
        }

        return response.Data;
    }

}