using Avalonia.Controls;
using CursoAvalonia.Services;
using CursoAvalonia.ViewModels;

namespace CursoAvalonia.Views;

public partial class ConfiguracaoWindow : Window
{
    public ConfiguracaoWindow()
        : this(new InicializacaoService())
    {
    }

    public ConfiguracaoWindow(InicializacaoService inicializacaoService)
    {
        InitializeComponent();
        DataContext = new ConfiguracaoViewModel(inicializacaoService);
    }
}
