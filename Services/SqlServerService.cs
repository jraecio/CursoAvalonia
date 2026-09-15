using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class SqlServerService
{
    public async Task<List<string>> ListarBancosAsync(
        string servidor,
        string usuario,
        string senha)
    {
        if (string.IsNullOrWhiteSpace(servidor))
            throw new Exception("Informe o servidor SQL Server.");

        if (string.IsNullOrWhiteSpace(usuario))
            throw new Exception("Informe o usuário do SQL Server.");

        if (string.IsNullOrWhiteSpace(senha))
            throw new Exception("Informe a senha do SQL Server.");

        string connectionString =
            $"Server={servidor};" +
            $"Database=master;" +
            $"User Id={usuario};" +
            $"Password={senha};" +
            $"TrustServerCertificate=True;" +
            $"Encrypt=False;" +
            $"Connect Timeout=5;";

        List<string> bancos = new();

        await using SqlConnection connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync();

        const string sql =
            """
            SELECT name
            FROM sys.databases
            WHERE state = 0
              AND database_id > 4
            ORDER BY name
            """;

        await using SqlCommand command =
            new SqlCommand(sql, connection);

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            bancos.Add(reader.GetString(0));
        }

        return bancos;
    }

    public async Task<bool> TestarBancoAsync(
        string servidor,
        string banco,
        string usuario,
        string senha)
    {
        if (string.IsNullOrWhiteSpace(banco))
            throw new Exception("Selecione um banco de dados.");

        string connectionString =
            $"Server={servidor};" +
            $"Database={banco};" +
            $"User Id={usuario};" +
            $"Password={senha};" +
            $"TrustServerCertificate=True;" +
            $"Encrypt=False;" +
            $"Connect Timeout=5;";

        await using SqlConnection connection =
            new SqlConnection(connectionString);

        await connection.OpenAsync();

        return true;
    }
}