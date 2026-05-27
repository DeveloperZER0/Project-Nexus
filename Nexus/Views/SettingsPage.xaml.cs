using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Nexus.ViewModels;

namespace Nexus.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<SettingsViewModel>();
        ViewModel.LoggedOut += OnLoggedOut;
        ViewModel.ChangePasswordRequested += OnChangePasswordRequested;
        Unloaded += (_, _) =>
        {
            ViewModel.LoggedOut -= OnLoggedOut;
            ViewModel.ChangePasswordRequested -= OnChangePasswordRequested;
        };
    }

    private void OnLoggedOut()
    {
        ((App)Application.Current).ShowAuthWindow();
    }

    private async void OnChangePasswordRequested()
    {
        var oldBox = new PasswordBox { PlaceholderText = "Stare hasło" };
        var newBox = new PasswordBox { PlaceholderText = "Nowe hasło (min. 8 znaków)" };

        Brush errorBrush;
        if (Application.Current.Resources.TryGetValue("NexusDestructiveBrush", out var resource) && resource is Brush brush)
        {
            errorBrush = brush;
        }
        else
        {
            errorBrush = new SolidColorBrush(Colors.Red);
        }

        var errorText = new TextBlock
        {
            Foreground = errorBrush,
            Visibility = Visibility.Collapsed,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap
        };

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(oldBox);
        panel.Children.Add(newBox);
        panel.Children.Add(errorText);

        var dialog = new ContentDialog
        {
            Title = "Zmień hasło",
            PrimaryButtonText = "Zmień",
            CloseButtonText = "Anuluj",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot,
            Content = panel
        };

        dialog.PrimaryButtonClick += (s, args) =>
        {
            var ok = ViewModel.TryChangePassword(oldBox.Password, newBox.Password);
            if (!ok)
            {
                errorText.Text = "Nie udało się zmienić hasła. Sprawdź stare hasło i czy nowe ma co najmniej 8 znaków.";
                errorText.Visibility = Visibility.Visible;
                args.Cancel = true;
            }
        };

        await dialog.ShowAsync();
    }
}
