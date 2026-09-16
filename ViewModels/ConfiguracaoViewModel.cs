using CommunityToolkit.Mvvm.Input;
using CursoAvalonia.Models;
using CursoAvalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace CursoAvalonia.ViewModels;

public partial class ConfiguracaoViewModel : ViewModelBase
{
    // =========================================================
    // SERVIÇOS
    // =========================================================

    private readonly SoftcomShopApiService _apiService = new();

    private readonly SoftcomShopAuthService _authService = new();

    private readonly SqlServerService _sqlService = new();

    private readonly ConfiguracaoService _configuracaoService = new();

    private readonly InicializacaoService _inicializacaoService;
    private readonly Task _carregamento;

    private readonly RemoteDbContextFactory _remoteDbFactory = new();


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public ConfiguracaoViewModel()
        : this(new InicializacaoService())
    {
    }

    public ConfiguracaoViewModel(InicializacaoService inicializacaoService)
    {
        _inicializacaoService = inicializacaoService;
        DeviceId = ObterDeviceId();

        SqlUsuario = "sa";
        SqlSenha = "qaz@123";

        _carregamento = CarregarConfiguracaoAsync();
    }


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
    // API
    // =========================================================

    private string _baseUrl = string.Empty;

    public string BaseUrl
    {
        get => _baseUrl;
        set => SetProperty(ref _baseUrl, value);
    }


    private string _clientId = string.Empty;

    public string ClientId
    {
        get => _clientId;
        set => SetProperty(ref _clientId, value);
    }


    private string _clientSecret = string.Empty;

    public string ClientSecret
    {
        get => _clientSecret;
        set => SetProperty(ref _clientSecret, value);
    }


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


    private DateTime? _ultimaAutenticacao;

    public DateTime? UltimaAutenticacao
    {
        get => _ultimaAutenticacao;
        set => SetProperty(ref _ultimaAutenticacao, value);
    }


    // =========================================================
    // STATUS API
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


    // =========================================================
    // SQL SERVER
    // =========================================================

    private string _sqlServidor = string.Empty;

    public string SqlServidor
    {
        get => _sqlServidor;
        set => SetProperty(ref _sqlServidor, value);
    }


    private string _sqlUsuario = "sa";

    public string SqlUsuario
    {
        get => _sqlUsuario;
        set => SetProperty(ref _sqlUsuario, value);
    }


    private string _sqlSenha = "qaz@123";

    public string SqlSenha
    {
        get => _sqlSenha;
        set => SetProperty(ref _sqlSenha, value);
    }


    public ObservableCollection<string> BancosSql { get; } = new();


    private string? _bancoSelecionado;

    public string? BancoSelecionado
    {
        get => _bancoSelecionado;
        set => SetProperty(ref _bancoSelecionado, value);
    }


    private string _statusSql = "DESCONECTADO";

    public string StatusSql
    {
        get => _statusSql;
        set => SetProperty(ref _statusSql, value);
    }


    private string _corStatusSql = "#6B7280";

    public string CorStatusSql
    {
        get => _corStatusSql;
        set => SetProperty(ref _corStatusSql, value);
    }


    // =========================================================
    // SINCRONIZAÇÃO
    // =========================================================

    private DateTime? _ultimaSincronizacao;

    public DateTime? UltimaSincronizacao
    {
        get => _ultimaSincronizacao;
        set => SetProperty(ref _ultimaSincronizacao, value);
    }


    // =========================================================
    // MENSAGEM
    // =========================================================

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


            UltimaAutenticacao =
                DateTime.Now;


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
    // TESTAR CONEXÃO SQL SERVER
    // =========================================================

    [RelayCommand]
    private async Task TestarConexaoSqlAsync()
    {
        await _carregamento;
        string? bancoAnterior = BancoSelecionado;
        try
        {
            StatusSql = "CONECTANDO...";
            CorStatusSql = "#F59E0B";
            var bancos = await _sqlService.ListarBancosAsync(SqlServidor, SqlUsuario, SqlSenha);
            BancosSql.Clear();
            foreach (string banco in bancos) BancosSql.Add(banco);
            if (!string.IsNullOrWhiteSpace(bancoAnterior) && !BancosSql.Contains(bancoAnterior))
                BancosSql.Add(bancoAnterior);
            BancoSelecionado = bancoAnterior;
            StatusSql = "CONECTADO";
            CorStatusSql = "#16A34A";
            Mensagem = $"Conexão realizada. {bancos.Count} banco(s) encontrado(s).";
        }
        catch (Exception ex)
        {
            StatusSql = "ERRO";
            CorStatusSql = "#DC2626";
            Mensagem = "Erro ao conectar no SQL Server: " + ex.Message;
        }
    }


    // =========================================================
    // TESTAR BANCO SELECIONADO
    // =========================================================

    
    // =========================================================
    // TESTAR BANCO SELECIONADO
    // =========================================================

    [RelayCommand]
    private async Task TestarBancoSelecionadoAsync()
    {
        await _carregamento;
        try
        {
            if (string.IsNullOrWhiteSpace(
                BancoSelecionado))
            {
                Mensagem =
                    "Selecione um banco.";

                return;
            }


            StatusSql =
                "TESTANDO BANCO...";

            CorStatusSql =
                "#F59E0B";


            // =====================================================
            // SALVAR CONFIGURAÇÃO ATUAL
            // =====================================================

            ConfiguracaoSistema config =
                new ConfiguracaoSistema
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
                        DeviceId,

                    Token =
                        Token,

                    UltimaAutenticacao =
                        UltimaAutenticacao,

                    SqlServidor =
                        SqlServidor,

                    SqlBanco =
                        BancoSelecionado,

                    SqlUsuario =
                        SqlUsuario,

                    SqlSenha =
                        SqlSenha,

                    UltimaSincronizacao =
                        UltimaSincronizacao
                };


            await _configuracaoService
                .SalvarAsync(
                    config
                );


            // =====================================================
            // TESTAR REMOTE DBCONTEXT
            // =====================================================

            bool conectado =
                await _remoteDbFactory
                    .TestarConexaoAsync();


            if (conectado)
            {
                StatusSql =
                    "BANCO CONECTADO";

                CorStatusSql =
                    "#16A34A";


                Mensagem =
                    $"RemoteDbContext conectado ao banco {BancoSelecionado}.";
            }
            else
            {
                StatusSql =
                    "SEM CONEXÃO";

                CorStatusSql =
                    "#DC2626";


                Mensagem =
                    "Não foi possível conectar ao banco SQL Server.";
            }
        }
        catch (Exception ex)
        {
            StatusSql =
                "ERRO";

            CorStatusSql =
                "#DC2626";


            Mensagem =
                "Erro no RemoteDbContext: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // SALVAR CONFIGURAÇÃO
    // =========================================================

    [RelayCommand]
    private async Task SalvarConfiguracaoAsync()
    {
        await _carregamento;
        try
        {
            ConfiguracaoSistema config =
                new ConfiguracaoSistema
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
                        DeviceId,

                    Token =
                        Token,

                    UltimaAutenticacao =
                        UltimaAutenticacao,

                    SqlServidor =
                        SqlServidor,

                    SqlBanco =
                        BancoSelecionado
                        ?? string.Empty,

                    SqlUsuario =
                        SqlUsuario,

                    SqlSenha =
                        SqlSenha,

                    UltimaSincronizacao =
                        UltimaSincronizacao
                };


            await _configuracaoService
                .SalvarAsync(
                    config
                );


            Mensagem =
                "Configuração salva com sucesso.";
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao salvar configuração: " +
                ex.Message;
        }
    }


    // =========================================================
    // CARREGAR CONFIGURAÇÃO
    // =========================================================

    private async Task CarregarConfiguracaoAsync()
    {
        try
        {
            ConfiguracaoSistema? config =
                await _configuracaoService
                    .CarregarAsync();


            if (config == null)
            {
                return;
            }


            BaseUrl =
                config.BaseUrl;

            ClientId =
                config.ClientId;

            ClientSecret =
                config.ClientSecret;

            EmpresaNome =
                config.EmpresaNome;

            EmpresaCnpj =
                config.EmpresaCnpj;

            DeviceName =
                config.DeviceName;


            if (!string.IsNullOrWhiteSpace(
                config.DeviceId))
            {
                DeviceId =
                    config.DeviceId;
            }


            Token =
                config.Token;


            UltimaAutenticacao =
                config.UltimaAutenticacao;


            SqlServidor =
                config.SqlServidor;


            SqlUsuario =
                string.IsNullOrWhiteSpace(
                    config.SqlUsuario)
                ? "sa"
                : config.SqlUsuario;


            SqlSenha =
                string.IsNullOrWhiteSpace(
                    config.SqlSenha)
                ? "qaz@123"
                : config.SqlSenha;


            BancosSql.Clear();
            if (!string.IsNullOrWhiteSpace(config.SqlBanco))
                BancosSql.Add(config.SqlBanco);
            BancoSelecionado = config.SqlBanco;


            UltimaSincronizacao =
                config.UltimaSincronizacao;


            if (!string.IsNullOrWhiteSpace(
                ClientSecret))
            {
                StatusApi =
                    "CONFIGURADO";

                CorStatusApi =
                    "#16A34A";
            }


            if (!string.IsNullOrWhiteSpace(
                    SqlServidor) &&
                !string.IsNullOrWhiteSpace(
                    BancoSelecionado))
            {
                StatusSql =
                    "CONFIGURADO";

                CorStatusSql =
                    "#2563EB";
            }


            Mensagem =
                "Configuração carregada.";
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao carregar configuração: " +
                ex.Message;
        }
    }


    // =========================================================
    // TESTAR API DE PRODUTOS
    // =========================================================

    [RelayCommand]
    private async Task TestarProdutosApiAsync()
    {
        try
        {
            Mensagem = string.Empty;

            StatusApi =
                "BUSCANDO PRODUTOS...";

            CorStatusApi =
                "#F59E0B";


            var produtos =
                await _apiService
                    .ObterProdutosAsync();


            StatusApi =
                "PRODUTOS OK";

            CorStatusApi =
                "#16A34A";


            if (produtos.Count == 0)
            {
                Mensagem =
                    "API respondeu corretamente, mas nenhum produto foi retornado.";

                return;
            }


            var primeiro =
                produtos[0];


            Mensagem =
                $"Produtos recebidos: {produtos.Count} | " +
                $"Primeiro produto: {primeiro.ProdutoId} - {primeiro.Nome} | " +
                $"Preço: {primeiro.PrecoVenda}";
        }
        catch (Exception ex)
        {
            StatusApi =
                "ERRO API";

            CorStatusApi =
                "#DC2626";


            Mensagem =
                "Erro ao buscar produtos: " +
                ex.Message;
        }
    }


    // =========================================================
    // TESTAR API DE CLIENTES
    // =========================================================

    [RelayCommand]
    private async Task TestarClientesApiAsync()
    {
        try
        {
            Mensagem = string.Empty;

            StatusApi =
                "BUSCANDO CLIENTES...";

            CorStatusApi =
                "#F59E0B";


            var clientes =
                await _apiService
                    .ObterClientesAsync();


            StatusApi =
                "CLIENTES OK";

            CorStatusApi =
                "#16A34A";


            if (clientes.Count == 0)
            {
                Mensagem =
                    "API respondeu corretamente, mas nenhum cliente foi retornado.";

                return;
            }


            var primeiro =
                clientes[0];


            Mensagem =
                $"Clientes recebidos: {clientes.Count} | " +
                $"Primeiro cliente: {primeiro.Id} - {primeiro.Nome}";
        }
        catch (Exception ex)
        {
            StatusApi =
                "ERRO API";

            CorStatusApi =
                "#DC2626";


            Mensagem =
                "Erro ao buscar clientes: " +
                ex.Message;
        }
    }


    // =========================================================
    // SINCRONIZAR API
    // =========================================================

    // =========================================================
    // SINCRONIZAR API
    // =========================================================

    [RelayCommand]
    private async Task SincronizarApiAsync()
    {
        await _carregamento;
        try
        {
            StatusApi = "SINCRONIZANDO...";
            CorStatusApi = "#F59E0B";
            // A sincronização usa exatamente os valores exibidos nesta tela.
            await PersistirConfiguracaoAtualAsync();
            var resultado = await _inicializacaoService.SincronizarAsync();
            var config = await _configuracaoService.CarregarAsync();
            if (config != null)
            {
                UltimaSincronizacao = config.UltimaSincronizacao;
                Token = config.Token;
                UltimaAutenticacao = config.UltimaAutenticacao;
            }
            StatusApi = resultado.Erros.Count == 0 ? "SINCRONIZAÇÃO OK" : "SINCRONIZAÇÃO COM PENDÊNCIAS";
            CorStatusApi = resultado.Erros.Count == 0 ? "#16A34A" : "#F59E0B";
            Mensagem = resultado.Mensagem;
        }
        catch (Exception ex)
        {
            StatusApi = "ERRO";
            CorStatusApi = "#DC2626";
            Mensagem = "Erro na sincronização: " + ex.Message;
        }
    }


    // =========================================================
    // GERAR / RECUPERAR DEVICE ID
    // =========================================================

    private static string ObterDeviceId()
    {
        string pasta =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder
                        .LocalApplicationData
                ),
                "CursoAvalonia"
            );


        Directory.CreateDirectory(
            pasta
        );


        string arquivo =
            Path.Combine(
                pasta,
                "device.id"
            );


        if (File.Exists(
            arquivo))
        {
            string deviceIdExistente =
                File.ReadAllText(
                    arquivo
                )
                .Trim();


            if (!string.IsNullOrWhiteSpace(
                deviceIdExistente))
            {
                return deviceIdExistente;
            }
        }


        string novoDeviceId =
            Guid.NewGuid()
                .ToString();


        File.WriteAllText(
            arquivo,
            novoDeviceId
        );


        return novoDeviceId;
    }


    // =========================================================
    // CRIAR CONFIGURAÇÃO API ATUAL
    // =========================================================

    private Task PersistirConfiguracaoAtualAsync()
    {
        return _configuracaoService.SalvarAsync(new ConfiguracaoSistema
        {
            BaseUrl = BaseUrl,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            EmpresaNome = EmpresaNome,
            EmpresaCnpj = EmpresaCnpj,
            DeviceName = DeviceName,
            DeviceId = DeviceId,
            Token = Token,
            UltimaAutenticacao = UltimaAutenticacao,
            SqlServidor = SqlServidor,
            SqlBanco = BancoSelecionado ?? string.Empty,
            SqlUsuario = SqlUsuario,
            SqlSenha = SqlSenha,
            UltimaSincronizacao = UltimaSincronizacao
        });
    }

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
