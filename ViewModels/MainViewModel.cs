using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursoAvalonia.Models;
using CursoAvalonia.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CursoAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // =========================================================
    // CAMPOS DO PRODUTO
    // =========================================================

    [ObservableProperty]
    private string codigo = string.Empty;

    [ObservableProperty]
    private string descricao = string.Empty;

    [ObservableProperty]
    private string quantidade = "1";

    [ObservableProperty]
    private string valor = string.Empty;


    // =========================================================
    // PAGAMENTO
    // =========================================================

    public ObservableCollection<string> FormasPagamento { get; } = new()
    {
        "Pix",
        "Cartão de Crédito",
        "Cartão de Débito",
        "Espécie"
    };


    private string _formaPagamento = "Pix";

    public string FormaPagamento
    {
        get => _formaPagamento;

        set
        {
            if (SetProperty(ref _formaPagamento, value))
            {
                AtualizarPagamento();
            }
        }
    }


    private string _valorPago = string.Empty;

    public string ValorPago
    {
        get => _valorPago;

        set
        {
            if (SetProperty(ref _valorPago, value))
            {
                AtualizarTroco();
            }
        }
    }


    private decimal _troco;

    public decimal Troco
    {
        get => _troco;
        set => SetProperty(ref _troco, value);
    }


    // =========================================================
    // ITENS DA VENDA
    // =========================================================

    public ObservableCollection<ItemVenda> Itens { get; } = new();

    [ObservableProperty]
    private ItemVenda? itemSelecionado;


    // =========================================================
    // PRODUTOS
    // =========================================================

    public ObservableCollection<Produto> Produtos { get; } = new();


    private Produto? _produtoSelecionado;

    public Produto? ProdutoSelecionado
    {
        get => _produtoSelecionado;

        set
        {
            if (SetProperty(ref _produtoSelecionado, value))
            {
                if (value == null)
                    return;


                Codigo =
                    value.Codigo.ToString();

                Descricao =
                    value.Descricao;

                Valor =
                    value.Valor.ToString("0.00");
            }
        }
    }


    // =========================================================
    // CLIENTES
    // =========================================================

    public ObservableCollection<Cliente> Clientes { get; } = new();


    [ObservableProperty]
    private Cliente? clienteSelecionado;


    // =========================================================
    // PEDIDOS
    // =========================================================

    public ObservableCollection<Pedido> Pedidos { get; } = new();


    private Pedido? _pedidoSelecionado;

    public Pedido? PedidoSelecionado
    {
        get => _pedidoSelecionado;

        set
        {
            if (SetProperty(ref _pedidoSelecionado, value))
            {
                CarregarPedido(value);

                OnPropertyChanged(
                    nameof(PodeFinalizarNovaVenda)
                );
            }
        }
    }


    // =========================================================
    // CONTROLE DE EDIÇÃO
    // =========================================================

    private bool _podeEditar = true;

    public bool PodeEditar
    {
        get => _podeEditar;
        set => SetProperty(ref _podeEditar, value);
    }


    private bool _emAlteracao;

    public bool EmAlteracao
    {
        get => _emAlteracao;

        set
        {
            if (SetProperty(ref _emAlteracao, value))
            {
                OnPropertyChanged(
                    nameof(PodeFinalizarNovaVenda)
                );
            }
        }
    }


    public bool PodeFinalizarNovaVenda =>
        PedidoSelecionado == null &&
        !EmAlteracao;


    private string _statusPedido = "NOVO PEDIDO";

    public string StatusPedido
    {
        get => _statusPedido;
        set => SetProperty(ref _statusPedido, value);
    }


    private string _corStatusPedido = "#2563EB";

    public string CorStatusPedido
    {
        get => _corStatusPedido;
        set => SetProperty(ref _corStatusPedido, value);
    }


    // =========================================================
    // TOTAIS / MENSAGEM
    // =========================================================

    [ObservableProperty]
    private string mensagem = string.Empty;

    [ObservableProperty]
    private string desconto = string.Empty;

    [ObservableProperty]
    private decimal subTotal;

    [ObservableProperty]
    private decimal total;


    // =========================================================
    // SERVIÇOS
    // =========================================================

    private readonly VendaService _vendaService =
        new();

    private readonly DadosLocaisService _dadosLocaisService =
        new();

    private readonly InicializacaoService _inicializacaoService;
    public InicializacaoService InicializacaoService => _inicializacaoService;


    // =========================================================
    // CONSTRUTOR PADRÃO
    // =========================================================

    public MainViewModel()
        : this(new InicializacaoService())
    {
    }


    // =========================================================
    // CONSTRUTOR COM INICIALIZAÇÃO COMPARTILHADA
    // =========================================================

    public MainViewModel(
        InicializacaoService inicializacaoService)
    {
        _inicializacaoService =
            inicializacaoService;


        // =====================================================
        // QUANDO A API TERMINAR DE SINCRONIZAR,
        // RECARREGA PRODUTOS E CLIENTES DO SQLITE
        // =====================================================

        _inicializacaoService
            .SincronizacaoConcluida +=
            InicializacaoService_SincronizacaoConcluida;


        // =====================================================
        // CARREGAMENTO LOCAL IMEDIATO
        // =====================================================

        _ = CarregarDadosIniciaisAsync();
    }


    // =========================================================
    // SINCRONIZAÇÃO CONCLUÍDA
    // =========================================================

    private async void
        InicializacaoService_SincronizacaoConcluida(
            object? sender,
            ResultadoSincronizacao e)
    {
        try
        {
            // ObservableCollection deve ser atualizada
            // na thread da interface.

            await Dispatcher.UIThread.InvokeAsync(
                async () =>
                {
                    await CarregarProdutosDoBancoAsync();

                    await CarregarClientesDoBancoAsync();

                    Mensagem =
                        e.Mensagem;
                }
            );
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao atualizar dados após sincronização: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // CARREGAR DADOS INICIAIS
    // =========================================================

    private async Task CarregarDadosIniciaisAsync()
    {
        try
        {
            await CarregarProdutosDoBancoAsync();

            await CarregarClientesDoBancoAsync();

            await CarregarPedidosDoBancoAsync();
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao carregar dados locais: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // CARREGAR PRODUTOS DO SQLITE
    // =========================================================

    private async Task CarregarProdutosDoBancoAsync()
    {
        var produtosBanco =
            await _dadosLocaisService
                .ListarProdutosAsync();

        int? codigoSelecionado = ProdutoSelecionado?.Codigo;
        string codigoDigitado = Codigo;
        string descricaoDigitada = Descricao;
        string valorDigitado = Valor;


        Produtos.Clear();


        foreach (Produto produto in produtosBanco)
        {
            Produtos.Add(
                produto
            );
        }
        ProdutoSelecionado = Produtos.FirstOrDefault(p => p.Codigo == codigoSelecionado);
        Codigo = codigoDigitado;
        Descricao = descricaoDigitada;
        Valor = valorDigitado;
    }


    // =========================================================
    // CARREGAR CLIENTES DO SQLITE
    // =========================================================

    private async Task CarregarClientesDoBancoAsync()
    {
        var clientesBanco =
            await _dadosLocaisService
                .ListarClientesAsync();

        int? clienteId = ClienteSelecionado?.Id;


        Clientes.Clear();


        foreach (Cliente cliente in clientesBanco)
        {
            Clientes.Add(
                cliente
            );
        }
        ClienteSelecionado = Clientes.FirstOrDefault(c => c.Id == clienteId);
    }


    // =========================================================
    // CARREGAR PEDIDOS DO BANCO
    // =========================================================

    private async Task CarregarPedidosDoBancoAsync()
    {
        try
        {
            var pedidosBanco =
                await _vendaService
                    .ListarPedidosAsync();


            Pedidos.Clear();


            foreach (Pedido pedido in pedidosBanco)
            {
                Pedidos.Add(
                    pedido
                );
            }
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao carregar pedidos: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // NOVO PEDIDO
    // =========================================================

    [RelayCommand]
    private void NovoPedido()
    {
        PedidoSelecionado =
            null;


        LimparItensVenda();


        ClienteSelecionado =
            null;

        ProdutoSelecionado =
            null;

        ItemSelecionado =
            null;


        Codigo =
            string.Empty;

        Descricao =
            string.Empty;

        Quantidade =
            "1";

        Valor =
            string.Empty;


        Total =
            0;

        SubTotal =
            0;

        Desconto =
            string.Empty;


        FormaPagamento =
            "Pix";

        ValorPago =
            string.Empty;

        Troco =
            0;


        PodeEditar =
            true;

        EmAlteracao =
            false;


        StatusPedido =
            "NOVO PEDIDO";

        CorStatusPedido =
            "#2563EB";


        Mensagem =
            "Nova venda iniciada.";


        OnPropertyChanged(
            nameof(PodeFinalizarNovaVenda)
        );
    }


    // =========================================================
    // ADICIONAR / ALTERAR ITEM
    // =========================================================

    [RelayCommand]
    private void AdicionarItem()
    {
        if (!PodeEditar)
        {
            Mensagem =
                "Pedido não está liberado para edição.";

            return;
        }


        Mensagem =
            string.Empty;


        try
        {
            int codigoItem =
                Convert.ToInt32(
                    Codigo
                );


            decimal quantidadeItem =
                Convert.ToDecimal(
                    Quantidade
                );


            decimal valorItem =
                Convert.ToDecimal(
                    Valor
                );


            // =================================================
            // ALTERAÇÃO DO ITEM SELECIONADO
            // =================================================

            if (ItemSelecionado != null)
            {
                int indice =
                    Itens.IndexOf(
                        ItemSelecionado
                    );


                if (indice >= 0)
                {
                    ItemSelecionado.PropertyChanged -=
                        Item_PropertyChanged;


                    ItemVenda itemAtualizado =
                        new ItemVenda
                        {
                            Codigo =
                                codigoItem,

                            Descricao =
                                Descricao,

                            Quantidade =
                                quantidadeItem,

                            Valor =
                                valorItem
                        };


                    itemAtualizado.PropertyChanged +=
                        Item_PropertyChanged;


                    Itens[indice] =
                        itemAtualizado;


                    Mensagem =
                        "Item atualizado.";
                }
            }
            else
            {
                // =============================================
                // NOVO ITEM
                // =============================================

                ItemVenda item =
                    new ItemVenda
                    {
                        Codigo =
                            codigoItem,

                        Descricao =
                            Descricao,

                        Quantidade =
                            quantidadeItem,

                        Valor =
                            valorItem
                    };


                item.PropertyChanged +=
                    Item_PropertyChanged;


                Itens.Add(
                    item
                );
            }


            RecalcularTotalVenda();


            // =================================================
            // LIMPA CAMPOS
            // =================================================

            ItemSelecionado =
                null;

            Codigo =
                string.Empty;

            Descricao =
                string.Empty;

            Quantidade =
                "1";

            Valor =
                string.Empty;

            ProdutoSelecionado =
                null;
        }
        catch
        {
            Mensagem =
                "Preencha Código, Quantidade e Valor corretamente.";
        }
    }


    // =========================================================
    // ALTERAÇÃO DIRETA NO DATAGRID
    // =========================================================

    private void Item_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName ==
                nameof(ItemVenda.Quantidade) ||
            e.PropertyName ==
                nameof(ItemVenda.Valor) ||
            e.PropertyName ==
                nameof(ItemVenda.Total))
        {
            RecalcularTotalVenda();
        }
    }


    // =========================================================
    // RECALCULAR TOTAL
    // =========================================================

    private void RecalcularTotalVenda()
    {
        Total =
            Itens.Sum(
                x => x.Total
            );


        AtualizarSubTotal();
    }


    // =========================================================
    // LIMPAR ITENS
    // =========================================================

    private void LimparItensVenda()
    {
        foreach (ItemVenda item in Itens)
        {
            item.PropertyChanged -=
                Item_PropertyChanged;
        }


        Itens.Clear();
    }


    // =========================================================
    // REMOVER ITEM
    // =========================================================

    [RelayCommand]
    private void RemoverItem()
    {
        if (!PodeEditar)
        {
            Mensagem =
                "Pedido não está liberado para edição.";

            return;
        }


        if (ItemSelecionado == null)
            return;


        ItemSelecionado.PropertyChanged -=
            Item_PropertyChanged;


        Itens.Remove(
            ItemSelecionado
        );


        ItemSelecionado =
            null;


        RecalcularTotalVenda();
    }


    // =========================================================
    // ALTERAR PEDIDO
    // =========================================================

    [RelayCommand]
    private void AlterarPedido()
    {
        if (PedidoSelecionado == null)
        {
            Mensagem =
                "Selecione um pedido.";

            return;
        }


        if (PedidoSelecionado.Cancelado)
        {
            Mensagem =
                "Pedido cancelado não pode ser alterado.";

            return;
        }


        PodeEditar =
            true;


        EmAlteracao =
            true;


        AtualizarStatusPedido();


        Mensagem =
            $"Pedido {PedidoSelecionado.NumeroPedido} liberado para alteração.";
    }


    // =========================================================
    // SALVAR ALTERAÇÕES
    // =========================================================

    [RelayCommand]
    private async Task SalvarAlteracoesAsync()
    {
        if (PedidoSelecionado == null)
        {
            Mensagem =
                "Selecione um pedido.";

            return;
        }


        if (!EmAlteracao)
        {
            Mensagem =
                "O pedido não está em alteração.";

            return;
        }


        if (PedidoSelecionado.Cancelado)
        {
            Mensagem =
                "Pedido cancelado não pode ser alterado.";

            return;
        }


        if (Itens.Count == 0)
        {
            Mensagem =
                "O pedido precisa ter pelo menos um item.";

            return;
        }


        try
        {
            decimal valorDesconto =
                0;


            if (!string.IsNullOrWhiteSpace(
                Desconto))
            {
                decimal.TryParse(
                    Desconto,
                    out valorDesconto
                );
            }


            decimal valorPagoDecimal =
                0;


            if (!string.IsNullOrWhiteSpace(
                ValorPago))
            {
                decimal.TryParse(
                    ValorPago,
                    out valorPagoDecimal
                );
            }


            PedidoSelecionado.ClienteId =
                ClienteSelecionado?.Id;


            PedidoSelecionado.Total =
                Total;


            PedidoSelecionado.Desconto =
                valorDesconto;


            PedidoSelecionado.SubTotal =
                SubTotal;


            PedidoSelecionado.FormaPagamento =
                FormaPagamento;


            PedidoSelecionado.ValorPago =
                valorPagoDecimal;


            PedidoSelecionado.Troco =
                Troco;


            PedidoSelecionado.Sincronizado =
                false;


            Pedido pedidoAtualizado =
                await _vendaService
                    .AtualizarPedidoAsync(
                        PedidoSelecionado,
                        Itens
                    );


            AtualizarPedidoNaLista(
                pedidoAtualizado
            );


            PedidoSelecionado =
                pedidoAtualizado;


            EmAlteracao =
                false;


            PodeEditar =
                false;


            AtualizarStatusPedido();


            Mensagem =
                $"Pedido {pedidoAtualizado.NumeroPedido} salvo e lacrado com sucesso.";


            OnPropertyChanged(
                nameof(PodeFinalizarNovaVenda)
            );
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao salvar alteração: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // CANCELAR PEDIDO
    // =========================================================

    [RelayCommand]
    private async Task CancelarPedidoAsync()
    {
        if (PedidoSelecionado == null)
        {
            Mensagem =
                "Selecione um pedido.";

            return;
        }


        if (PedidoSelecionado.Cancelado)
        {
            Mensagem =
                "Este pedido já está cancelado.";

            return;
        }


        try
        {
            await _vendaService
                .CancelarPedidoAsync(
                    PedidoSelecionado.Id
                );


            PedidoSelecionado.Cancelado =
                true;


            PedidoSelecionado.Lacrado =
                true;


            PedidoSelecionado.Sincronizado =
                false;


            PedidoSelecionado.AtualizadoEm =
                DateTime.Now;


            PodeEditar =
                false;


            EmAlteracao =
                false;


            AtualizarStatusPedido();


            Mensagem =
                $"Pedido {PedidoSelecionado.NumeroPedido} cancelado com sucesso.";


            OnPropertyChanged(
                nameof(PodeFinalizarNovaVenda)
            );
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao cancelar pedido: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // CARREGAR PEDIDO
    // =========================================================

    private void CarregarPedido(
        Pedido? pedido)
    {
        if (pedido == null)
        {
            EmAlteracao =
                false;


            AtualizarStatusPedido();


            OnPropertyChanged(
                nameof(PodeFinalizarNovaVenda)
            );


            return;
        }


        LimparItensVenda();


        // =====================================================
        // ITENS
        // =====================================================

        foreach (ItemPedido item in pedido.Itens)
        {
            ItemVenda itemVenda =
                new ItemVenda
                {
                    Codigo =
                        item.ProdutoId,

                    Descricao =
                        item.DescricaoProduto,

                    Quantidade =
                        item.Quantidade,

                    Valor =
                        item.ValorUnitario
                };


            itemVenda.PropertyChanged +=
                Item_PropertyChanged;


            Itens.Add(
                itemVenda
            );
        }


        // =====================================================
        // CLIENTE
        // =====================================================

        ClienteSelecionado =
            Clientes.FirstOrDefault(
                c =>
                    c.Id ==
                    pedido.ClienteId
            );


        // =====================================================
        // TOTAIS
        // =====================================================

        Total =
            Itens.Sum(
                x => x.Total
            );


        Desconto =
            pedido.Desconto
                .ToString("0.00");


        AtualizarSubTotal();


        // =====================================================
        // PAGAMENTO
        // =====================================================

        if (string.IsNullOrWhiteSpace(
            pedido.FormaPagamento))
        {
            FormaPagamento =
                "Pix";
        }
        else
        {
            FormaPagamento =
                pedido.FormaPagamento;
        }


        ValorPago =
            pedido.ValorPago
                .ToString("0.00");


        Troco =
            pedido.Troco;


        // =====================================================
        // STATUS
        // =====================================================

        EmAlteracao =
            false;


        PodeEditar =
            false;


        AtualizarStatusPedido();


        Mensagem =
            $"Pedido {pedido.NumeroPedido} carregado.";


        OnPropertyChanged(
            nameof(PodeFinalizarNovaVenda)
        );
    }


    // =========================================================
    // ATUALIZAR PEDIDO NA LISTA
    // =========================================================

    private void AtualizarPedidoNaLista(
        Pedido pedidoAtualizado)
    {
        Pedido? pedidoLista =
            Pedidos.FirstOrDefault(
                p =>
                    p.Id ==
                    pedidoAtualizado.Id
            );


        if (pedidoLista == null)
            return;


        int indice =
            Pedidos.IndexOf(
                pedidoLista
            );


        if (indice >= 0)
        {
            Pedidos[indice] =
                pedidoAtualizado;
        }
    }


    // =========================================================
    // STATUS DO PEDIDO
    // =========================================================

    private void AtualizarStatusPedido()
    {
        // =====================================================
        // NOVO
        // =====================================================

        if (PedidoSelecionado == null)
        {
            StatusPedido =
                "NOVO PEDIDO";


            CorStatusPedido =
                "#2563EB";


            PodeEditar =
                true;


            return;
        }


        // =====================================================
        // CANCELADO
        // =====================================================

        if (PedidoSelecionado.Cancelado)
        {
            StatusPedido =
                "CANCELADO";


            CorStatusPedido =
                "#DC2626";


            PodeEditar =
                false;


            return;
        }


        // =====================================================
        // EM ALTERAÇÃO
        // =====================================================

        if (EmAlteracao)
        {
            StatusPedido =
                "EM ALTERAÇÃO";


            CorStatusPedido =
                "#FFC400";


            PodeEditar =
                true;


            return;
        }


        // =====================================================
        // LACRADO
        // =====================================================

        StatusPedido =
            "LACRADO";


        CorStatusPedido =
            "#16A34A";


        PodeEditar =
            false;
    }


    // =========================================================
    // FINALIZAR NOVA VENDA
    // =========================================================

    [RelayCommand]
    private async Task FinalizarVendaAsync()
    {
        if (PedidoSelecionado != null)
        {
            Mensagem =
                "Pedido existente. Use SALVAR ALTERAÇÕES.";

            return;
        }


        if (!PodeEditar)
        {
            Mensagem =
                "Venda não está liberada para edição.";

            return;
        }


        if (Itens.Count == 0)
        {
            Mensagem =
                "Adicione pelo menos um item na venda.";

            return;
        }


        try
        {
            decimal valorDesconto =
                0;


            if (!string.IsNullOrWhiteSpace(
                Desconto))
            {
                decimal.TryParse(
                    Desconto,
                    out valorDesconto
                );
            }


            decimal valorPagoDecimal =
                0;


            if (!string.IsNullOrWhiteSpace(
                ValorPago))
            {
                decimal.TryParse(
                    ValorPago,
                    out valorPagoDecimal
                );
            }


            Pedido novoPedido =
                await _vendaService
                    .FinalizarVendaAsync(
                        Itens,
                        ClienteSelecionado?.Id,
                        Total,
                        valorDesconto,
                        SubTotal,
                        FormaPagamento,
                        valorPagoDecimal,
                        Troco
                    );


            Pedidos.Insert(
                0,
                novoPedido
            );


            PedidoSelecionado =
                novoPedido;


            EmAlteracao =
                false;


            PodeEditar =
                false;


            AtualizarStatusPedido();


            Mensagem =
                $"Pedido {novoPedido.NumeroPedido} finalizado e salvo com sucesso.";


            OnPropertyChanged(
                nameof(PodeFinalizarNovaVenda)
            );
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro ao finalizar venda: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // BUSCA PRODUTO PELO CÓDIGO
    // =========================================================

    partial void OnCodigoChanged(
        string value)
    {
        if (!string.IsNullOrEmpty(value) &&
            !value.All(char.IsDigit))
        {
            Codigo =
                new string(
                    value
                        .Where(char.IsDigit)
                        .ToArray()
                );


            return;
        }


        if (string.IsNullOrWhiteSpace(
            value))
        {
            return;
        }


        if (!int.TryParse(
                value,
                out int codigoProduto))
        {
            return;
        }


        Produto? produto =
            Produtos.FirstOrDefault(
                p =>
                    p.Codigo ==
                    codigoProduto
            );


        if (produto != null)
        {
            ProdutoSelecionado =
                produto;
        }
    }


    // =========================================================
    // ALTERAÇÃO DO DESCONTO
    // =========================================================

    partial void OnDescontoChanged(
        string value)
    {
        AtualizarSubTotal();
    }


    // =========================================================
    // SUBTOTAL
    // =========================================================

    private void AtualizarSubTotal()
    {
        decimal valorDesconto =
            0;


        if (!string.IsNullOrWhiteSpace(
            Desconto))
        {
            decimal.TryParse(
                Desconto,
                out valorDesconto
            );
        }


        if (valorDesconto >= Total)
        {
            SubTotal =
                0;
        }
        else
        {
            SubTotal =
                Total -
                valorDesconto;
        }


        AtualizarPagamento();
    }


    // =========================================================
    // PAGAMENTO
    // =========================================================

    private void AtualizarPagamento()
    {
        if (FormaPagamento ==
            "Espécie")
        {
            AtualizarTroco();

            return;
        }


        ValorPago =
            SubTotal.ToString("0.00");


        Troco =
            0;
    }


    // =========================================================
    // TROCO
    // =========================================================

    private void AtualizarTroco()
    {
        if (FormaPagamento !=
            "Espécie")
        {
            Troco =
                0;

            return;
        }


        decimal valorPagoDecimal =
            0;


        if (!string.IsNullOrWhiteSpace(
            ValorPago))
        {
            decimal.TryParse(
                ValorPago,
                out valorPagoDecimal
            );
        }


        if (valorPagoDecimal <=
            SubTotal)
        {
            Troco =
                0;

            return;
        }


        Troco =
            valorPagoDecimal -
            SubTotal;
    }
}
