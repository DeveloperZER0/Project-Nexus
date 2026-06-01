using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Models;
using Nexus.ViewModels;

namespace Nexus.Views;

public sealed partial class MessagesPage : Page
{
    public MessagesViewModel ViewModel { get; }

    public MessagesPage()
    {
        InitializeComponent();
        ViewModel = ((App)Application.Current).Services.GetRequiredService<MessagesViewModel>();
        ConversationsListView.ItemsSource = ViewModel.Conversations;
        ChatMessagesListView.ItemsSource = ViewModel.ChatMessages;
    }

    /// <summary>
    /// Wybór konwersacji — aktualizacja nagłówka czatu i widoczności paneli.
    /// </summary>
    private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ConversationsListView.SelectedItem is Conversation conv)
        {
            ViewModel.SelectedConversation = conv;

            // Pokaż nagłówek czatu
            ChatHeader.Visibility = Visibility.Visible;
            ChatMessagesListView.Visibility = Visibility.Visible;
            ChatInputBar.Visibility = Visibility.Visible;
            ChatPlaceholder.Visibility = Visibility.Collapsed;

            // Aktualizuj dane nagłówka
            ChatDisplayName.Text = conv.DisplayName;
            ChatLogin.Text = $"@{conv.Login}";

            // Awatar kontaktu w nagłówku
            try
            {
                ChatAvatarBorder.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(conv.AvatarUrl))
                    {
                        CreateOptions = BitmapCreateOptions.IgnoreImageCache,
                        DecodePixelWidth = 72,
                    },
                    Stretch = Stretch.UniformToFill,
                };
            }
            catch
            {
                // Gracefully handle image loading errors
            }
        }
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.MessageText = MessageInput.Text;
        ViewModel.SendMessageCommand.Execute(null);
        MessageInput.Text = string.Empty;
    }

    /// <summary>
    /// Dialog rozpoczęcia nowej konwersacji — wyszukiwarka osób + wybór odbiorcy.
    /// </summary>
    private async void NewConversation_Click(object sender, RoutedEventArgs e)
    {
        var users = new ObservableCollection<SuggestedUser>(ViewModel.FindUsersToMessage(null));

        var searchBox = new TextBox { PlaceholderText = "Szukaj osób..." };
        var list = new ListView
        {
            ItemsSource = users,
            ItemTemplate = (DataTemplate)Resources["MessageUserTemplate"],
            SelectionMode = ListViewSelectionMode.Single,
            MaxHeight = 320,
            MinWidth = 320,
        };

        searchBox.TextChanged += (_, _) =>
        {
            users.Clear();
            foreach (var u in ViewModel.FindUsersToMessage(searchBox.Text))
                users.Add(u);
        };

        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(searchBox);
        panel.Children.Add(list);

        var dialog = new ContentDialog
        {
            Title = "Nowa wiadomość",
            PrimaryButtonText = "Rozpocznij",
            CloseButtonText = "Anuluj",
            DefaultButton = ContentDialogButton.Primary,
            Content = panel,
            XamlRoot = this.Content.XamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && list.SelectedItem is SuggestedUser selected)
        {
            ViewModel.CreateNewConversationCommand.Execute(selected);
        }
    }
}
