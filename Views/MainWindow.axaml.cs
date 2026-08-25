using Avalonia.Controls;
using Avalonia.Input;

namespace CursoAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Codigo_KeyDown(object? sender, KeyEventArgs e)
    {
        bool numero =
            (e.Key >= Key.D0 && e.Key <= Key.D9) ||
            (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);

        bool teclaPermitida =
            e.Key == Key.Back ||
            e.Key == Key.Delete ||
            e.Key == Key.Left ||
            e.Key == Key.Right ||
            e.Key == Key.Tab ||
            e.Key == Key.Home ||
            e.Key == Key.End;

        if (!numero && !teclaPermitida)
        {
            e.Handled = true;
        }
    }

    private void Decimal_KeyDown(object? sender, KeyEventArgs e)
    {
        bool numero =
            (e.Key >= Key.D0 && e.Key <= Key.D9) ||
            (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);

        bool separadorDecimal =
            e.Key == Key.OemComma ||
            e.Key == Key.Decimal;

        bool teclaPermitida =
            e.Key == Key.Back ||
            e.Key == Key.Delete ||
            e.Key == Key.Left ||
            e.Key == Key.Right ||
            e.Key == Key.Tab ||
            e.Key == Key.Home ||
            e.Key == Key.End;

        if (!numero && !separadorDecimal && !teclaPermitida)
        {
            e.Handled = true;
        }
    }
}