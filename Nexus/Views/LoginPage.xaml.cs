using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.ViewModels;
using Windows.System;

namespace Nexus.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<LoginViewModel>();
        ViewModel.LoginSucceeded += OnLoginSucceeded;
        Unloaded += (_, _) => ViewModel.LoginSucceeded -= OnLoginSucceeded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        // Wyczyść stan po nawigacji z RegisterPage z sukcesem
        ViewModel.ErrorMessage = null;
        LoginField.Focus(FocusState.Programmatic);
    }

    private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = PasswordField.Password;
    }

    private void PasswordField_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter)
        {
            ViewModel.ExecuteLoginCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ExecuteLoginCommand.Execute(null);
    }

    private void GoToRegister_Click(object sender, RoutedEventArgs e)
    {
        if (Frame != null)
        {
            Frame.Navigate(typeof(RegisterPage));
        }
    }

    private void OnLoginSucceeded()
    {
        ((App)Application.Current).ShowMainWindow();
    }
}
