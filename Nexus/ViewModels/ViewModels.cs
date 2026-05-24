using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nexus.Data;
using Nexus.Data.Entities;
using Nexus.Models;

namespace Nexus.ViewModels;

/// <summary>
/// ViewModel strony głównej — feed postów z zakładkami "Dla Ciebie" / "Obserwowani".
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;
    private Dictionary<int, Post> _postLookup = new();

    [ObservableProperty]
    public partial string SelectedTab { get; set; }

    [ObservableProperty]
    public partial string ComposeText { get; set; }

    [ObservableProperty]
    public partial Post? SelectedPost { get; set; }

    public ObservableCollection<Post> Posts { get; }

    /// <summary>
    /// Pozostała liczba znaków w polu compose (max 280).
    /// </summary>
    public int RemainingChars => 280 - (ComposeText?.Length ?? 0);

    public HomeViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        Posts = new ObservableCollection<Post>();
        ComposeText = string.Empty;
        // Ustawienie SelectedTab triggeruje OnSelectedTabChanged → RefreshFeed,
        // który ładuje początkowy feed "Dla Ciebie".
        SelectedTab = "forYou";
    }

    partial void OnSelectedTabChanged(string value)
    {
        RefreshFeed();
    }

    private void RefreshFeed()
    {
        List<Post> posts = SelectedTab switch
        {
            "following" => _dataService.GetFeedPostsFollowing(_currentUserId),
            _ => _dataService.GetFeedPostsForYou(_currentUserId),
        };

        Posts.Clear();
        _postLookup.Clear();
        foreach (var post in posts)
        {
            Posts.Add(post);
            if (int.TryParse(post.Id, out var id))
                _postLookup[id] = post;
        }
    }

    public void SetSelectedPost(Post? post)
    {
        SelectedPost = post;
    }

    public void AddOrUpdatePost(Post post)
    {
        if (!int.TryParse(post.Id, out var id))
        {
            return;
        }

        if (_postLookup.TryGetValue(id, out var existing))
        {
            existing.LikesCount = post.LikesCount;
            existing.CommentsCount = post.CommentsCount;
            existing.RetweetsCount = post.RetweetsCount;
            existing.Bookmarked = post.Bookmarked;
        }
        else
        {
            _postLookup[id] = post;
        }
        OnPropertyChanged(nameof(Posts));
    }

    [RelayCommand]
    private void ToggleBookmark()
    {
        if (SelectedPost == null || !int.TryParse(SelectedPost.Id, out var postId)) return;
        var bookmarked = _dataService.ToggleBookmark(_currentUserId, postId);
        SelectedPost.Bookmarked = bookmarked;
        AddOrUpdatePost(SelectedPost);
    }

    [RelayCommand]
    private void Like()
    {
        if (SelectedPost == null || !int.TryParse(SelectedPost.Id, out var postId)) return;
        SelectedPost.Liked = _dataService.ToggleLikePost(postId, _currentUserId);
        SelectedPost.LikesCount += SelectedPost.Liked ? 1 : -1;
        AddOrUpdatePost(SelectedPost);
    }

    [RelayCommand]
    private void Repost()
    {
        if (SelectedPost == null || !int.TryParse(SelectedPost.Id, out var postId)) return;
        var retweeted = _dataService.ToggleRetweet(postId, _currentUserId);
        SelectedPost.RetweetsCount += retweeted ? 1 : -1;
        AddOrUpdatePost(SelectedPost);
    }

    [RelayCommand]
    private void Comment()
    {
        // Bez treści komentarza (UI nie ma jeszcze formularza); zwiększ tylko licznik.
        // Faktyczne dodawanie komentarza odbywa się przez AddComment z dedykowanego dialogu.
        if (SelectedPost == null || !int.TryParse(SelectedPost.Id, out var postId)) return;
        SelectedPost.CommentsCount = _dataService.IncrementComment(postId);
        AddOrUpdatePost(SelectedPost);
    }

    /// <summary>Dodaje rzeczywisty komentarz (wywoływane np. z dialogu).</summary>
    public Comment? AddComment(string content)
    {
        if (SelectedPost == null || !int.TryParse(SelectedPost.Id, out var postId)) return null;
        if (string.IsNullOrWhiteSpace(content)) return null;

        var comment = _dataService.AddComment(postId, _currentUserId, content);
        // Po dodaniu komentarza odśwież licznik z bazy (AddComment już go zwiększył)
        SelectedPost.CommentsCount += 1;
        AddOrUpdatePost(SelectedPost);
        return comment;
    }

    partial void OnComposeTextChanged(string value)
    {
        OnPropertyChanged(nameof(RemainingChars));
        PublishCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanPublish))]
    private void Publish()
    {
        if (string.IsNullOrWhiteSpace(ComposeText)) return;

        var newPost = _dataService.AddPost(_currentUserId, ComposeText);
        // Świeży post — domyślnie nielubię ani nie zapisałem
        newPost.Liked = false;
        newPost.Bookmarked = false;
        Posts.Insert(0, newPost);

        if (int.TryParse(newPost.Id, out var id))
            _postLookup[id] = newPost;

        ComposeText = string.Empty;
    }

    private bool CanPublish() => !string.IsNullOrWhiteSpace(ComposeText);

    [RelayCommand]
    private void DeletePost()
    {
        if (SelectedPost == null || !int.TryParse(SelectedPost.Id, out var postId)) return;
        if (_dataService.DeletePost(postId))
        {
            Posts.Remove(SelectedPost);
            SelectedPost = null;
        }
    }
}

/// <summary>
/// ViewModel strony Explore — wyszukiwanie i popularne hashtagi.
/// </summary>
public partial class ExploreViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial string SelectedCategory { get; set; }

    public ObservableCollection<string> Categories { get; } =
        new(["Wszystko", "Technologia", "Gaming", "Muzyka", "Sport", "Nauka"]);

    public ObservableCollection<TrendingHashtag> TrendingHashtags { get; }

    public ObservableCollection<Post> SearchedPosts { get; }

    public ObservableCollection<SuggestedUser> SearchedUsers { get; }

    public ExploreViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        TrendingHashtags = new ObservableCollection<TrendingHashtag>(_dataService.GetTrendingHashtags());
        SearchedPosts = new ObservableCollection<Post>();
        SearchedUsers = new ObservableCollection<SuggestedUser>();
        SearchQuery = string.Empty;
        SelectedCategory = "Wszystko";
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SearchedPosts.Clear();
            SearchedUsers.Clear();
            return;
        }

        PerformSearch(value);
    }

    private void PerformSearch(string query)
    {
        SearchedPosts.Clear();
        SearchedUsers.Clear();

        var posts = _dataService.SearchPosts(query);
        var users = _dataService.SearchUsers(query);

        foreach (var post in posts)
            SearchedPosts.Add(post);

        foreach (var user in users)
            SearchedUsers.Add(user);
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        SelectedCategory = category;
    }

    [RelayCommand]
    private void SelectTrendingTopic(TrendingHashtag? hashtag)
    {
        if (hashtag != null)
            SearchQuery = "#" + hashtag.Name;
    }
}

/// <summary>
/// ViewModel powiadomień z zakładkami Wszystkie / Wzmianki.
/// </summary>
public partial class NotificationsViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial string SelectedTab { get; set; }

    private readonly List<Notification> _allNotifications;

    public ObservableCollection<Notification> Notifications { get; }

    public NotificationsViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        _allNotifications = _dataService.GetNotifications(_currentUserId);
        Notifications = new ObservableCollection<Notification>(_allNotifications);

        // Oznacz wszystkie powiadomienia jako przeczytane przy wejściu na stronę.
        _dataService.MarkAllNotificationsRead(_currentUserId);
        foreach (var n in _allNotifications) n.IsRead = true;

        SelectedTab = "all";
    }

    partial void OnSelectedTabChanged(string value)
    {
        Notifications.Clear();
        var filtered = value == "mentions"
            ? _allNotifications.Where(n => n.Type == "Mention")
            : _allNotifications;

        foreach (var n in filtered)
            Notifications.Add(n);
    }

    /// <summary>
    /// Zwraca glyph ikony Segoe MDL2 Assets na podstawie typu powiadomienia.
    /// </summary>
    public static string GetNotificationIcon(string type) => type switch
    {
        "Like" => "\uEB51",      // Heart
        "Follow" => "\uE8FA",    // AddFriend
        "Repost" => "\uE8EB",    // RepeatAll
        "Reply" => "\uE8BD",     // Comment
        "Mention" => "\uE910",   // Mention
        _ => "\uE7C8",           // ActionCenter
    };

    /// <summary>
    /// Zwraca etykietę po polsku na podstawie typu powiadomienia.
    /// </summary>
    public static string GetNotificationLabel(string type) => type switch
    {
        "Like" => "polubił(a) Twój post",
        "Follow" => "zaczął(-ęła) Cię obserwować",
        "Repost" => "udostępnił(a) Twój post",
        "Reply" => "odpowiedział(a) na komentarz",
        "Mention" => "wspomniał(a) o Tobie",
        _ => "",
    };
}

/// <summary>
/// ViewModel wiadomości — lista konwersacji + okno czatu.
/// </summary>
public partial class MessagesViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial Conversation? SelectedConversation { get; set; }

    [ObservableProperty]
    public partial string MessageText { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    public ObservableCollection<Conversation> Conversations { get; }

    public ObservableCollection<ChatMessage> ChatMessages { get; }

    public MessagesViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        Conversations = new ObservableCollection<Conversation>(_dataService.GetConversations(_currentUserId));
        ChatMessages = new ObservableCollection<ChatMessage>();
        MessageText = string.Empty;
        SearchQuery = string.Empty;
    }

    public bool HasSelectedConversation => SelectedConversation != null;

    partial void OnSelectedConversationChanged(Conversation? value)
    {
        OnPropertyChanged(nameof(HasSelectedConversation));

        ChatMessages.Clear();
        if (value != null && int.TryParse(value.Id, out var conversationId))
        {
            foreach (var message in _dataService.GetMessages(conversationId))
            {
                ChatMessages.Add(message);
            }
        }
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;

        if (SelectedConversation == null || !int.TryParse(SelectedConversation.Id, out var conversationId))
        {
            return;
        }

        var message = _dataService.AddMessage(conversationId, _currentUserId, MessageText);
        ChatMessages.Add(message);

        MessageText = string.Empty;
    }

    [RelayCommand]
    private void CreateNewConversation(SuggestedUser? user)
    {
        if (user == null) return;

        var conversationId = _dataService.CreateConversation(_currentUserId, user.Id);

        // Sprawdź czy konwersacja już jest na liście
        var existing = Conversations.FirstOrDefault(c => c.Login == user.Login);
        if (existing != null)
        {
            SelectedConversation = existing;
            return;
        }

        // Dodaj nową konwersację bezpośrednio zamiast przeładowywać całą listę
        var newConversation = new Conversation
        {
            Id = conversationId.ToString(),
            Imie = user.Imie,
            Nazwisko = user.Nazwisko,
            Login = user.Login,
            AvatarUrl = user.AvatarUrl,
            LastMessage = string.Empty,
            Time = DateTime.Now.ToString("g", new System.Globalization.CultureInfo("pl-PL")),
            IsRead = true,
        };

        Conversations.Insert(0, newConversation);
        SelectedConversation = newConversation;
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            // Pokazuj wszystkie konwersacje
            Conversations.Clear();
            foreach (var c in _dataService.GetConversations(_currentUserId))
                Conversations.Add(c);
            return;
        }

        // Filtruj konwersacje
        var filtered = _dataService.GetConversations(_currentUserId)
            .Where(c => c.Imie.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                       c.Nazwisko.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                       c.Login.Contains(value, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Conversations.Clear();
        foreach (var c in filtered)
            Conversations.Add(c);
    }
}

/// <summary>
/// ViewModel profilu użytkownika.
/// </summary>
public partial class ProfileViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial string SelectedTab { get; set; }

    [ObservableProperty]
    public partial UserProfile? User { get; set; }

    public ObservableCollection<Post> Posts { get; }

    public ProfileViewModel(NexusDataService dataService, string? userHandle = null)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        Posts = new ObservableCollection<Post>();
        SelectedTab = "posts";

        if (!string.IsNullOrWhiteSpace(userHandle))
        {
            LoadProfileByHandle(userHandle);
        }
        else
        {
            LoadCurrentUserProfile();
        }
    }

    public void LoadCurrentUserProfile()
    {
        User = _dataService.GetCurrentUserProfile();
        RefreshPosts();
    }

    public void LoadProfileByHandle(string handle)
    {
        User = _dataService.GetUserProfileByHandle(handle);
        RefreshPosts();
    }

    partial void OnSelectedTabChanged(string value)
    {
        RefreshPosts();
    }

    private void RefreshPosts()
    {
        if (User == null) return;

        Posts.Clear();

        List<Post> posts = SelectedTab switch
        {
            "likes" => _dataService.GetLikedPosts(User.Id),
            // "replies" => _dataService.GetUserRepliesAsync(User.Id), // TODO: Dodać później
            // "media" => _dataService.GetUserMediaPostsAsync(User.Id), // TODO: Dodać później
            _ => _dataService.GetUserPosts(User.Id),
        };

        foreach (var post in posts)
            Posts.Add(post);
    }

    public string JoinedDate
    {
        get
        {
            if (User != null && DateTime.TryParse(User.DataUtworzenia, out var d))
                return d.ToString("MMMM yyyy", new System.Globalization.CultureInfo("pl-PL"));
            return User?.DataUtworzenia ?? "";
        }
    }

    public string BirthDate
    {
        get
        {
            if (User != null && DateTime.TryParse(User.DataUrodzenia, out var d))
                return d.ToString("dd.MM.yyyy");
            return User?.DataUrodzenia ?? "";
        }
    }

    public bool IsCurrentUser => User?.Id == _currentUserId;

    [RelayCommand]
    private void EditProfile()
    {
        // Implementacja edycji profilu
    }

    [RelayCommand]
    private void Follow()
    {
        if (User == null) return;
        _dataService.ToggleFollow(_currentUserId, User.Id);
    }
}

/// <summary>
/// ViewModel zakładek (bookmarks).
/// </summary>
public partial class BookmarksViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;
    private readonly string _login;
    public ObservableCollection<Post> Posts { get; }

    public string Subtitle => $"@{_login} · {Posts.Count} zapisanych postów";

    public BookmarksViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        _login = _dataService.GetCurrentUserProfile().Login;
        Posts = new ObservableCollection<Post>(_dataService.GetBookmarkedPosts(_currentUserId));
        Posts.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Subtitle));
    }

    [RelayCommand]
    private void ToggleLike(Post? post)
    {
        if (post == null || !int.TryParse(post.Id, out var postId)) return;
        post.Liked = _dataService.ToggleLikePost(postId, _currentUserId);
        post.LikesCount += post.Liked ? 1 : -1;
    }

    [RelayCommand]
    private void ToggleRetweet(Post? post)
    {
        if (post == null || !int.TryParse(post.Id, out var postId)) return;
        var retweeted = _dataService.ToggleRetweet(postId, _currentUserId);
        post.RetweetsCount += retweeted ? 1 : -1;
    }

    [RelayCommand]
    private void RemoveBookmark(Post? post)
    {
        if (post == null || !int.TryParse(post.Id, out var postId)) return;
        _dataService.ToggleBookmark(_currentUserId, postId);
        Posts.Remove(post);
    }
}

/// <summary>
/// ViewModel ustawień — sekcje konta, bezpieczeństwa, powiadomień, prywatności, motywu.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;
    private readonly UserSettingsEntity _settings;
    // Flaga zapobiega zapisywaniu w bazie ustawień podczas inicjalizacji,
    // gdy partial property setter jest wywoływany po raz pierwszy.
    private bool _initialized;

    [ObservableProperty]
    public partial bool EmailNotifications { get; set; }

    [ObservableProperty]
    public partial bool PushNotifications { get; set; }

    [ObservableProperty]
    public partial string PrivacyLevel { get; set; }

    [ObservableProperty]
    public partial string Theme { get; set; }

    [ObservableProperty]
    public partial string Language { get; set; }

    public UserProfile User { get; }

    public ObservableCollection<UserSession> Sessions { get; }

    public List<string> PrivacyOptions { get; } = ["Public", "Friends", "Private"];
    public List<string> ThemeOptions { get; } = ["Dark", "Light", "System"];
    public List<string> LanguageOptions { get; } = ["pl-PL", "en-US", "de-DE"];

    public SettingsViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        User = _dataService.GetCurrentUserProfile();
        Sessions = new ObservableCollection<UserSession>(_dataService.GetSessions(_currentUserId));
        _settings = _dataService.GetUserSettings(_currentUserId);
        EmailNotifications = _settings.EmailNotifications;
        PushNotifications = _settings.PushNotifications;
        PrivacyLevel = _settings.PrivacyLevel;
        Theme = _settings.Theme;
        Language = _settings.Language;
        _initialized = true;
    }

    partial void OnEmailNotificationsChanged(bool value)
    {
        if (!_initialized) return;
        _settings.EmailNotifications = value;
        _dataService.SaveUserSettings(_settings);
    }

    partial void OnPushNotificationsChanged(bool value)
    {
        if (!_initialized) return;
        _settings.PushNotifications = value;
        _dataService.SaveUserSettings(_settings);
    }

    partial void OnPrivacyLevelChanged(string value)
    {
        if (!_initialized) return;
        _settings.PrivacyLevel = value;
        _dataService.SaveUserSettings(_settings);
    }

    partial void OnThemeChanged(string value)
    {
        if (!_initialized) return;
        _settings.Theme = value;
        _dataService.SaveUserSettings(_settings);
        if (App.Current is App app)
        {
            app.ApplyTheme(value);
        }
    }

    partial void OnLanguageChanged(string value)
    {
        if (!_initialized) return;
        _settings.Language = value;
        _dataService.SaveUserSettings(_settings);
    }

    /// <summary>Emitowany po wylogowaniu lub usunięciu konta. App nawiguje do ekranu logowania.</summary>
    public event Action? LoggedOut;

    [RelayCommand]
    private void ChangePassword()
    {
        // Dialog do zmiany hasła jest obsługiwany przez code-behind SettingsPage.
        // ViewModel udostępnia rzeczywistą zmianę przez TryChangePassword poniżej.
    }

    /// <summary>Zmienia hasło, gdy stare jest poprawne. Wywoływane z dialogu.</summary>
    public bool TryChangePassword(string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8) return false;
        return _dataService.ChangePassword(_currentUserId, oldPassword, newPassword);
    }

    [RelayCommand]
    private async Task DeleteAccount()
    {
        if (_dataService.DeleteAccount(_currentUserId))
        {
            await _dataService.LogoutAsync();
            LoggedOut?.Invoke();
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _dataService.LogoutAsync();
        LoggedOut?.Invoke();
    }
}

/// <summary>
/// ViewModel głównego okna — nawigacja między stronami.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial string SearchQuery { get; set; }

    [ObservableProperty]
    public partial int UnreadNotifications { get; set; }

    [ObservableProperty]
    public partial int UnreadMessages { get; set; }

    public UserProfile CurrentUser { get; }
    public ObservableCollection<SuggestedUser> SuggestedUsers { get; }
    public ObservableCollection<TrendingHashtag> TrendingHashtags { get; }

    public bool HasUnreadNotifications => UnreadNotifications > 0;
    public bool HasUnreadMessages => UnreadMessages > 0;

    public ShellViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        _currentUserId = _dataService.GetCurrentUserId();
        CurrentUser = _dataService.GetCurrentUserProfile();
        SuggestedUsers = new ObservableCollection<SuggestedUser>(_dataService.GetSuggestedUsers(_currentUserId));
        TrendingHashtags = new ObservableCollection<TrendingHashtag>(_dataService.GetTrendingHashtags(5));
        SearchQuery = string.Empty;
        RefreshBadges();
    }

    public void RefreshBadges()
    {
        UnreadNotifications = _dataService.GetUnreadNotificationCount(_currentUserId);
        UnreadMessages = _dataService.GetUnreadConversationCount(_currentUserId);
    }

    partial void OnUnreadNotificationsChanged(int value) => OnPropertyChanged(nameof(HasUnreadNotifications));
    partial void OnUnreadMessagesChanged(int value) => OnPropertyChanged(nameof(HasUnreadMessages));

    public void ToggleFollow(SuggestedUser user)
    {
        if (user == null) return;
        user.IsFollowing = _dataService.ToggleFollow(_currentUserId, user.Id);
        OnPropertyChanged(nameof(SuggestedUsers));
    }
}

/// <summary>
/// ViewModel logowania — autentykacja użytkownika.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;

    [ObservableProperty]
    public partial string Login { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public LoginViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        Login = string.Empty;
        Password = string.Empty;
    }

    /// <summary>Emitowany po pomyślnym zalogowaniu (do podłączenia nawigacji w App).</summary>
    public event Action? LoginSucceeded;

    [RelayCommand]
    private void ExecuteLogin()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Podaj login i hasło";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (_dataService.AuthenticateUser(Login, Password))
            {
                LoginSucceeded?.Invoke();
            }
            else
            {
                ErrorMessage = "Nieprawidłowy login lub hasło";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd logowania: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoToRegister()
    {
        // Nawigacja do rejestracji - obsługiwane w App
    }
}

/// <summary>
/// ViewModel rejestracji — tworzenie nowego konta.
/// </summary>
public partial class RegisterViewModel : ObservableObject
{
    private readonly NexusDataService _dataService;

    [ObservableProperty]
    public partial string Login { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial string FirstName { get; set; }

    [ObservableProperty]
    public partial string LastName { get; set; }

    [ObservableProperty]
    public partial string Password { get; set; }

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public RegisterViewModel(NexusDataService dataService)
    {
        _dataService = dataService;
        Login = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    public event Action? RegistrationSucceeded;

    private static readonly System.Text.RegularExpressions.Regex EmailRegex = new(
        @"^[^\s@]+@[^\s@]+\.[^\s@]+$",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    [RelayCommand]
    private void Register()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Wypełnij wszystkie pola";
            return;
        }

        if (!EmailRegex.IsMatch(Email))
        {
            ErrorMessage = "Nieprawidłowy adres email";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Hasła się nie zgadzają";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Hasło musi mieć co najmniej 8 znaków";
            return;
        }

        IsLoading = true;

        try
        {
            var success = _dataService.RegisterUser(Login, Email, Password, FirstName, LastName);
            if (!success)
            {
                ErrorMessage = "Login lub email już istnieje";
                return;
            }

            RegistrationSucceeded?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Błąd rejestracji: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoToLogin()
    {
        // Nawigacja do logowania - obsługiwane w App
    }
}
