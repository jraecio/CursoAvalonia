using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CursoAvalonia.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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

    [RelayCommand]
    private void AdicionarItem()
    {
        ItemVenda item = new ItemVenda();

        item.Codigo = Convert.ToInt32(Codigo);
        item.Descricao = Descricao;
        item.Quantidade = Convert.ToDecimal(Quantidade);
        item.Valor = Convert.ToDecimal(Valor);

        Itens.Add(item);
    }

    partial void OnCodigoChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && !value.All(char.IsDigit))
        {
            Codigo = new string(value.Where(char.IsDigit).ToArray());
        }
    }
}