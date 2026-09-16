using System;
using System.Threading;
using System.Threading.Tasks;

namespace CursoAvalonia.Services;

public class InicializacaoService
{
    private readonly ConfiguracaoService _configuracaoService = new();
    private readonly SincronizacaoService _sincronizacaoService = new();
    private readonly SincronizacaoPedidosService _pedidosService = new();
    private readonly SemaphoreSlim _exclusao = new(1, 1);
    public event EventHandler<ResultadoSincronizacao>? SincronizacaoConcluida;

    public async Task InicializarAsync()
    {
        try { await SincronizarAsync(); }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Sincronização inicial: {ex.Message}");
        }
    }

    public async Task<ResultadoSincronizacao> SincronizarAsync()
    {
        await _exclusao.WaitAsync();
        var resultado = new ResultadoSincronizacao();
        try
        {
            var config = await _configuracaoService.CarregarAsync();
            if (config == null)
            {
                resultado.Erros.Add("Salve a configuração antes de sincronizar.");
                return resultado;
            }
            if (!string.IsNullOrWhiteSpace(config.BaseUrl) &&
                !string.IsNullOrWhiteSpace(config.ClientId) &&
                !string.IsNullOrWhiteSpace(config.ClientSecret))
            {
                resultado.Executada = true;
                try { resultado.Produtos = await _sincronizacaoService.SincronizarProdutosAsync(); }
                catch (Exception ex) { resultado.Erros.Add($"Produtos: {ex.Message}"); }
                try { resultado.Clientes = await _sincronizacaoService.SincronizarClientesAsync(); }
                catch (Exception ex) { resultado.Erros.Add($"Clientes: {ex.Message}"); }
            }
            if (!string.IsNullOrWhiteSpace(config.SqlServidor) &&
                !string.IsNullOrWhiteSpace(config.SqlBanco) &&
                !string.IsNullOrWhiteSpace(config.SqlUsuario))
            {
                resultado.Executada = true;
                try
                {
                    var envio = await _pedidosService.SincronizarAsync();
                    resultado.Pedidos = envio.Enviados;
                    resultado.Erros.AddRange(envio.Erros);
                }
                catch (Exception ex) { resultado.Erros.Add($"Pedidos: {ex.Message}"); }
            }
            if (!resultado.Executada)
                resultado.Erros.Add("Configure a API ou o SQL Server para sincronizar.");
            if (resultado.Executada && resultado.Erros.Count == 0)
                await _configuracaoService.AtualizarUltimaSincronizacaoAsync(DateTime.Now);
            return resultado;
        }
        finally
        {
            _exclusao.Release();
            // Atualiza a tela também quando houve sucesso parcial.
            if (resultado.Executada) SincronizacaoConcluida?.Invoke(this, resultado);
        }
    }
}
