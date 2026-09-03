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
    [ObservableProperty]
    private string codigo = string.Empty;

    [ObservableProperty]
    private string descricao = string.Empty;

    [ObservableProperty]
    private string quantidade = string.Empty;

    [ObservableProperty]
    private string valor = string.Empty;

    public ObservableCollection<ItemVenda> Itens { get; } = new();

    public ObservableCollection<Produto> Produtos { get; } = new();

    [ObservableProperty]
    private string mensagem = string.Empty;

    [ObservableProperty]
    private string desconto = string.Empty;

    [ObservableProperty]
    private decimal subTotal;

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private ItemVenda? itemSelecionado;

    private readonly VendaService _vendaService = new();

    [RelayCommand]
    private async Task FinalizarVendaAsync()
    {
        try
        {
            decimal valorDesconto = 0;

            if (!string.IsNullOrWhiteSpace(Desconto))
            {
                decimal.TryParse(Desconto, out valorDesconto);
            }

            await _vendaService.FinalizarVendaAsync(
                Itens,
                Total,
                valorDesconto,
                SubTotal
            );

            Mensagem = "Venda finalizada e salva com sucesso.";

            Itens.Clear();

            Total = 0;
            SubTotal = 0;
            Desconto = string.Empty;
        }
        catch (Exception ex)
        {
            Mensagem = "Erro: " +
                (ex.InnerException?.Message ?? ex.Message);
        }
    }

    [RelayCommand]
    private void AdicionarItem()
    {
        Mensagem = string.Empty;

        try
        {
            ItemVenda item = new ItemVenda();

            item.Codigo = Convert.ToInt32(Codigo);
            item.Descricao = Descricao;
            item.Quantidade = Convert.ToDecimal(Quantidade);
            item.Valor = Convert.ToDecimal(Valor);

            Itens.Add(item);

            Total = Itens.Sum(x => x.Total);
            SubTotal = Total;

            Codigo = string.Empty;
            Descricao = string.Empty;
            Quantidade = string.Empty;
            Valor = string.Empty;
        }
        catch (Exception)
        {
            Mensagem = "Preencha Código, Quantidade e Valor corretamente.";
        }
    }

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

    partial void OnCodigoChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !value.All(char.IsDigit))
        {
            Codigo = new string(value.Where(char.IsDigit).ToArray());
        }
    }

    partial void OnDescontoChanged(string value)
    {
        AtualizarSubTotal();
    }

    private void AtualizarSubTotal()
    {
        decimal valorDesconto = 0;

        if (!string.IsNullOrWhiteSpace(Desconto))
        {
            decimal.TryParse(Desconto, out valorDesconto);
        }

        if (valorDesconto >= Total)
        {
            SubTotal = 0;
            return;
        }

        SubTotal = Total - valorDesconto;
    }
}