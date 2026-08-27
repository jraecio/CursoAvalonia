using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Styling;

namespace CursoAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        btnAdicionar.Click += Adicionar_Click;
    }

    private void AlternarTema_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Application.Current is not { } app)
            return;

        if (app.ActualThemeVariant == ThemeVariant.Dark)
        {
            app.RequestedThemeVariant = ThemeVariant.Light;
        }
        else
        {
            app.RequestedThemeVariant = ThemeVariant.Dark;
        }
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

    private void Adicionar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        txtCodigo.Focus();
    }

}