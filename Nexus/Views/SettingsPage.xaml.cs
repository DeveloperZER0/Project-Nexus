using Microsoft.UI.Xaml.Controls;
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
        Unloaded += (_, _) => ViewModel.LoggedOut -= OnLoggedOut;
    }

    private void OnLoggedOut()
    {
        ((App)Application.Current).ShowAuthWindow();
    }
}
