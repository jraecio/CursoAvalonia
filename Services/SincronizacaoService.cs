using CursoAvalonia.Data;
using CursoAvalonia.DTOs;
using CursoAvalonia.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class SincronizacaoService
{
    private readonly SoftcomShopApiService _apiService;


    public SincronizacaoService()
    {
        _apiService =
            new SoftcomShopApiService();
    }


    // =========================================================
    // SINCRONIZAR PRODUTOS
    // =========================================================

    public async Task<int> SincronizarProdutosAsync()
    {
        // =====================================================
        // BUSCA PRODUTOS NA API
        // =====================================================

        var produtosApi =
            await _apiService
                .ObterProdutosAsync();


        if (produtosApi.Count == 0)
        {
            return 0;
        }


        await using LocalDbContext db =
            new LocalDbContext();


        // =====================================================
        // PRODUTOS JÁ EXISTENTES
        // =====================================================

        var produtosLocais =
            await db.Produtos
                .ToDictionaryAsync(
                    p => p.Codigo
                );


        int quantidadeAtualizada = 0;


        // =====================================================
        // INSERT / UPDATE
        // =====================================================

        foreach (ProdutoApiDto produtoApi in produtosApi)
        {
            if (produtoApi.ProdutoId <= 0)
            {
                continue;
            }


            Produto produto;


            // =================================================
            // PRODUTO JÁ EXISTE
            // =================================================

            if (produtosLocais.TryGetValue(
                produtoApi.ProdutoId,
                out Produto? produtoExistente))
            {
                produto =
                    produtoExistente;
            }
            else
            {
                // =============================================
                // PRODUTO NOVO
                // =============================================

                produto =
                    new Produto
                    {
                        Codigo =
                            produtoApi.ProdutoId
                    };


                db.Produtos.Add(
                    produto
                );


                produtosLocais.Add(
                    produto.Codigo,
                    produto
                );
            }


            // =================================================
            // ATUALIZAR DADOS
            // =================================================

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


            quantidadeAtualizada++;
        }


        // =====================================================
        // SALVAR NO SQLITE
        // =====================================================

        await db.SaveChangesAsync();


        return quantidadeAtualizada;
    }


    // =========================================================
    // CONVERSÃO DECIMAL API
    // =========================================================

    private static decimal ConverterDecimal(
        string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return 0;
        }


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
}