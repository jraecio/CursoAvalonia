using CursoAvalonia.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class RemoteDbContextFactory
{
    private readonly ConfiguracaoService _configuracaoService;


    public RemoteDbContextFactory()
    {
        _configuracaoService =
            new ConfiguracaoService();
    }


    // =========================================================
    // CRIAR CONTEXTO SQL SERVER
    // =========================================================

    public async Task<RemoteDbContext> CriarAsync()
    {
        var configuracao =
            await _configuracaoService
                .CarregarAsync();


        if (configuracao == null)
        {
            throw new Exception(
                "Configuração do sistema não encontrada."
            );
        }


        if (string.IsNullOrWhiteSpace(
            configuracao.SqlServidor))
        {
            throw new Exception(
                "Servidor SQL Server não configurado."
            );
        }


        if (string.IsNullOrWhiteSpace(
            configuracao.SqlBanco))
        {
            throw new Exception(
                "Banco SQL Server não configurado."
            );
        }


        if (string.IsNullOrWhiteSpace(
            configuracao.SqlUsuario))
        {
            throw new Exception(
                "Usuário SQL Server não configurado."
            );
        }


        SqlConnectionStringBuilder builder =
            new SqlConnectionStringBuilder
            {
                DataSource =
                    configuracao.SqlServidor,

                InitialCatalog =
                    configuracao.SqlBanco,

                UserID =
                    configuracao.SqlUsuario,

                Password =
                    configuracao.SqlSenha,

                TrustServerCertificate =
                    true,

                Encrypt =
                    false,

                MultipleActiveResultSets =
                    true,

                ConnectTimeout =
                    10
            };


        return new RemoteDbContext(
            builder.ConnectionString
        );
    }


    // =========================================================
    // TESTAR CONEXÃO
    // =========================================================

    public async Task<bool> TestarConexaoAsync()
    {
        await using RemoteDbContext db =
            await CriarAsync();


        return await db.Database
            .CanConnectAsync();
    }
}