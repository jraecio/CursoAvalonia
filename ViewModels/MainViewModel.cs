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
        // CLIENTES FIXOS
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
        // PRODUTOS FIXOS
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
    }


    // =========================================================
    // ADICIONAR ITEM
    // =========================================================

    [RelayCommand]
    private void AdicionarItem()
    {
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

            // Total da venda sem desconto
            Total = Itens.Sum(x => x.Total);

            // Recalcula subtotal e pagamento
            AtualizarSubTotal();


            // =================================================
            // LIMPA PRODUTO PARA PRÓXIMA INCLUSÃO
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
        if (ItemSelecionado == null)
            return;

        Itens.Remove(ItemSelecionado);

        Total = Itens.Sum(x => x.Total);

        AtualizarSubTotal();

        ItemSelecionado = null;
    }


    // =========================================================
    // FINALIZAR VENDA
    // =========================================================

    [RelayCommand]
    private async Task FinalizarVendaAsync()
    {
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
            // SALVA VENDA
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

            Itens.Clear();

            Total = 0;
            SubTotal = 0;

            Desconto = string.Empty;

            ValorPago = string.Empty;
            Troco = 0;


            // =================================================
            // LIMPA PRODUTO
            // =================================================

            Codigo = string.Empty;
            Descricao = string.Empty;
            Quantidade = "1";
            Valor = string.Empty;

            ProdutoSelecionado = null;
            ItemSelecionado = null;
        }
        catch (Exception ex)
        {
            Mensagem =
                "Erro: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }


    // =========================================================
    // BUSCA POR CÓDIGO
    // =========================================================

    partial void OnCodigoChanged(string value)
    {
        // Permite somente números
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


        if (!int.TryParse(value, out int codigoProduto))
            return;


        Produto? produto = Produtos
            .FirstOrDefault(
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
            SubTotal = Total - valorDesconto;
        }


        // =====================================================
        // ATUALIZA DE ACORDO COM A FORMA DE PAGAMENTO
        // =====================================================

        AtualizarPagamento();
    }


    // =========================================================
    // ATUALIZA PAGAMENTO
    // =========================================================

    private void AtualizarPagamento()
    {
        // =====================================================
        // DINHEIRO / ESPÉCIE
        // =====================================================

        if (FormaPagamento == "Espécie")
        {
            AtualizarTroco();
            return;
        }


        // =====================================================
        // PIX / CRÉDITO / DÉBITO
        //
        // O VALOR PAGO É O PRÓPRIO TOTAL DA VENDA
        // E NÃO EXISTE TROCO
        // =====================================================

        ValorPago = SubTotal.ToString("0.00");

        Troco = 0;
    }


    // =========================================================
    // CÁLCULO DO TROCO
    // =========================================================

    private void AtualizarTroco()
    {
        // Só existe troco para pagamento em espécie
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
        // VALOR PAGO MENOR OU IGUAL AO TOTAL
        // =====================================================

        if (valorPagoDecimal <= SubTotal)
        {
            Troco = 0;
            return;
        }


        // =====================================================
        // CALCULA TROCO
        // =====================================================

        Troco = valorPagoDecimal - SubTotal;
    }
}