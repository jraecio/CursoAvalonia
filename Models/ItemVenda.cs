using CommunityToolkit.Mvvm.ComponentModel;

namespace CursoAvalonia.Models;

public class ItemVenda : ObservableObject
{
    private int _codigo;

    public int Codigo
    {
        get => _codigo;
        set => SetProperty(ref _codigo, value);
    }


    private string _descricao = string.Empty;

    public string Descricao
    {
        get => _descricao;
        set => SetProperty(ref _descricao, value);
    }


    private decimal _quantidade;

    public decimal Quantidade
    {
        get => _quantidade;

        set
        {
            if (SetProperty(ref _quantidade, value))
            {
                OnPropertyChanged(nameof(Total));
            }
        }
    }


    private decimal _valor;

    public decimal Valor
    {
        get => _valor;

        set
        {
            if (SetProperty(ref _valor, value))
            {
                OnPropertyChanged(nameof(Total));
            }
        }
    }


    public decimal Total
    {
        get
        {
            return Quantidade * Valor;
        }
    }
}