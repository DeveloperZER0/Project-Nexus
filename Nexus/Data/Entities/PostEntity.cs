using System;
using System.Collections.Generic;

namespace Nexus.Data.Entities;

public class PostEntity
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int RetweetsCount { get; set; }

    public UserEntity? Author { get; set; }
    public ICollection<PostHashtagEntity> Hashtags { get; set; } = new List<PostHashtagEntity>();
    public ICollection<BookmarkEntity> Bookmarks { get; set; } = new List<BookmarkEntity>();
    public ICollection<LikeEntity> Likes { get; set; } = new List<LikeEntity>();
    public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
    public ICollection<RetweetEntity> Retweets { get; set; } = new List<RetweetEntity>();
}
