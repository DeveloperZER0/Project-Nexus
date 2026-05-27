using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.ViewModels;

namespace Nexus.Views;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; }

    public ProfilePage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<ProfileViewModel>();

        UserPostsListView.ItemsSource = ViewModel.Posts;
        ViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.User))
            {
                RefreshProfileFields();
            }
        };

        ViewModel.EditProfileRequested += OnEditProfileRequested;
        Unloaded += (_, _) => ViewModel.EditProfileRequested -= OnEditProfileRequested;

        RefreshProfileFields();

        // Ustaw stan wizualny zakładek na domyślny ("posty")
        UpdateTabVisuals("posts");
    }

    // Obsługa kliknięcia w zakładkę profilu (Posty / Odpowiedzi / Media / Polubienia)
    private void OnTabClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            ViewModel.SelectedTab = tag;
            UpdateTabVisuals(tag);
        }
    }

    // Aktualizuje wygląd zakładek - aktywna ma jaśniejszy kolor i SemiBold
    private void UpdateTabVisuals(string activeTag)
    {
        var activeBrush = (SolidColorBrush)Application.Current.Resources["NexusForegroundBrush"];
        var inactiveBrush = (SolidColorBrush)Application.Current.Resources["NexusMutedForegroundBrush"];

        SetTabStyle(TabPosts, "posts", activeTag, activeBrush, inactiveBrush);
        SetTabStyle(TabReplies, "replies", activeTag, activeBrush, inactiveBrush);
        SetTabStyle(TabMedia, "media", activeTag, activeBrush, inactiveBrush);
        SetTabStyle(TabLikes, "likes", activeTag, activeBrush, inactiveBrush);
    }

    private static void SetTabStyle(Button btn, string tag, string activeTag, SolidColorBrush active, SolidColorBrush inactive)
    {
        bool isActive = tag == activeTag;
        btn.Foreground = isActive ? active : inactive;
        btn.FontWeight = isActive
            ? Microsoft.UI.Text.FontWeights.SemiBold
            : Microsoft.UI.Text.FontWeights.Normal;
    }

    /// <summary>
    /// Pozwala nawigować z parametrem string (login użytkownika) aby pokazać cudzy profil.
    /// Brak parametru = pokaż profil zalogowanego.
    /// </summary>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string handle && !string.IsNullOrWhiteSpace(handle))
        {
            ViewModel.LoadProfileByHandle(handle);
        }
        else
        {
            ViewModel.LoadCurrentUserProfile();
        }
    }

    private void RefreshProfileFields()
    {
        var u = ViewModel.User;
        if (u == null) return;

        ProfileName.Text = u.DisplayName;
        ProfileLogin.Text = u.AtLogin;
        ProfileBio.Text = u.Bio;
        ProfileLoginDetail.Text = u.Login;
        ProfileEmail.Text = u.Email;
        ProfileLocation.Text = u.Lokalizacja;
        ProfileWebsite.Text = u.Website;
        ProfileBirth.Text = $"Ur. {ViewModel.BirthDate}";
        ProfileJoined.Text = $"Dołączył: {ViewModel.JoinedDate}";

        var pl = new System.Globalization.CultureInfo("pl-PL");
        StatPosts.Text = u.PostsCount.ToString("N0", pl);
        StatFollowing.Text = u.FollowCount.ToString("N0", pl);
        StatFollowers.Text = u.FollowersCount.ToString("N0", pl);
    }

    /// <summary>
    /// Otwiera dialog edycji profilu i zapisuje zmiany po kliknięciu „Zapisz".
    /// Dialog jest budowany w code-behind, bo zawiera tylko 5 prostych TextBoxów.
    /// </summary>
    private async void OnEditProfileRequested()
    {
        var u = ViewModel.User;
        if (u == null) return;

        var displayNameBox = new TextBox
        {
            Header = "Imię i nazwisko",
            Text = u.DisplayName,
        };
        var bioBox = new TextBox
        {
            Header = "Bio",
            Text = u.Bio,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 80,
        };
        var locationBox = new TextBox
        {
            Header = "Lokalizacja",
            Text = u.Lokalizacja,
        };
        var websiteBox = new TextBox
        {
            Header = "Strona internetowa",
            Text = u.Website,
        };
        var birthBox = new TextBox
        {
            Header = "Data urodzenia (dd.MM.yyyy)",
            Text = ViewModel.BirthDate,
        };

        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(displayNameBox);
        panel.Children.Add(bioBox);
        panel.Children.Add(locationBox);
        panel.Children.Add(websiteBox);
        panel.Children.Add(birthBox);

        var dialog = new ContentDialog
        {
            Title = "Edytuj profil",
            PrimaryButtonText = "Zapisz",
            CloseButtonText = "Anuluj",
            DefaultButton = ContentDialogButton.Primary,
            Content = panel,
            XamlRoot = this.Content.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            ViewModel.SaveProfile(
                displayNameBox.Text,
                bioBox.Text,
                locationBox.Text,
                websiteBox.Text,
                birthBox.Text);
            RefreshProfileFields();
        }
    }
}
