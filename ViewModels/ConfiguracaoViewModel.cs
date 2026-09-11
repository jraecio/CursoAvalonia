using CommunityToolkit.Mvvm.Input;
using CursoAvalonia.Models;
using CursoAvalonia.Services;
using System;
using System.Threading.Tasks;

namespace CursoAvalonia.ViewModels;

public partial class ConfiguracaoViewModel : ViewModelBase
{
    // =========================================================
    // SERVIÇOS
    // =========================================================

    private readonly SoftcomShopAuthService _authService = new();


    // =========================================================
    // URL DO DISPOSITIVO
    // =========================================================

    private string _urlDispositivo = string.Empty;

    public string UrlDispositivo
    {
        get => _urlDispositivo;
        set => SetProperty(ref _urlDispositivo, value);
    }


    // =========================================================
    // BASE URL
    // =========================================================

    private string _baseUrl = string.Empty;

    public string BaseUrl
    {
        get => _baseUrl;
        set => SetProperty(ref _baseUrl, value);
    }


    // =========================================================
    // CLIENT ID
    // =========================================================

    private string _clientId = string.Empty;

    public string ClientId
    {
        get => _clientId;
        set => SetProperty(ref _clientId, value);
    }


    // =========================================================
    // CLIENT SECRET
    // =========================================================

    private string _clientSecret = string.Empty;

    public string ClientSecret
    {
        get => _clientSecret;
        set => SetProperty(ref _clientSecret, value);
    }


    // =========================================================
    // EMPRESA
    // =========================================================

    private string _empresaNome = string.Empty;

    public string EmpresaNome
    {
        get => _empresaNome;
        set => SetProperty(ref _empresaNome, value);
    }


    private string _empresaCnpj = string.Empty;

    public string EmpresaCnpj
    {
        get => _empresaCnpj;
        set => SetProperty(ref _empresaCnpj, value);
    }


    // =========================================================
    // DISPOSITIVO
    // =========================================================

    private string _deviceName = string.Empty;

    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }


    private string _deviceId = string.Empty;

    public string DeviceId
    {
        get => _deviceId;
        set => SetProperty(ref _deviceId, value);
    }


    // =========================================================
    // TOKEN
    // =========================================================

    private string _token = string.Empty;

    public string Token
    {
        get => _token;
        set => SetProperty(ref _token, value);
    }


    // =========================================================
    // STATUS
    // =========================================================

    private string _statusApi = "NÃO CONFIGURADO";

    public string StatusApi
    {
        get => _statusApi;
        set => SetProperty(ref _statusApi, value);
    }


    private string _corStatusApi = "#6B7280";

    public string CorStatusApi
    {
        get => _corStatusApi;
        set => SetProperty(ref _corStatusApi, value);
    }


    private string _mensagem = string.Empty;

    public string Mensagem
    {
        get => _mensagem;
        set => SetProperty(ref _mensagem, value);
    }


    // =========================================================
    // PROCESSAR URL
    // =========================================================

    [RelayCommand]
    private void ProcessarUrl()
    {
        try
        {
            Mensagem = string.Empty;

            ConfiguracaoApi config =
                DevicePairingParser.Parse(
                    UrlDispositivo
                );


            BaseUrl =
                config.BaseUrl;

            ClientId =
                config.ClientId;

            EmpresaNome =
                config.EmpresaNome;

            EmpresaCnpj =
                config.EmpresaCnpj;

            DeviceName =
                config.DeviceName;


            StatusApi =
                "URL PROCESSADA";

            CorStatusApi =
                "#2563EB";


            Mensagem =
                "URL processada com sucesso.";
        }
        catch (Exception ex)
        {
            StatusApi =
                "ERRO";

            CorStatusApi =
                "#DC2626";

            Mensagem =
                ex.Message;
        }
    }


    // =========================================================
    // VINCULAR DISPOSITIVO
    // =========================================================

    [RelayCommand]
    private async Task VincularDispositivoAsync()
    {
        try
        {
            Mensagem = string.Empty;

            StatusApi =
                "VINCULANDO...";

            CorStatusApi =
                "#F59E0B";


            ConfiguracaoApi config =
                CriarConfiguracaoAtual();


            string secret =
                await _authService
                    .CadastrarDispositivoAsync(
                        config
                    );


            ClientSecret =
                secret;


            StatusApi =
                "DISPOSITIVO VINCULADO";

            CorStatusApi =
                "#16A34A";


            Mensagem =
                "Dispositivo vinculado com sucesso.";
        }
        catch (Exception ex)
        {
            StatusApi =
                "ERRO";

            CorStatusApi =
                "#DC2626";

            Mensagem =
                ex.Message;
        }
    }


    // =========================================================
    // TESTAR AUTENTICAÇÃO
    // =========================================================

    [RelayCommand]
    private async Task TestarAutenticacaoAsync()
    {
        try
        {
            Mensagem = string.Empty;

            StatusApi =
                "AUTENTICANDO...";

            CorStatusApi =
                "#F59E0B";


            ConfiguracaoApi config =
                CriarConfiguracaoAtual();


            string tokenRecebido =
                await _authService
                    .ObterTokenAsync(
                        config
                    );


            Token =
                tokenRecebido;


            StatusApi =
                "AUTENTICADO";

            CorStatusApi =
                "#16A34A";


            Mensagem =
                "Autenticação realizada com sucesso.";
        }
        catch (Exception ex)
        {
            StatusApi =
                "ERRO";

            CorStatusApi =
                "#DC2626";

            Mensagem =
                ex.Message;
        }
    }


    // =========================================================
    // CRIAR CONFIGURAÇÃO ATUAL
    // =========================================================

    private ConfiguracaoApi CriarConfiguracaoAtual()
    {
        return new ConfiguracaoApi
        {
            BaseUrl =
                BaseUrl,

            ClientId =
                ClientId,

            ClientSecret =
                ClientSecret,

            EmpresaNome =
                EmpresaNome,

            EmpresaCnpj =
                EmpresaCnpj,

            DeviceName =
                DeviceName,

            DeviceId =
                DeviceId
        };
    }
}