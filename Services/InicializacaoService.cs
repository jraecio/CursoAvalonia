using CursoAvalonia.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class InicializacaoService
{
    private readonly ConfiguracaoService _configuracaoService;
    private readonly SincronizacaoService _sincronizacaoService;

    public event EventHandler? SincronizacaoConcluida;


    public InicializacaoService()
    {
        _configuracaoService =
            new ConfiguracaoService();

        _sincronizacaoService =
            new SincronizacaoService();
    }


    // =========================================================
    // INICIALIZAR SISTEMA
    // =========================================================

    public async Task InicializarAsync()
    {
        // =====================================================
        // BANCO LOCAL
        // =====================================================

        await VerificarBancoLocalAsync();


        // =====================================================
        // CONFIGURAÇÃO
        // =====================================================

        var configuracao =
            await _configuracaoService
                .CarregarAsync();


        if (configuracao == null)
            return;


        // =====================================================
        // API CONFIGURADA?
        // =====================================================

        if (string.IsNullOrWhiteSpace(
                configuracao.BaseUrl) ||
            string.IsNullOrWhiteSpace(
                configuracao.ClientId) ||
            string.IsNullOrWhiteSpace(
                configuracao.ClientSecret))
        {
            return;
        }


        // =====================================================
        // SINCRONIZAÇÃO
        // =====================================================

        try
        {
            await _sincronizacaoService
                .SincronizarTudoAsync();


            configuracao.UltimaSincronizacao =
                DateTime.Now;


            await _configuracaoService
                .SalvarAsync(
                    configuracao
                );


            // =================================================
            // AVISA QUE O SQLITE FOI ATUALIZADO
            // =================================================

            SincronizacaoConcluida?.Invoke(
                this,
                EventArgs.Empty
            );
        }
        catch
        {
            // A aplicação continua trabalhando
            // normalmente com os dados locais.
        }
    }


    // =========================================================
    // VERIFICAR BANCO LOCAL
    // =========================================================

    private static async Task VerificarBancoLocalAsync()
    {
        await using LocalDbContext db =
            new LocalDbContext();

        await db.Database
            .CanConnectAsync();
    }
}