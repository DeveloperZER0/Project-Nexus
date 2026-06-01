using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nexus.Models;
using Microsoft.Extensions.DependencyInjection;
using Nexus.ViewModels;

namespace Nexus.Views;

/// <summary>
/// Code-behind strony głównej — minimalna logika UI (zakładki, compose).
/// Dane i komendy obsługiwane przez HomeViewModel.
/// </summary>
public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<HomeViewModel>();
        PostsListView.ItemsSource = ViewModel.Posts;
    }

    /// <summary>
    /// Przełączanie zakładek "Dla Ciebie" / "Obserwowani".
    /// </summary>
    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            var tag = btn.Tag?.ToString() ?? "forYou";
            ViewModel.SelectedTab = tag;

            // Aktualizacja wizualna zakładek
            TabForYou.Foreground = tag == "forYou"
                ? (Microsoft.UI.Xaml.Media.Brush)Resources["NexusForegroundBrush"]
                    ?? App.Current.Resources["NexusForegroundBrush"] as Microsoft.UI.Xaml.Media.Brush
                : App.Current.Resources["NexusMutedForegroundBrush"] as Microsoft.UI.Xaml.Media.Brush;

            TabFollowing.Foreground = tag == "following"
                ? App.Current.Resources["NexusForegroundBrush"] as Microsoft.UI.Xaml.Media.Brush
                : App.Current.Resources["NexusMutedForegroundBrush"] as Microsoft.UI.Xaml.Media.Brush;

            TabForYou.FontWeight = tag == "forYou"
                ? Microsoft.UI.Text.FontWeights.SemiBold
                : Microsoft.UI.Text.FontWeights.Normal;

            TabFollowing.FontWeight = tag == "following"
                ? Microsoft.UI.Text.FontWeights.SemiBold
                : Microsoft.UI.Text.FontWeights.Normal;
        }
    }

    private void ComposeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = ComposeTextBox.Text;
        ViewModel.ComposeText = text;
        CharCountText.Text = ViewModel.RemainingChars.ToString();
        PublishButton.IsEnabled = !string.IsNullOrWhiteSpace(text);

        // Podświetlenie na pomarańczowo gdy mało znaków
        CharCountText.Foreground = ViewModel.RemainingChars < 30
            ? App.Current.Resources["NexusOrangeBrush"] as Microsoft.UI.Xaml.Media.Brush
            : App.Current.Resources["NexusMutedForegroundBrush"] as Microsoft.UI.Xaml.Media.Brush;
    }

    private void PublishButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.PublishCommand.Execute(null);
        ComposeTextBox.Text = string.Empty;
    }

    private async void CommentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Post post) return;

        ViewModel.SetSelectedPost(post);
        await ShowCommentsDialogAsync();
    }

    /// <summary>
    /// Dialog komentarzy — wyświetla istniejące komentarze posta i pozwala dodać nowy.
    /// Lista odświeża się na żywo po dodaniu, a licznik komentarzy na karcie posta rośnie.
    /// </summary>
    private async System.Threading.Tasks.Task ShowCommentsDialogAsync()
    {
        var comments = new ObservableCollection<Comment>(ViewModel.GetCommentsForSelected());

        var list = new ListView
        {
            ItemsSource = comments,
            ItemTemplate = (DataTemplate)Resources["CommentTemplate"],
            SelectionMode = ListViewSelectionMode.None,
            IsItemClickEnabled = false,
            MaxHeight = 320,
        };

        var emptyText = new TextBlock
        {
            Text = "Brak komentarzy. Bądź pierwszy!",
            FontSize = 13,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)App.Current.Resources["NexusMutedForegroundBrush"],
            Visibility = comments.Count == 0 ? Visibility.Visible : Visibility.Collapsed,
        };

        var input = new TextBox
        {
            PlaceholderText = "Napisz komentarz...",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 80,
        };

        var addButton = new Button
        {
            Content = "Dodaj",
            Style = (Style)App.Current.Resources["NexusPrimaryButtonStyle"],
        };

        addButton.Click += (_, _) =>
        {
            var text = input.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            var added = ViewModel.AddComment(text);
            if (added != null)
            {
                comments.Insert(0, added);
                emptyText.Visibility = Visibility.Collapsed;
                input.Text = string.Empty;
            }
        };

        var inputRow = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 8, 0, 0) };
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(input, 0);
        Grid.SetColumn(addButton, 1);
        inputRow.Children.Add(input);
        inputRow.Children.Add(addButton);

        var panel = new StackPanel { Spacing = 6, MinWidth = 360 };
        panel.Children.Add(emptyText);
        panel.Children.Add(list);
        panel.Children.Add(inputRow);

        var dialog = new ContentDialog
        {
            Title = "Komentarze",
            CloseButtonText = "Zamknij",
            Content = panel,
            XamlRoot = this.Content.XamlRoot,
        };

        await dialog.ShowAsync();
    }

    private void RepostButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Post post)
        {
            ViewModel.SetSelectedPost(post);
            ViewModel.RepostCommand.Execute(null);
        }
    }

    /// <summary>Klik w nazwę autora posta otwiera jego profil.</summary>
    private void AuthorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is PostAuthor author &&
            !string.IsNullOrWhiteSpace(author.Login))
        {
            Frame?.Navigate(typeof(ProfilePage), author.Login);
        }
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Post post)
        {
            ViewModel.SetSelectedPost(post);
            ViewModel.LikeCommand.Execute(null);
        }
    }

    private void BookmarkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Post post)
        {
            ViewModel.SetSelectedPost(post);
            ViewModel.ToggleBookmarkCommand.Execute(null);
        }
    }

    /// <summary>Menu posta — usuwanie dostępne tylko dla własnych postów.</summary>
    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Post post) return;

        var flyout = new MenuFlyout();
        if (post.Author.Login == ViewModel.CurrentUserLogin)
        {
            var delete = new MenuFlyoutItem
            {
                Text = "Usuń post",
                Icon = new FontIcon { Glyph = "" },
            };
            delete.Click += (_, _) =>
            {
                ViewModel.SetSelectedPost(post);
                ViewModel.DeletePostCommand.Execute(null);
            };
            flyout.Items.Add(delete);
        }
        else
        {
            flyout.Items.Add(new MenuFlyoutItem { Text = "Zgłoś post", IsEnabled = false });
        }

        flyout.ShowAt(button);
    }

    /// <summary>Udostępnij — kopiuje treść posta do schowka.</summary>
    private void ShareButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Post post) return;

        var data = new Windows.ApplicationModel.DataTransfer.DataPackage();
        data.SetText(post.Tekst);
        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(data);

        var flyout = new Flyout
        {
            Content = new TextBlock { Text = "Skopiowano treść posta do schowka" },
        };
        flyout.ShowAt(button);
    }
}
