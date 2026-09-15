using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CursoAvalonia.Services;
using CursoAvalonia.ViewModels;
using CursoAvalonia.Views;

namespace CursoAvalonia;

public partial class App : Application
{
    // =========================================================
    // SERVIÇO ÚNICO DE INICIALIZAÇÃO
    // =========================================================

    private readonly InicializacaoService _inicializacaoService =
        new();


    // =========================================================
    // INICIALIZAÇÃO DO AVALONIA
    // =========================================================

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }


    // =========================================================
    // INICIALIZAÇÃO DA APLICAÇÃO
    // =========================================================

    public override void OnFrameworkInitializationCompleted()
    {
        // =====================================================
        // TEMA
        // =====================================================

        RequestedThemeVariant =
            ThemeVariant.Light;


        // =====================================================
        // APLICAÇÃO DESKTOP
        // =====================================================

        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // =================================================
            // VIEWMODEL PRINCIPAL
            // =================================================

            MainViewModel mainViewModel =
                new MainViewModel(
                    _inicializacaoService
                );


            // =================================================
            // ABRIR TELA PRINCIPAL IMEDIATAMENTE
            // =================================================

            desktop.MainWindow =
                new MainWindow
                {
                    DataContext =
                        mainViewModel
                };


            // =================================================
            // SINCRONIZAÇÃO EM SEGUNDO PLANO
            // =================================================

            _ = _inicializacaoService
                .InicializarAsync();
        }


        base.OnFrameworkInitializationCompleted();
    }
}