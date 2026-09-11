using Avalonia.Controls;
using CursoAvalonia.ViewModels;

namespace CursoAvalonia.Views;

public partial class ConfiguracaoView : UserControl
{
    public ConfiguracaoView()
    {
        InitializeComponent();

        DataContext = new ConfiguracaoViewModel();
    }
}