using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursoAvalonia.Models;
using CursoAvalonia.Services;
using System;
using System.Collections.ObjectModel;
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

                Codigo = value.Codigo.ToString();
                Descricao = value.Descricao;
                Valor = value.Valor.ToString("0.00");
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


    private string _statusPedido = "EM EDIÇÃO";

    public string StatusPedido
    {
        get => _statusPedido;
        set => SetProperty(ref _statusPedido, value);
    }


    private string _corStatusPedido = "#FFC400";

    public string CorStatusPedido
    {
        get => _corStatusPedido;
        set => SetProperty(ref _corStatusPedido, value);
    }


    // =========================================================
    // TOTAIS / MENSAGENS
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

    private readonly VendaService _vendaService = new();


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public MainViewModel()
    {
        // =====================================================
        // CLIENTES DE TESTE
        // =====================================================

        Clientes.Add(new Cliente
        {
            Id = 1,
            Nome = "Aecio"
        });

        Clientes.Add(new Cliente
        {
            Id = 2,
            Nome = "Softcom"
        });

        Clientes.Add(new Cliente
        {
            Id = 3,
            Nome = "Barbosa"
        });


        // =====================================================
        // PRODUTOS DE TESTE
        // =====================================================

        Produtos.Add(new Produto
        {
            Codigo = 1,
            Descricao = "Notebook",
            Valor = 2200.00m
        });

        Produtos.Add(new Produto
        {
            Codigo = 2,
            Descricao = "Pelicula Insulfilm",
            Valor = 350.00m
        });

        Produtos.Add(new Produto
        {
            Codigo = 3,
            Descricao = "Farol de LED H11",
            Valor = 150.00m
        });


        // =====================================================
        // PEDIDOS DE TESTE
        // =====================================================

        Pedidos.Add(new Pedido
        {
            NumeroPedido = 1001,
            Total = 2200.00m,
            Desconto = 0,
            SubTotal = 2200.00m,
            Lacrado = true,
            Cancelado = false
        });

        Pedidos.Add(new Pedido
        {
            NumeroPedido = 1002,
            Total = 350.00m,
            Desconto = 50.00m,
            SubTotal = 300.00m,
            Lacrado = false,
            Cancelado = false
        });

        Pedidos.Add(new Pedido
        {
            NumeroPedido = 1003,
            Total = 150.00m,
            Desconto = 0,
            SubTotal = 150.00m,
            Lacrado = true,
            Cancelado = true
        });


        // =====================================================
        // ESTADO INICIAL
        // =====================================================

        AtualizarStatusPedido();
    }


    // =========================================================
    // ADICIONAR ITEM
    // =========================================================

    [RelayCommand]
    private void AdicionarItem()
    {
        if (!PodeEditar)
        {
            Mensagem = "Pedido não está liberado para edição.";
            return;
        }

        Mensagem = string.Empty;

        try
        {
            ItemVenda item = new ItemVenda
            {
                Codigo = Convert.ToInt32(Codigo),
                Descricao = Descricao,
                Quantidade = Convert.ToDecimal(Quantidade),
                Valor = Convert.ToDecimal(Valor)
            };

            Itens.Add(item);

            Total = Itens.Sum(x => x.Total);

            AtualizarSubTotal();


            // =================================================
            // LIMPA PRODUTO
            // =================================================

            Codigo = string.Empty;
            Descricao = string.Empty;
            Quantidade = "1";
            Valor = string.Empty;

            ProdutoSelecionado = null;
        }
        catch (Exception)
        {
            Mensagem =
                "Preencha Código, Quantidade e Valor corretamente.";
        }
    }


    // =========================================================
    // REMOVER ITEM
    // =========================================================

    [RelayCommand]
    private void RemoverItem()
    {
        if (!PodeEditar)
        {
            Mensagem = "Pedido não está liberado para edição.";
            return;
        }

        if (ItemSelecionado == null)
            return;

        Itens.Remove(ItemSelecionado);

        Total = Itens.Sum(x => x.Total);

        AtualizarSubTotal();

        ItemSelecionado = null;
    }
    // =========================================================
    // NOVO PEDIDO
    // =========================================================

    [RelayCommand]
    private void NovoPedido()
    {
        // Sai de qualquer pedido pesquisado
        PedidoSelecionado = null;

        // Limpa os itens
        Itens.Clear();

        // Limpa cliente
        ClienteSelecionado = null;

        // Limpa produto
        ProdutoSelecionado = null;
        ItemSelecionado = null;

        Codigo = string.Empty;
        Descricao = string.Empty;
        Quantidade = "1";
        Valor = string.Empty;

        // Limpa valores
        Total = 0;
        SubTotal = 0;
        Desconto = string.Empty;

        // Pagamento
        FormaPagamento = "Pix";
        ValorPago = string.Empty;
        Troco = 0;

        // Libera a venda
        PodeEditar = true;

        // Status
        StatusPedido = "NOVO PEDIDO";
        CorStatusPedido = "#2563EB";

        Mensagem = "Nova venda iniciada.";
    }

    // =========================================================
    // ALTERAR PEDIDO
    // =========================================================

    [RelayCommand]
    private void AlterarPedido()
    {
        if (PedidoSelecionado == null)
        {
            Mensagem = "Selecione um pedido.";
            return;
        }

        if (PedidoSelecionado.Cancelado)
        {
            Mensagem =
                "Pedido cancelado não pode ser alterado.";

            return;
        }

        PedidoSelecionado.Lacrado = false;

        PodeEditar = true;

        AtualizarStatusPedido();

        Mensagem =
            "Pedido liberado para alteração.";
    }


    // =========================================================
    // CANCELAR PEDIDO
    // =========================================================

    [RelayCommand]
    private void CancelarPedido()
    {
        if (PedidoSelecionado == null)
        {
            Mensagem = "Selecione um pedido.";
            return;
        }

        if (PedidoSelecionado.Cancelado)
        {
            Mensagem =
                "Este pedido já está cancelado.";

            return;
        }

        PedidoSelecionado.Cancelado = true;
        PedidoSelecionado.Lacrado = true;

        PodeEditar = false;

        AtualizarStatusPedido();

        Mensagem =
            "Pedido cancelado.";
    }


    // =========================================================
    // CARREGAR PEDIDO
    // =========================================================

    private void CarregarPedido(Pedido? pedido)
    {
        if (pedido == null)
        {
            AtualizarStatusPedido();
            return;
        }

        Itens.Clear();

        // =====================================================
        // NESTA ETAPA AINDA NÃO CARREGAMOS OS ITENS DO BANCO.
        // ISSO SERÁ FEITO QUANDO CONECTARMOS A BUSCA AO SQLITE.
        // =====================================================

        Total = pedido.Total;

        Desconto =
            pedido.Desconto.ToString("0.00");

        SubTotal =
            pedido.SubTotal;


        // =====================================================
        // DEFINE SE O PEDIDO PODE SER EDITADO
        // =====================================================

        PodeEditar =
            !pedido.Lacrado &&
            !pedido.Cancelado;

        AtualizarStatusPedido();

        Mensagem =
            $"Pedido {pedido.NumeroPedido} carregado.";
    }


    // =========================================================
    // ATUALIZAR STATUS DO PEDIDO
    // =========================================================

    private void AtualizarStatusPedido()
    {
        // =====================================================
        // NOVA VENDA
        // =====================================================

        if (PedidoSelecionado == null)
        {
            StatusPedido = "EM EDIÇÃO";

            CorStatusPedido = "#FFC400";

            PodeEditar = true;

            return;
        }


        // =====================================================
        // CANCELADO
        // =====================================================

        if (PedidoSelecionado.Cancelado)
        {
            StatusPedido = "CANCELADO";

            CorStatusPedido = "#DC2626";

            PodeEditar = false;

            return;
        }


        // =====================================================
        // LACRADO
        // =====================================================

        if (PedidoSelecionado.Lacrado)
        {
            StatusPedido = "LACRADO";

            CorStatusPedido = "#16A34A";

            PodeEditar = false;

            return;
        }


        // =====================================================
        // EM ALTERAÇÃO
        // =====================================================

        StatusPedido = "EM ALTERAÇÃO";

        CorStatusPedido = "#FFC400";

        PodeEditar = true;
    }


    // =========================================================
    // FINALIZAR VENDA
    // =========================================================

    [RelayCommand]
    private async Task FinalizarVendaAsync()
    {
        if (!PodeEditar)
        {
            Mensagem =
                "Pedido não está liberado para edição.";

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
            decimal valorDesconto = 0;

            if (!string.IsNullOrWhiteSpace(Desconto))
            {
                decimal.TryParse(
                    Desconto,
                    out valorDesconto
                );
            }


            // =================================================
            // PEDIDO EXISTENTE
            // =================================================

            if (PedidoSelecionado != null)
            {
                PedidoSelecionado.Total =
                    Total;

                PedidoSelecionado.Desconto =
                    valorDesconto;

                PedidoSelecionado.SubTotal =
                    SubTotal;

                PedidoSelecionado.AtualizadoEm =
                    DateTime.Now;

                PedidoSelecionado.Lacrado =
                    true;

                PodeEditar =
                    false;

                AtualizarStatusPedido();

                Mensagem =
                    $"Pedido {PedidoSelecionado.NumeroPedido} atualizado e lacrado.";

                return;
            }


            // =================================================
            // NOVA VENDA
            // =================================================

            await _vendaService.FinalizarVendaAsync(
                Itens,
                Total,
                valorDesconto,
                SubTotal
            );


            Mensagem =
                "Venda finalizada e salva com sucesso.";


            // =================================================
            // LIMPA VENDA
            // =================================================

            LimparVenda();
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // LIMPAR VENDA
    // =========================================================

    private void LimparVenda()
    {
        Itens.Clear();

        Total = 0;
        SubTotal = 0;

        Desconto = string.Empty;

        ValorPago = string.Empty;

        Troco = 0;

        Codigo = string.Empty;
        Descricao = string.Empty;
        Quantidade = "1";
        Valor = string.Empty;

        ProdutoSelecionado = null;
        ItemSelecionado = null;
    }


    // =========================================================
    // BUSCA DO PRODUTO PELO CÓDIGO
    // =========================================================

    partial void OnCodigoChanged(string value)
    {
        // =====================================================
        // PERMITE SOMENTE NÚMEROS
        // =====================================================

        if (!string.IsNullOrEmpty(value) &&
            !value.All(char.IsDigit))
        {
            Codigo = new string(
                value.Where(char.IsDigit).ToArray()
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(value))
            return;


        if (!int.TryParse(
                value,
                out int codigoProduto))
        {
            return;
        }


        Produto? produto =
            Produtos.FirstOrDefault(
                p => p.Codigo == codigoProduto
            );


        if (produto != null)
        {
            ProdutoSelecionado = produto;
        }
    }


    // =========================================================
    // ALTERAÇÃO DO DESCONTO
    // =========================================================

    partial void OnDescontoChanged(string value)
    {
        AtualizarSubTotal();
    }


    // =========================================================
    // CÁLCULO DO SUBTOTAL
    // =========================================================

    private void AtualizarSubTotal()
    {
        decimal valorDesconto = 0;


        if (!string.IsNullOrWhiteSpace(Desconto))
        {
            decimal.TryParse(
                Desconto,
                out valorDesconto
            );
        }


        // =====================================================
        // NÃO PERMITE SUBTOTAL NEGATIVO
        // =====================================================

        if (valorDesconto >= Total)
        {
            SubTotal = 0;
        }
        else
        {
            SubTotal =
                Total - valorDesconto;
        }


        AtualizarPagamento();
    }


    // =========================================================
    // ATUALIZAR PAGAMENTO
    // =========================================================

    private void AtualizarPagamento()
    {
        // =====================================================
        // ESPÉCIE
        // =====================================================

        if (FormaPagamento == "Espécie")
        {
            AtualizarTroco();
            return;
        }


        // =====================================================
        // PIX / CRÉDITO / DÉBITO
        // =====================================================

        ValorPago =
            SubTotal.ToString("0.00");

        Troco = 0;
    }


    // =========================================================
    // CÁLCULO DO TROCO
    // =========================================================

    private void AtualizarTroco()
    {
        // =====================================================
        // TROCO SOMENTE EM ESPÉCIE
        // =====================================================

        if (FormaPagamento != "Espécie")
        {
            Troco = 0;
            return;
        }


        decimal valorPagoDecimal = 0;


        if (!string.IsNullOrWhiteSpace(ValorPago))
        {
            decimal.TryParse(
                ValorPago,
                out valorPagoDecimal
            );
        }


        // =====================================================
        // NÃO EXISTE TROCO SE PAGOU MENOS OU IGUAL
        // =====================================================

        if (valorPagoDecimal <= SubTotal)
        {
            Troco = 0;
            return;
        }


        // =====================================================
        // CALCULA TROCO
        // =====================================================

        Troco =
            valorPagoDecimal - SubTotal;
    }
}