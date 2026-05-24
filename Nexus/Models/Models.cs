using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nexus.Models;

/// <summary>
/// Model autora posta — dane wyświetlane przy każdym poście.
/// </summary>
public class PostAuthor
{
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    public string DisplayName => $"{Imie} {Nazwisko}";
    public string AtLogin => $"@{Login}";
}

/// <summary>
/// Model posta w feedzie — odpowiednik PostData z designu React.
/// </summary>
public partial class Post : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public PostAuthor Author { get; set; } = new();
    public string Tekst { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public List<string> Hashtags { get; set; } = [];
    public string DataUtworzenia { get; set; } = string.Empty;

    private int _likesCount;
    public int LikesCount
    {
        get => _likesCount;
        set => SetProperty(ref _likesCount, value);
    }

    private int _commentsCount;
    public int CommentsCount
    {
        get => _commentsCount;
        set => SetProperty(ref _commentsCount, value);
    }

    private int _retweetsCount;
    public int RetweetsCount
    {
        get => _retweetsCount;
        set => SetProperty(ref _retweetsCount, value);
    }

    private bool _liked;
    public bool Liked
    {
        get => _liked;
        set => SetProperty(ref _liked, value);
    }

    private bool _bookmarked;
    public bool Bookmarked
    {
        get => _bookmarked;
        set => SetProperty(ref _bookmarked, value);
    }
}

/// <summary>
/// Model powiadomienia.
/// </summary>
public class Notification
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Like, Follow, Repost, Reply, Mention
    public PostAuthor User { get; set; } = new();
    public string? Preview { get; set; }
    public string Time { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}

/// <summary>
/// Model konwersacji na liście wiadomości.
/// </summary>
public class Conversation
{
    public string Id { get; set; } = string.Empty;
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    public string DisplayName => $"{Imie} {Nazwisko}";
}

/// <summary>
/// Pojedyncza wiadomość w czacie.
/// </summary>
public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public string Tekst { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    /// <summary>
    /// Czy wiadomość jest od zalogowanego użytkownika.
    /// Wymaga ustawienia currentUserId przez DataService.
    /// </summary>
    public int CurrentUserId { get; set; }

    public bool IsOwn => SenderId == CurrentUserId;
}

/// <summary>
/// Model popularnego hashtagu (strona Explore / RightPanel).
/// </summary>
public class TrendingHashtag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PostsCount { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string DisplayName => $"#{Name}";
}

/// <summary>
/// Model komentarza na poście.
/// </summary>
public class Comment
{
    public int Id { get; set; }
    public PostAuthor Author { get; set; } = new();
    public string Content { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public List<Comment> Replies { get; set; } = [];
}

/// <summary>
/// Sugerowany użytkownik do obserwowania.
/// </summary>
public class SuggestedUser
{
    public int Id { get; set; }
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsFollowing { get; set; }

    public string DisplayName => $"{Imie} {Nazwisko}";
    public string AtLogin => $"@{Login}";
}

/// <summary>
/// Profil aktualnego użytkownika.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string DataUrodzenia { get; set; } = string.Empty;
    public string Lokalizacja { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string DataUtworzenia { get; set; } = string.Empty;
    public int FollowCount { get; set; }
    public int FollowersCount { get; set; }
    public int PostsCount { get; set; }

    public string DisplayName => $"{Imie} {Nazwisko}";
    public string AtLogin => $"@{Login}";
}

/// <summary>
/// Sesja aktywna użytkownika (Settings).
/// </summary>
public class UserSession
{
    public int Id { get; set; }
    public string Device { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
    public bool Current { get; set; }
}
