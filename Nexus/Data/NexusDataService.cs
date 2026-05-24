using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexus.Data.Entities;
using Nexus.Models;
using BCryptHelper = BCrypt.Net.BCrypt;

namespace Nexus.Data;

public class NexusDataService
{
    private readonly DbContextOptions<NexusDbContext> _options;
    private int _currentUserId = -1;

    public NexusDataService(DbContextOptions<NexusDbContext> options)
    {
        _options = options;
    }

    private NexusDbContext CreateContext() => new(_options);

    /// <summary>
    /// Zwraca ID aktualnie zalogowanego użytkownika (ustawiony przez AuthenticateUser),
    /// lub -1 jeśli nikt nie jest zalogowany.
    /// </summary>
    public int GetCurrentUserId() => _currentUserId > 0 ? _currentUserId : -1;

    public bool IsAuthenticated => _currentUserId > 0;

    public void SetCurrentUserId(int userId)
    {
        _currentUserId = userId;
    }

    public bool AuthenticateUser(string login, string password)
    {
        using var db = CreateContext();
        var user = db.Users.FirstOrDefault(u => u.Login == login);
        if (user == null) return false;

        // Weryfikacja hasła przy użyciu BCrypt
        if (!BCryptHelper.Verify(password, user.PasswordHash))
            return false;

        _currentUserId = user.Id;
        return true;
    }

    public void Logout()
    {
        _currentUserId = -1;
    }

    public async Task LogoutAsync()
    {
        using var db = CreateContext();
        // Usuń sesję użytkownika
        if (_currentUserId > 0)
        {
            var sessions = db.Sessions.Where(s => s.UserId == _currentUserId);
            db.Sessions.RemoveRange(sessions);
            await db.SaveChangesAsync();
        }
        _currentUserId = -1;
    }

    public UserProfile GetCurrentUserProfile()
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            return new UserProfile();
        }

        using var db = CreateContext();
        var user = db.Users
            .Include(u => u.Profile)
            .Include(u => u.Following)
            .Include(u => u.Followers)
            .Include(u => u.Posts)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return new UserProfile();

        return MapUserProfile(user, user.Profile);
    }

    public List<Post> GetFeedPosts()
    {
        using var db = CreateContext();
        var currentUserId = GetCurrentUserId();
        var posts = db.Posts
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, currentUserId)).ToList();
    }

    public List<Post> GetUserPosts(int userId)
    {
        using var db = CreateContext();
        var currentUserId = GetCurrentUserId();
        var posts = db.Posts
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, currentUserId)).ToList();
    }

    public List<Post> GetBookmarkedPosts(int userId)
    {
        using var db = CreateContext();
        var currentUserId = GetCurrentUserId();
        var posts = db.Bookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.Post!)
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, currentUserId)).ToList();
    }

    public List<Post> GetLikedPosts(int userId)
    {
        using var db = CreateContext();
        var currentUserId = GetCurrentUserId();
        var posts = db.Likes
            .Where(l => l.UserId == userId)
            .Select(l => l.Post!)
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, currentUserId)).ToList();
    }

    public List<TrendingHashtag> GetTrendingHashtags(int take = 10)
    {
        using var db = CreateContext();
        var tags = db.PostHashtags
            .GroupBy(h => h.Tag)
            .Select(g => new { Tag = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(take)
            .ToList();

        var id = 1;
        return tags.Select(t => new TrendingHashtag
        {
            Id = id++,
            Name = t.Tag.TrimStart('#'),
            PostsCount = t.Count.ToString("N0", CultureInfo.InvariantCulture),
            Category = "",
            Description = "",
        }).ToList();
    }

    public List<SuggestedUser> GetSuggestedUsers(int userId)
    {
        using var db = CreateContext();
        var followingIds = db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToHashSet();

        var users = db.Users
            .Include(u => u.Profile)
            .Where(u => u.Id != userId)
            .OrderBy(u => u.Profile!.FirstName)
            .Take(3)
            .ToList();

        return users.Select(u => new SuggestedUser
        {
            Id = u.Id,
            Imie = u.Profile?.FirstName ?? string.Empty,
            Nazwisko = u.Profile?.LastName ?? string.Empty,
            Login = u.Login,
            AvatarUrl = u.Profile?.AvatarUrl ?? string.Empty,
            IsFollowing = followingIds.Contains(u.Id),
        }).ToList();
    }

    public bool ToggleFollow(int followerId, int followingId)
    {
        using var db = CreateContext();
        var existing = db.Follows.FirstOrDefault(f => f.FollowerId == followerId && f.FollowingId == followingId);
        if (existing != null)
        {
            db.Follows.Remove(existing);
            db.SaveChanges();
            return false;
        }

        db.Follows.Add(new FollowEntity
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow,
        });

        if (followerId != followingId)
        {
            db.Notifications.Add(new NotificationEntity
            {
                RecipientId = followingId,
                ActorId = followerId,
                Type = "Follow",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            });
        }

        db.SaveChanges();
        return true;
    }

    public UserSettingsEntity GetUserSettings(int userId)
    {
        using var db = CreateContext();
        var settings = db.UserSettings.FirstOrDefault(s => s.UserId == userId);
        if (settings != null)
        {
            return settings;
        }

        if (userId <= 0 || !db.Users.Any(u => u.Id == userId))
        {
            throw new InvalidOperationException("Nie znaleziono bieżącego użytkownika.");
        }

        settings = new UserSettingsEntity
        {
            UserId = userId,
            EmailNotifications = true,
            PushNotifications = true,
            PrivacyLevel = "Public",
            Theme = "Dark",
            Language = "pl-PL",
        };

        db.UserSettings.Add(settings);
        db.SaveChanges();
        return settings;
    }

    public void SaveUserSettings(UserSettingsEntity settings)
    {
        using var db = CreateContext();
        db.UserSettings.Update(settings);
        db.SaveChanges();
    }

    public bool ToggleBookmark(int userId, int postId)
    {
        using var db = CreateContext();
        var existing = db.Bookmarks.FirstOrDefault(b => b.UserId == userId && b.PostId == postId);
        if (existing != null)
        {
            db.Bookmarks.Remove(existing);
            db.SaveChanges();
            return false;
        }

        db.Bookmarks.Add(new BookmarkEntity { UserId = userId, PostId = postId });
        db.SaveChanges();
        return true;
    }

    public int IncrementLike(int postId)
    {
        using var db = CreateContext();
        db.Posts.Where(p => p.Id == postId)
            .ExecuteUpdate(s => s.SetProperty(p => p.LikesCount, p => p.LikesCount + 1));
        return db.Posts.Where(p => p.Id == postId).Select(p => p.LikesCount).First();
    }

    public int IncrementRepost(int postId)
    {
        using var db = CreateContext();
        db.Posts.Where(p => p.Id == postId)
            .ExecuteUpdate(s => s.SetProperty(p => p.RetweetsCount, p => p.RetweetsCount + 1));
        return db.Posts.Where(p => p.Id == postId).Select(p => p.RetweetsCount).First();
    }

    public int IncrementComment(int postId)
    {
        using var db = CreateContext();
        db.Posts.Where(p => p.Id == postId)
            .ExecuteUpdate(s => s.SetProperty(p => p.CommentsCount, p => p.CommentsCount + 1));
        return db.Posts.Where(p => p.Id == postId).Select(p => p.CommentsCount).First();
    }

    public List<Notification> GetNotifications(int userId)
    {
        using var db = CreateContext();
        var notifications = db.Notifications
            .Include(n => n.Actor)
                .ThenInclude(a => a!.Profile)
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return notifications.Select(n => new Notification
        {
            Id = n.Id.ToString(),
            Type = n.Type,
            User = MapAuthor(n.Actor, n.Actor?.Profile),
            Preview = n.Preview,
            Time = ToDisplayTime(n.CreatedAt),
            IsRead = n.IsRead,
        }).ToList();
    }

    public List<Conversation> GetConversations(int userId)
    {
        using var db = CreateContext();
        var conversations = db.Conversations
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                    .ThenInclude(u => u!.Profile)
            .Include(c => c.Messages)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.UpdatedAt)
            .ToList();

        return conversations.Select(c => MapConversationForUser(c, userId)).ToList();
    }

    private static Conversation MapConversationForUser(ConversationEntity conv, int userId)
    {
        var c = MapConversation(conv, userId);
        c.IsRead = !conv.Messages.Any(m => !m.IsRead && m.SenderId != userId);
        return c;
    }

    public List<ChatMessage> GetMessages(int conversationId)
    {
        using var db = CreateContext();
        var messages = db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        var currentUserId = GetCurrentUserId();

        // Oznacz przychodzące wiadomości jako przeczytane
        var unread = messages.Where(m => !m.IsRead && m.SenderId != currentUserId).ToList();
        if (unread.Count > 0)
        {
            foreach (var m in unread) m.IsRead = true;
            db.SaveChanges();
        }

        return messages.Select(m => new ChatMessage
        {
            Id = m.Id.ToString(),
            SenderId = m.SenderId,
            Tekst = m.Text,
            Time = m.CreatedAt.ToString("HH:mm"),
            IsRead = m.IsRead,
            CurrentUserId = currentUserId,
        }).ToList();
    }

    public Post AddPost(int authorId, string text)
    {
        using var db = CreateContext();
        var author = db.Users.Include(u => u.Profile).First(u => u.Id == authorId);
        var post = new PostEntity
        {
            AuthorId = authorId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var tag in ExtractHashtags(text))
        {
            post.Hashtags.Add(new PostHashtagEntity { Tag = tag });
        }

        db.Posts.Add(post);
        db.SaveChanges();

        db.Entry(post).Reference(p => p.Author).CurrentValue = author;
        return MapPost(post);
    }

    public ChatMessage AddMessage(int conversationId, int senderId, string text)
    {
        using var db = CreateContext();
        var conversation = db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefault(c => c.Id == conversationId);

        if (conversation == null)
        {
            return new ChatMessage();
        }

        var message = new MessageEntity
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
        };

        db.Messages.Add(message);

        conversation.LastMessage = text;
        conversation.UpdatedAt = DateTime.UtcNow;

        db.SaveChanges();

        return new ChatMessage
        {
            Id = message.Id.ToString(),
            SenderId = message.SenderId,
            Tekst = message.Text,
            Time = message.CreatedAt.ToString("HH:mm"),
            IsRead = message.IsRead,
            CurrentUserId = senderId,
        };
    }

    public List<UserSession> GetSessions(int userId)
    {
        using var db = CreateContext();
        return db.Sessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Current)
            .Select(s => new UserSession
            {
                Id = s.Id,
                Device = s.Device,
                IconGlyph = s.IconGlyph,
                Location = s.Location,
                Expiry = s.Expiry,
                Current = s.Current,
            }).ToList();
    }

    private static Post MapPost(PostEntity post)
    {
        return new Post
        {
            Id = post.Id.ToString(),
            Author = MapAuthor(post.Author, post.Author?.Profile),
            Tekst = post.Text,
            MediaUrl = post.MediaUrl,
            Hashtags = post.Hashtags.Select(h => h.Tag.TrimStart('#')).ToList(),
            DataUtworzenia = ToDisplayTime(post.CreatedAt),
            LikesCount = post.LikesCount,
            CommentsCount = post.CommentsCount,
            RetweetsCount = post.RetweetsCount,
        };
    }

    private static Post MapPostWithUserStatus(PostEntity post, int userId)
    {
        var postModel = MapPost(post);
        postModel.Liked = post.Likes?.Any(l => l.UserId == userId) ?? false;
        postModel.Bookmarked = post.Bookmarks?.Any(b => b.UserId == userId) ?? false;
        return postModel;
    }

    private static Conversation MapConversation(ConversationEntity conversation, int userId)
    {
        var other = conversation.Participants
            .Select(p => p.User)
            .FirstOrDefault(u => u != null && u.Id != userId);

        return new Conversation
        {
            Id = conversation.Id.ToString(),
            Imie = other?.Profile?.FirstName ?? string.Empty,
            Nazwisko = other?.Profile?.LastName ?? string.Empty,
            Login = other?.Login ?? string.Empty,
            AvatarUrl = other?.Profile?.AvatarUrl ?? string.Empty,
            LastMessage = conversation.LastMessage ?? conversation.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()?.Text ?? string.Empty,
            Time = ToDisplayTime(conversation.UpdatedAt),
            IsRead = true,
        };
    }

    private static PostAuthor MapAuthor(UserEntity? user, UserProfileEntity? profile)
    {
        return new PostAuthor
        {
            Imie = profile?.FirstName ?? string.Empty,
            Nazwisko = profile?.LastName ?? string.Empty,
            Login = user?.Login ?? string.Empty,
            AvatarUrl = profile?.AvatarUrl ?? string.Empty,
        };
    }

    private static UserProfile MapUserProfile(UserEntity user, UserProfileEntity? profile)
    {
        // Wczytaj relacje, jeśli nie zostały załadowane
        var followingCount = user.Following != null ? user.Following.Count : 0;
        var followersCount = user.Followers != null ? user.Followers.Count : 0;
        var postsCount = user.Posts != null ? user.Posts.Count : 0;

        return new UserProfile
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            Imie = profile?.FirstName ?? string.Empty,
            Nazwisko = profile?.LastName ?? string.Empty,
            AvatarUrl = profile?.AvatarUrl ?? string.Empty,
            Bio = profile?.Bio ?? string.Empty,
            DataUrodzenia = profile?.BirthDate ?? string.Empty,
            Lokalizacja = profile?.Location ?? string.Empty,
            Website = profile?.Website ?? string.Empty,
            DataUtworzenia = profile?.JoinedDate ?? string.Empty,
            FollowCount = followingCount,
            FollowersCount = followersCount,
            PostsCount = postsCount,
        };
    }

    private static string ToDisplayTime(DateTime dateTime)
    {
        return dateTime.ToLocalTime().ToString("g", CultureInfo.GetCultureInfo("pl-PL"));
    }

    private static IEnumerable<string> ExtractHashtags(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.StartsWith('#') && word.Length > 1)
            .Select(word => word.Trim().TrimStart('#'))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    // Faza 1: Dodatkowe metody
    public bool DeletePost(int postId)
    {
        using var db = CreateContext();
        var post = db.Posts.Find(postId);
        if (post == null) return false;

        db.Posts.Remove(post);
        db.SaveChanges();
        return true;
    }

    public List<Post> SearchPosts(string query)
    {
        using var db = CreateContext();
        var currentUserId = GetCurrentUserId();
        var posts = db.Posts
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .Where(p => p.Text.Contains(query) ||
                        p.Hashtags.Any(h => h.Tag.Contains(query)))
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, currentUserId)).ToList();
    }

    public List<SuggestedUser> SearchUsers(string query)
    {
        using var db = CreateContext();
        var currentUserId = GetCurrentUserId();
        var followingIds = db.Follows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FollowingId)
            .ToHashSet();

        // SQLite EF Core nie tłumaczy Contains(string, StringComparison) — używamy LIKE.
        var like = $"%{query}%";
        var users = db.Users
            .Include(u => u.Profile)
            .Where(u => EF.Functions.Like(u.Login, like) ||
                        (u.Profile != null && (EF.Functions.Like(u.Profile.FirstName, like) ||
                                               EF.Functions.Like(u.Profile.LastName, like))))
            .OrderBy(u => u.Profile != null ? u.Profile.FirstName : string.Empty)
            .Take(20)
            .ToList();

        return users.Select(u => new SuggestedUser
        {
            Id = u.Id,
            Imie = u.Profile?.FirstName ?? string.Empty,
            Nazwisko = u.Profile?.LastName ?? string.Empty,
            Login = u.Login,
            AvatarUrl = u.Profile?.AvatarUrl ?? string.Empty,
            IsFollowing = followingIds.Contains(u.Id),
        }).ToList();
    }

    public bool ChangePassword(int userId, string oldPassword, string newPassword)
    {
        using var db = CreateContext();
        var user = db.Users.Find(userId);
        if (user == null) return false;

        // Weryfikacja starego hasła za pomocą BCrypt
        if (!BCryptHelper.Verify(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCryptHelper.HashPassword(newPassword);
        db.SaveChanges();
        return true;
    }

    public bool DeleteAccount(int userId)
    {
        using var db = CreateContext();
        var user = db.Users.Find(userId);
        if (user == null) return false;

        // Ręcznie usuń wiersze z FK ustawionym na Restrict (Notifications.Actor, Messages.Sender),
        // żeby nie naruszyć constraints przy kaskadowym usuwaniu użytkownika.
        var notificationsAsActor = db.Notifications.Where(n => n.ActorId == userId);
        db.Notifications.RemoveRange(notificationsAsActor);

        var messagesFromUser = db.Messages.Where(m => m.SenderId == userId);
        db.Messages.RemoveRange(messagesFromUser);

        db.SaveChanges();

        db.Users.Remove(user);
        db.SaveChanges();
        return true;
    }

    /// <summary>Liczba nieprzeczytanych powiadomień użytkownika.</summary>
    public int GetUnreadNotificationCount(int userId)
    {
        if (userId <= 0) return 0;
        using var db = CreateContext();
        return db.Notifications.Count(n => n.RecipientId == userId && !n.IsRead);
    }

    /// <summary>Liczba konwersacji z nieprzeczytanymi wiadomościami.</summary>
    public int GetUnreadConversationCount(int userId)
    {
        if (userId <= 0) return 0;
        using var db = CreateContext();
        return db.Messages
            .Where(m => !m.IsRead && m.SenderId != userId &&
                        m.Conversation!.Participants.Any(p => p.UserId == userId))
            .Select(m => m.ConversationId)
            .Distinct()
            .Count();
    }

    /// <summary>Oznacz wszystkie powiadomienia użytkownika jako przeczytane.</summary>
    public void MarkAllNotificationsRead(int userId)
    {
        if (userId <= 0) return;
        using var db = CreateContext();
        var unread = db.Notifications.Where(n => n.RecipientId == userId && !n.IsRead).ToList();
        if (unread.Count == 0) return;
        foreach (var n in unread) n.IsRead = true;
        db.SaveChanges();
    }

    public int CreateConversation(int userId1, int userId2)
    {
        using var db = CreateContext();

        // Sprawdź czy konwersacja już istnieje
        var existing = db.Conversations
            .Include(c => c.Participants)
            .FirstOrDefault(c => c.Participants.Any(p => p.UserId == userId1) &&
                               c.Participants.Any(p => p.UserId == userId2));

        if (existing != null) return existing.Id;

        // Utwórz nową konwersację
        var conversation = new ConversationEntity
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastMessage = "",
        };
        db.Conversations.Add(conversation);
        db.SaveChanges();

        // Dodaj uczestników
        var participant1 = new ConversationParticipantEntity
        {
            ConversationId = conversation.Id,
            UserId = userId1,
        };
        var participant2 = new ConversationParticipantEntity
        {
            ConversationId = conversation.Id,
            UserId = userId2,
        };
        db.ConversationParticipants.Add(participant1);
        db.ConversationParticipants.Add(participant2);
        db.SaveChanges();

        return conversation.Id;
    }

    public bool RegisterUser(string login, string email, string password, string firstName, string lastName)
    {
        using var db = CreateContext();

        // Sprawdź czy login lub email już istnieje
        if (db.Users.Any(u => u.Login == login || u.Email == email))
            return false;

        var user = new UserEntity
        {
            Login = login,
            Email = email,
            PasswordHash = BCryptHelper.HashPassword(password),
        };
        db.Users.Add(user);
        db.SaveChanges();

        // Utwórz profil (PNG zamiast SVG — WinUI 3 BitmapImage nie wspiera SVG)
        var profile = new UserProfileEntity
        {
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            JoinedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            AvatarUrl = $"https://api.dicebear.com/7.x/avataaars/png?seed={login}",
        };
        db.UserProfiles.Add(profile);
        db.SaveChanges();

        // Utwórz domyślne ustawienia
        var settings = new UserSettingsEntity
        {
            UserId = user.Id,
            EmailNotifications = true,
            PushNotifications = true,
            PrivacyLevel = "Public",
            Theme = "Dark",
            Language = "pl-PL",
        };
        db.UserSettings.Add(settings);
        db.SaveChanges();

        return true;
    }

    public UserProfile? GetUserProfileByHandle(string handle)
    {
        using var db = CreateContext();
        var user = db.Users
            .Include(u => u.Profile)
            .Include(u => u.Following)
            .Include(u => u.Followers)
            .Include(u => u.Posts)
            .FirstOrDefault(u => u.Login == handle);

        if (user == null) return null;

        return MapUserProfile(user, user.Profile);
    }

    public List<Post> GetFeedPostsForYou(int userId)
    {
        using var db = CreateContext();
        var followingIds = db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToHashSet();

        var posts = db.Posts
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .Where(p => p.AuthorId == userId || followingIds.Contains(p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, userId)).ToList();
    }

    public List<Post> GetFeedPostsFollowing(int userId)
    {
        using var db = CreateContext();
        var followingIds = db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToHashSet();

        var posts = db.Posts
            .Include(p => p.Author)
                .ThenInclude(a => a!.Profile)
            .Include(p => p.Hashtags)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .Where(p => followingIds.Contains(p.AuthorId))
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return posts.Select(p => MapPostWithUserStatus(p, userId)).ToList();
    }

    public bool ToggleLikePost(int postId, int userId)
    {
        using var db = CreateContext();
        var post = db.Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null) return false;

        var existing = db.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

        if (existing != null)
        {
            db.Likes.Remove(existing);
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
            db.SaveChanges();
            return false;
        }

        db.Likes.Add(new LikeEntity
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        });

        post.LikesCount += 1;

        // Powiadomienie dla autora (jeśli to nie ten sam user)
        if (post.AuthorId != userId)
        {
            db.Notifications.Add(new NotificationEntity
            {
                RecipientId = post.AuthorId,
                ActorId = userId,
                Type = "Like",
                Preview = post.Text.Length > 80 ? post.Text.Substring(0, 80) + "…" : post.Text,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            });
        }

        db.SaveChanges();
        return true;
    }

    public List<Comment> GetComments(int postId)
    {
        using var db = CreateContext();
        var comments = db.Comments
            .Include(c => c.User)
                .ThenInclude(u => u!.Profile)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                    .ThenInclude(u => u!.Profile)
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();

        return comments.Select(c => MapComment(c)).ToList();
    }

    public Comment AddComment(int postId, int userId, string content)
    {
        using var db = CreateContext();
        var user = db.Users.Include(u => u.Profile).FirstOrDefault(u => u.Id == userId);
        var post = db.Posts.FirstOrDefault(p => p.Id == postId);
        if (user == null || post == null) return new Comment();

        var comment = new CommentEntity
        {
            PostId = postId,
            UserId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow,
        };

        db.Comments.Add(comment);
        post.CommentsCount += 1;

        if (post.AuthorId != userId)
        {
            db.Notifications.Add(new NotificationEntity
            {
                RecipientId = post.AuthorId,
                ActorId = userId,
                Type = "Reply",
                Preview = content.Length > 80 ? content.Substring(0, 80) + "…" : content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            });
        }

        db.SaveChanges();

        db.Entry(comment).Reference(c => c.User).CurrentValue = user;
        return MapComment(comment);
    }

    public bool DeleteComment(int commentId)
    {
        using var db = CreateContext();
        var comment = db.Comments.Include(c => c.Post).FirstOrDefault(c => c.Id == commentId);
        if (comment == null) return false;

        if (comment.Post != null)
        {
            comment.Post.CommentsCount = Math.Max(0, comment.Post.CommentsCount - 1);
        }

        db.Comments.Remove(comment);
        db.SaveChanges();
        return true;
    }

    public bool ToggleRetweet(int postId, int userId)
    {
        using var db = CreateContext();
        var post = db.Posts.FirstOrDefault(p => p.Id == postId);
        if (post == null) return false;

        var existing = db.Retweets.FirstOrDefault(r => r.OriginalPostId == postId && r.UserId == userId);

        if (existing != null)
        {
            db.Retweets.Remove(existing);
            post.RetweetsCount = Math.Max(0, post.RetweetsCount - 1);
            db.SaveChanges();
            return false;
        }

        db.Retweets.Add(new RetweetEntity
        {
            OriginalPostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
        });

        post.RetweetsCount += 1;

        if (post.AuthorId != userId)
        {
            db.Notifications.Add(new NotificationEntity
            {
                RecipientId = post.AuthorId,
                ActorId = userId,
                Type = "Repost",
                Preview = post.Text.Length > 80 ? post.Text.Substring(0, 80) + "…" : post.Text,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            });
        }

        db.SaveChanges();
        return true;
    }

    public string SaveUploadedImage(byte[] imageData)
    {
        var fileName = $"{Guid.NewGuid()}.jpg";
        var uploadsDir = Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(uploadsDir);
        
        var filePath = Path.Combine(uploadsDir, fileName);
        File.WriteAllBytes(filePath, imageData);
        
        return $"uploads/{fileName}";
    }

    public Post AddPostWithImage(int userId, string content, byte[]? imageData)
    {
        using var db = CreateContext();
        var user = db.Users.Include(u => u.Profile).First(u => u.Id == userId);
        
        string? mediaUrl = null;
        if (imageData != null && imageData.Length > 0)
        {
            mediaUrl = SaveUploadedImage(imageData);
        }

        var post = new PostEntity
        {
            AuthorId = userId,
            Text = content,
            MediaUrl = mediaUrl,
            CreatedAt = DateTime.UtcNow,
        };

        db.Posts.Add(post);
        db.SaveChanges();

        db.Entry(post).Reference(p => p.Author).CurrentValue = user;
        return MapPost(post);
    }

    private static Comment MapComment(CommentEntity comment)
    {
        return new Comment
        {
            Id = comment.Id,
            Author = MapAuthor(comment.User, comment.User?.Profile),
            Content = comment.Content,
            CreatedAt = ToDisplayTime(comment.CreatedAt),
            ParentCommentId = comment.ParentCommentId,
            Replies = comment.Replies?.Select(MapComment).ToList() ?? [],
        };
    }
}
