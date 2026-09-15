using CursoAvalonia.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace CursoAvalonia.Data;

public class RemoteDbContextDesignFactory
    : IDesignTimeDbContextFactory<RemoteDbContext>
{
    public RemoteDbContext CreateDbContext(string[] args)
    {
        ConfiguracaoService configuracaoService =
            new ConfiguracaoService();

        var configuracao =
            configuracaoService
                .CarregarAsync()
                .GetAwaiter()
                .GetResult();


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
}