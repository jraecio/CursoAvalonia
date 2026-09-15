using CursoAvalonia.Data;
using CursoAvalonia.DTOs;
using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class SincronizacaoService
{
    private readonly SoftcomShopApiService _apiService;

    public SincronizacaoService()
    {
        _apiService = new SoftcomShopApiService();
    }

    // =========================================================
    // SINCRONIZAR TUDO
    // =========================================================

    public async Task<(int Produtos, int Clientes)> SincronizarTudoAsync()
    {
        int produtos =
            await SincronizarProdutosAsync();

        int clientes =
            await SincronizarClientesAsync();

        return (produtos, clientes);
    }

    // =========================================================
    // SINCRONIZAR PRODUTOS
    // =========================================================

    public async Task<int> SincronizarProdutosAsync()
    {
        var produtosApi =
            await _apiService
                .ObterProdutosAsync();

        if (produtosApi.Count == 0)
            return 0;

        await using LocalDbContext db =
            new LocalDbContext();

        var produtosLocais =
            await db.Produtos
                .ToDictionaryAsync(
                    p => p.Codigo
                );

        int quantidade = 0;

        foreach (ProdutoApiDto produtoApi in produtosApi)
        {
            if (produtoApi.ProdutoId <= 0)
                continue;

            Produto produto;

            if (produtosLocais.TryGetValue(
                produtoApi.ProdutoId,
                out Produto? existente))
            {
                produto = existente;
            }
            else
            {
                produto = new Produto
                {
                    Codigo =
                        produtoApi.ProdutoId
                };

                db.Produtos.Add(produto);

                produtosLocais.Add(
                    produto.Codigo,
                    produto
                );
            }

            produto.ProdutoEmpresaId =
                produtoApi.ProdutoEmpresaId;

            produto.Descricao =
                produtoApi.Nome ?? string.Empty;

            produto.Sku =
                produtoApi.Sku ?? string.Empty;

            produto.CodigoBarras =
                produtoApi.CodigoBarras ?? string.Empty;

            produto.Referencia =
                produtoApi.Referencia ?? string.Empty;

            produto.UnidadeMedida =
                produtoApi.UnidadeMedida ?? string.Empty;

            produto.TipoProduto =
                produtoApi.TipoProduto ?? string.Empty;

            produto.Valor =
                ConverterDecimal(
                    produtoApi.PrecoVenda
                );

            produto.PrecoCompra =
                ConverterDecimal(
                    produtoApi.PrecoCompra
                );

            produto.Estoque =
                ConverterDecimal(
                    produtoApi.Estoque
                );

            produto.AtualizadoEm =
                DateTime.Now;

            quantidade++;
        }

        await db.SaveChangesAsync();

        return quantidade;
    }

    // =========================================================
    // SINCRONIZAR CLIENTES
    // =========================================================

    public async Task<int> SincronizarClientesAsync()
    {
        var clientesApi =
            await _apiService
                .ObterClientesAsync();

        if (clientesApi.Count == 0)
            return 0;

        await using LocalDbContext db =
            new LocalDbContext();

        var clientesLocais =
            await db.Clientes
                .ToDictionaryAsync(
                    c => c.Id
                );

        int quantidade = 0;

        foreach (ClienteApiDto clienteApi in clientesApi)
        {
            if (clienteApi.Id <= 0)
                continue;

            Cliente cliente;

            if (clientesLocais.TryGetValue(
                clienteApi.Id,
                out Cliente? existente))
            {
                cliente = existente;
            }
            else
            {
                cliente = new Cliente
                {
                    Id =
                        clienteApi.Id
                };

                db.Clientes.Add(cliente);

                clientesLocais.Add(
                    cliente.Id,
                    cliente
                );
            }

            cliente.Nome =
                clienteApi.Nome ?? string.Empty;

            cliente.CPF =
                clienteApi.CpfCnpj ?? string.Empty;

            cliente.Pessoa =
                clienteApi.Pessoa ?? string.Empty;

            cliente.RazaoSocial =
                clienteApi.RazaoSocial ?? string.Empty;

            cliente.InscricaoEstadual =
                clienteApi.InscricaoEstadual ?? string.Empty;

            cliente.Rg =
                clienteApi.Rg ?? string.Empty;

            cliente.Endereco =
                clienteApi.Endereco ?? string.Empty;

            cliente.Numero =
                JsonParaTexto(
                    clienteApi.Numero
                );

            cliente.Complemento =
                clienteApi.Complemento ?? string.Empty;

            cliente.Bairro =
                clienteApi.Bairro ?? string.Empty;

            cliente.Cidade =
                clienteApi.Cidade ?? string.Empty;

            cliente.Uf =
                clienteApi.Uf ?? string.Empty;

            cliente.Cep =
                clienteApi.Cep ?? string.Empty;

            cliente.Telefone =
                MontarTelefone(
                    clienteApi.ContatoDdd,
                    clienteApi.ContatoTelefone
                );

            cliente.Email =
                clienteApi.ContatoEmail ?? string.Empty;

            cliente.TipoCliente =
                clienteApi.TipoClienteNome ?? string.Empty;

            cliente.Bloqueado =
                ConverterBooleanoApi(
                    clienteApi.Bloqueado
                );

            cliente.LimiteCredito =
                ConverterDecimalJson(
                    clienteApi.LimiteCredito
                );

            cliente.CreditoSaldoDisponivel =
                ConverterDecimalJson(
                    clienteApi.CreditoSaldoDisponivel
                );

            cliente.AtualizadoEm =
                DateTime.Now;

            quantidade++;
        }

        await db.SaveChangesAsync();

        return quantidade;
    }

    // =========================================================
    // DECIMAL STRING
    // =========================================================

    private static decimal ConverterDecimal(
        string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return 0;

        if (decimal.TryParse(
            valor,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out decimal resultado))
        {
            return resultado;
        }

        return 0;
    }

    // =========================================================
    // JSON -> TEXTO
    // =========================================================

    private static string JsonParaTexto(
        JsonElement? elemento)
    {
        if (elemento == null)
            return string.Empty;

        JsonElement valor =
            elemento.Value;

        return valor.ValueKind switch
        {
            JsonValueKind.String =>
                valor.GetString()
                ?? string.Empty,

            JsonValueKind.Number =>
                valor.ToString(),

            JsonValueKind.True =>
                "1",

            JsonValueKind.False =>
                "0",

            _ =>
                string.Empty
        };
    }

    // =========================================================
    // JSON -> DECIMAL
    // =========================================================

    private static decimal ConverterDecimalJson(
        JsonElement? elemento)
    {
        string valor =
            JsonParaTexto(
                elemento
            );

        return ConverterDecimal(
            valor
        );
    }

    // =========================================================
    // JSON -> BOOLEANO
    // =========================================================

    private static bool ConverterBooleanoApi(
        JsonElement? elemento)
    {
        if (elemento == null)
            return false;

        JsonElement valor =
            elemento.Value;

        if (valor.ValueKind ==
            JsonValueKind.True)
        {
            return true;
        }

        if (valor.ValueKind ==
            JsonValueKind.False)
        {
            return false;
        }

        string texto =
            JsonParaTexto(
                elemento
            );

        return texto == "1" ||
               texto.Equals(
                   "true",
                   StringComparison.OrdinalIgnoreCase
               ) ||
               texto.Equals(
                   "sim",
                   StringComparison.OrdinalIgnoreCase
               );
    }

    // =========================================================
    // TELEFONE
    // =========================================================

    private static string MontarTelefone(
        JsonElement? ddd,
        JsonElement? telefone)
    {
        string valorDdd =
            JsonParaTexto(
                ddd
            );

        string valorTelefone =
            JsonParaTexto(
                telefone
            );

        if (string.IsNullOrWhiteSpace(
                valorDdd))
        {
            return valorTelefone;
        }

        if (string.IsNullOrWhiteSpace(
                valorTelefone))
        {
            return valorDdd;
        }

        return $"({valorDdd}) {valorTelefone}";
    }
}