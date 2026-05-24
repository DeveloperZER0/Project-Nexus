using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.ViewModels;

namespace Nexus.Views;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel { get; }

    public RegisterPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<RegisterViewModel>();
        ViewModel.RegistrationSucceeded += OnRegistrationSucceeded;
        Unloaded += (_, _) => ViewModel.RegistrationSucceeded -= OnRegistrationSucceeded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.ErrorMessage = null;
        LoginField.Focus(FocusState.Programmatic);
    }

    private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = PasswordField.Password;
    }

    private void ConfirmField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.ConfirmPassword = ConfirmField.Password;
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RegisterCommand.Execute(null);
    }

    private void GoToLogin_Click(object sender, RoutedEventArgs e)
    {
        if (Frame != null)
        {
            // Frame.GoBack jeśli przyszliśmy z LoginPage, inaczej Navigate
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Navigate(typeof(LoginPage));
        }
    }

    private void OnRegistrationSucceeded()
    {
        // Po rejestracji wróć do LoginPage; preload login do TextBoxa.
        var login = ViewModel.Login;
        if (Frame == null) return;

        if (Frame.CanGoBack)
            Frame.GoBack();
        else
            Frame.Navigate(typeof(LoginPage));

        // Preload login na nowej stronie
        if (Frame.Content is LoginPage page)
        {
            page.ViewModel.Login = login;
            page.ViewModel.ErrorMessage = "Konto utworzone — zaloguj się";
        }
    }
}
