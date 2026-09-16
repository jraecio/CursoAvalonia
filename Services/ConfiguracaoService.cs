using CursoAvalonia.Models;
using System;
using System.IO;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class ConfiguracaoService
{
    private static readonly SemaphoreSlim Exclusao = new(1, 1);
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
        await Exclusao.WaitAsync();
        try { await SalvarArquivoAsync(config); }
        finally { Exclusao.Release(); }
    }

    private async Task SalvarArquivoAsync(ConfiguracaoSistema config)
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


        string temporario = _arquivoConfiguracao + ".tmp";
        await File.WriteAllTextAsync(temporario, json);
        File.Move(temporario, _arquivoConfiguracao, true);
    }


    // =========================================================
    // CARREGAR CONFIGURAÇÃO
    // =========================================================

    public async Task<ConfiguracaoSistema?>
        CarregarAsync()
    {
        await Exclusao.WaitAsync();
        try { return await CarregarArquivoAsync(); }
        finally { Exclusao.Release(); }
    }

    public async Task AtualizarUltimaSincronizacaoAsync(DateTime data)
    {
        await Exclusao.WaitAsync();
        try
        {
            var config = await CarregarArquivoAsync();
            if (config == null) return;
            config.UltimaSincronizacao = data;
            await SalvarArquivoAsync(config);
        }
        finally { Exclusao.Release(); }
    }

    private async Task<ConfiguracaoSistema?> CarregarArquivoAsync()
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
