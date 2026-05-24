using Microsoft.UI.Xaml;
using Nexus.Views;

namespace Nexus;

/// <summary>
/// Okno autoryzacji — startuje na LoginPage, pozwala przejść do RegisterPage.
/// Wymiana na MainWindow po pomyślnym logowaniu odbywa się w App.ShowMainWindow.
/// </summary>
public sealed partial class AuthWindow : Window
{
    public AuthWindow()
    {
        InitializeComponent();
        AuthFrame.Navigate(typeof(LoginPage));
    }

    public void NavigateToLogin() => AuthFrame.Navigate(typeof(LoginPage));
    public void NavigateToRegister() => AuthFrame.Navigate(typeof(RegisterPage));
}
