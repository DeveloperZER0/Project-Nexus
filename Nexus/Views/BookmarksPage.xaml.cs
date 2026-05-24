using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Models;
using Nexus.ViewModels;

namespace Nexus.Views;

public sealed partial class BookmarksPage : Page
{
    public BookmarksViewModel ViewModel { get; }

    public BookmarksPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<BookmarksViewModel>();
        SubtitleText.Text = ViewModel.Subtitle;
        BookmarksListView.ItemsSource = ViewModel.Posts;
        ViewModel.Posts.CollectionChanged += (_, _) => SubtitleText.Text = ViewModel.Subtitle;
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Post post)
            ViewModel.ToggleLikeCommand.Execute(post);
    }

    private void RepostButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Post post)
            ViewModel.ToggleRetweetCommand.Execute(post);
    }

    private void RemoveBookmarkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Post post)
            ViewModel.RemoveBookmarkCommand.Execute(post);
    }
}
