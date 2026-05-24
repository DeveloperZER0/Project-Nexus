using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
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

        RefreshProfileFields();
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
}
