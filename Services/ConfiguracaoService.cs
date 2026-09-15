using CursoAvalonia.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class ConfiguracaoService
{
    private readonly string _pastaConfiguracao;
    private readonly string _arquivoConfiguracao;

    public ConfiguracaoService()
    {
        _pastaConfiguracao = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            ),
            "CursoAvalonia"
        );

        _arquivoConfiguracao = Path.Combine(
            _pastaConfiguracao,
            "config.json"
        );
    }


    // =========================================================
    // SALVAR CONFIGURAÇÃO
    // =========================================================

    public async Task SalvarAsync(
        ConfiguracaoSistema config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(
                nameof(config)
            );
        }

        Directory.CreateDirectory(
            _pastaConfiguracao
        );


        JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };


        string json =
            JsonSerializer.Serialize(
                config,
                options
            );


        await File.WriteAllTextAsync(
            _arquivoConfiguracao,
            json
        );
    }


    // =========================================================
    // CARREGAR CONFIGURAÇÃO
    // =========================================================

    public async Task<ConfiguracaoSistema?>
        CarregarAsync()
    {
        if (!File.Exists(
            _arquivoConfiguracao))
        {
            return null;
        }


        string json =
            await File.ReadAllTextAsync(
                _arquivoConfiguracao
            );


        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }


        ConfiguracaoSistema? config =
            JsonSerializer.Deserialize
            <ConfiguracaoSistema>(
                json
            );


        return config;
    }


    // =========================================================
    // VERIFICAR SE EXISTE CONFIGURAÇÃO
    // =========================================================

    public bool ExisteConfiguracao()
    {
        return File.Exists(
            _arquivoConfiguracao
        );
    }


    // =========================================================
    // CAMINHO DO ARQUIVO
    // =========================================================

    public string ObterCaminhoConfiguracao()
    {
        return _arquivoConfiguracao;
    }
}