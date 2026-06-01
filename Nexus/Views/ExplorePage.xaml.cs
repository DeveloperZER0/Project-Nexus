using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Models;
using Nexus.ViewModels;

namespace Nexus.Views;

public sealed partial class ExplorePage : Page
{
    public ExploreViewModel ViewModel { get; }

    public ExplorePage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<ExploreViewModel>();
        HashtagsListView.ItemsSource = ViewModel.TrendingHashtags;
    }

    /// <summary>Pozwala wejść na Odkrywaj z gotowym zapytaniem (globalna wyszukiwarka).</summary>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is string query && !string.IsNullOrWhiteSpace(query))
        {
            ViewModel.SearchQuery = query;
        }
    }

    private void Category_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string category)
        {
            ViewModel.SelectCategoryCommand.Execute(category);
        }
    }

    private void Trending_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is TrendingHashtag hashtag)
        {
            ViewModel.SelectTrendingTopicCommand.Execute(hashtag);
        }
    }

    private void FollowUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SuggestedUser user)
        {
            ViewModel.ToggleFollow(user);
        }
    }

    private void UserResult_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SuggestedUser user)
        {
            Frame?.Navigate(typeof(ProfilePage), user.Login);
        }
    }
}
