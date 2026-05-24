using System.Collections.Generic;
using Nexus.Models;

namespace Nexus.Helpers;

/// <summary>
/// Dane mockowe przeniesione z design/src/app/data/mockData.ts.
/// Używane do wyświetlania UI w trybie demo (bez bazy danych).
/// </summary>
public static class MockData
{
    public static readonly UserProfile CurrentUser = new()
    {
        Id = 1,
        Login = "marekdev",
        Email = "marek@nexus.pl",
        Imie = "Marek",
        Nazwisko = "Kowalski",
        AvatarUrl = "https://images.unsplash.com/photo-1543132220-e7fef0b974e7?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
        Bio = "Full-stack developer 🧑‍💻 | C# & .NET enthusiast | Open source contributor",
        DataUrodzenia = "1998-03-12",
        Lokalizacja = "Warszawa, Polska",
        Website = "marekdev.pl",
        DataUtworzenia = "2024-01-15",
        FollowCount = 1247,
        FollowersCount = 3892,
        PostsCount = 187,
    };

    private static readonly PostAuthor AnnaAuthor = new()
    {
        Imie = "Anna", Nazwisko = "Nowak", Login = "anna_design",
        AvatarUrl = "https://images.unsplash.com/photo-1762522921456-cdfe882d36c3?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    };

    private static readonly PostAuthor TomekAuthor = new()
    {
        Imie = "Tomek", Nazwisko = "Zieliński", Login = "tomek_codes",
        AvatarUrl = "https://images.unsplash.com/photo-1720166067122-b5036f549ff9?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    };

    private static readonly PostAuthor KarolinaAuthor = new()
    {
        Imie = "Karolina", Nazwisko = "Wiśniewska", Login = "karolina_dev",
        AvatarUrl = "https://images.unsplash.com/photo-1569078449082-d264d9e239c5?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=400",
    };

    private static readonly PostAuthor MarekAuthor = new()
    {
        Imie = "Marek", Nazwisko = "Kowalski", Login = "marekdev",
        AvatarUrl = CurrentUser.AvatarUrl,
    };

    public static List<Post> FeedPosts =>
    [
        new()
        {
            Id = "1", Author = AnnaAuthor,
            Tekst = "Właśnie skończyłam nowy projekt w Blazor! 🔥 Połączenie C# z WebAssembly to przyszłość frontendu. Kto jeszcze eksperymentuje z .NET na frontendzie?",
            Hashtags = ["Blazor", "DotNet", "WebAssembly"],
            DataUtworzenia = "2h", LikesCount = 142, CommentsCount = 28, RetweetsCount = 34,
        },
        new()
        {
            Id = "2", Author = TomekAuthor,
            Tekst = "Zachód słońca po 12 godzinach kodowania. Czasem trzeba oderwać się od ekranu. 🌅",
            MediaUrl = "https://images.unsplash.com/photo-1732808460864-b8e5eb489a52?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=800",
            DataUtworzenia = "4h", LikesCount = 312, CommentsCount = 45, RetweetsCount = 67, Liked = true,
        },
        new()
        {
            Id = "3", Author = KarolinaAuthor,
            Tekst = "Tips na dziś:\n\n1. Używaj record types w C# 12\n2. Pattern matching > if/else chains\n3. Minimal APIs > Controllers (dla prostych projektów)\n4. Source generators zamiast reflection\n\nZapisz na później! 📌",
            Hashtags = ["CSharp", "DotNet9"],
            DataUtworzenia = "6h", LikesCount = 567, CommentsCount = 89, RetweetsCount = 201,
        },
        new()
        {
            Id = "4", Author = MarekAuthor,
            Tekst = "Moje biurko o 23:00. Deadline nie czeka na nikogo 😅☕",
            MediaUrl = "https://images.unsplash.com/photo-1759668358660-0d06064f0f84?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=800",
            DataUtworzenia = "8h", LikesCount = 89, CommentsCount = 12, RetweetsCount = 5,
        },
    ];

    public static List<Post> UserPosts =>
    [
        new()
        {
            Id = "p1", Author = MarekAuthor,
            Tekst = "Moje biurko o 23:00. Deadline nie czeka na nikogo 😅☕",
            MediaUrl = "https://images.unsplash.com/photo-1759668358660-0d06064f0f84?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=800",
            DataUtworzenia = "8h", LikesCount = 89, CommentsCount = 12, RetweetsCount = 5,
        },
        new()
        {
            Id = "p2", Author = MarekAuthor,
            Tekst = "Nowy artykuł na blogu: \"Dlaczego C# jest najlepszym językiem w 2026 roku\" 🚀\n\nLink w bio!",
            Hashtags = ["CSharp", "Blog"],
            DataUtworzenia = "1d", LikesCount = 234, CommentsCount = 56, RetweetsCount = 78,
        },
    ];

    public static List<Post> BookmarkedPosts =>
    [
        new()
        {
            Id = "3", Author = KarolinaAuthor,
            Tekst = "Tips na dziś:\n\n1. Używaj record types w C# 12\n2. Pattern matching > if/else chains\n3. Minimal APIs > Controllers\n4. Source generators zamiast reflection\n\nZapisz na później! 📌",
            Hashtags = ["CSharp", "DotNet9"],
            DataUtworzenia = "6h", LikesCount = 567, CommentsCount = 89, RetweetsCount = 201, Bookmarked = true,
        },
        new()
        {
            Id = "1", Author = AnnaAuthor,
            Tekst = "Właśnie skończyłam nowy projekt w Blazor! 🔥 Połączenie C# z WebAssembly to przyszłość frontendu.",
            Hashtags = ["Blazor", "DotNet", "WebAssembly"],
            DataUtworzenia = "2h", LikesCount = 142, CommentsCount = 28, RetweetsCount = 34, Bookmarked = true,
        },
    ];

    public static List<Notification> Notifications =>
    [
        new() { Id = "1", Type = "Like", User = AnnaAuthor, Preview = "\"Właśnie skończyłam nowy projekt w Blazor!\"", Time = "5 min", IsRead = false },
        new() { Id = "2", Type = "Follow", User = KarolinaAuthor, Time = "15 min", IsRead = false },
        new() { Id = "3", Type = "Repost", User = TomekAuthor, Preview = "\"Tips na dziś: Używaj record types w C# 12...\"", Time = "1h", IsRead = false },
        new() { Id = "4", Type = "Reply", User = AnnaAuthor, Preview = "\"Zgadzam się! Blazor naprawdę zmienia podejście...\"", Time = "2h", IsRead = true },
        new() { Id = "5", Type = "Mention", User = TomekAuthor, Preview = "\"@marekdev co myślisz o nowym Entity Framework?\"", Time = "3h", IsRead = true },
    ];

    public static List<Conversation> Conversations =>
    [
        new() { Id = "1", Imie = "Anna", Nazwisko = "Nowak", Login = "anna_design", AvatarUrl = AnnaAuthor.AvatarUrl, LastMessage = "Jasne, prześlę Ci link do repo!", Time = "14:30", IsRead = false },
        new() { Id = "2", Imie = "Tomek", Nazwisko = "Zieliński", Login = "tomek_codes", AvatarUrl = TomekAuthor.AvatarUrl, LastMessage = "Dzięki za feedback! 🙏", Time = "12:15", IsRead = true },
        new() { Id = "3", Imie = "Karolina", Nazwisko = "Wiśniewska", Login = "karolina_dev", AvatarUrl = KarolinaAuthor.AvatarUrl, LastMessage = "Spotkajmy się na konferencji .NET!", Time = "wczoraj", IsRead = true },
    ];

    public static List<ChatMessage> ChatMessages =>
    [
        new() { Id = "1", SenderId = 2, Tekst = "Hej! Widziałam Twój post o Blazorze", Time = "14:20", IsRead = true },
        new() { Id = "2", SenderId = 1, Tekst = "Cześć Anna! Tak, świetny framework", Time = "14:22", IsRead = true },
        new() { Id = "3", SenderId = 2, Tekst = "Masz może jakieś repo z przykładami?", Time = "14:25", IsRead = true },
        new() { Id = "4", SenderId = 1, Tekst = "Tak! Mam kilka projektów na GitHubie", Time = "14:27", IsRead = true },
        new() { Id = "5", SenderId = 2, Tekst = "Jasne, prześlę Ci link do repo!", Time = "14:30", IsRead = false },
    ];

    public static List<TrendingHashtag> TrendingHashtags =>
    [
        new() { Id = 1, Name = "CSharp", PostsCount = "12.4K", Category = "Technologia", Description = "Dyskusje o najnowszych funkcjach C# 12" },
        new() { Id = 2, Name = "DotNet9", PostsCount = "8.7K", Category = "Programowanie", Description = "Premiera .NET 9 - co nowego?" },
        new() { Id = 3, Name = "Blazor", PostsCount = "4.1K", Category = "Programowanie", Description = "WebAssembly z C# w akcji" },
        new() { Id = 4, Name = "GameDev", PostsCount = "5.2K", Category = "Gaming", Description = "Tworzenie gier w Unity i Godot" },
        new() { Id = 5, Name = "AI", PostsCount = "34.1K", Category = "Technologia", Description = "Sztuczna inteligencja zmienia świat" },
        new() { Id = 6, Name = "OpenSource", PostsCount = "3.8K", Category = "Społeczność", Description = "Projekty open source warte uwagi" },
        new() { Id = 7, Name = "WebDev", PostsCount = "15.3K", Category = "Programowanie", Description = "Frontend, backend i wszystko pomiędzy" },
        new() { Id = 8, Name = "Startup", PostsCount = "6.9K", Category = "Biznes", Description = "Historie polskich startupów" },
    ];

    public static List<SuggestedUser> SuggestedUsers =>
    [
        new() { Id = 2, Imie = "Anna", Nazwisko = "Nowak", Login = "anna_design", AvatarUrl = AnnaAuthor.AvatarUrl },
        new() { Id = 3, Imie = "Karolina", Nazwisko = "Wiśniewska", Login = "karolina_dev", AvatarUrl = KarolinaAuthor.AvatarUrl },
        new() { Id = 4, Imie = "Tomek", Nazwisko = "Zieliński", Login = "tomek_codes", AvatarUrl = TomekAuthor.AvatarUrl },
    ];

    public static List<UserSession> Sessions =>
    [
        new() { Id = 1, Device = "Windows 11 · Chrome", IconGlyph = "\uE977", Location = "Warszawa, PL", Expiry = "2026-06-04", Current = true },
        new() { Id = 2, Device = "iPhone 15 · Safari", IconGlyph = "\uE8EA", Location = "Kraków, PL", Expiry = "2026-05-20", Current = false },
    ];
}
