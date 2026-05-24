using System;
using System.Collections.Generic;

namespace Nexus.Data.Entities;

public class CommentEntity
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? ParentCommentId { get; set; }

    public PostEntity? Post { get; set; }
    public UserEntity? User { get; set; }
    public CommentEntity? ParentComment { get; set; }
    public ICollection<CommentEntity> Replies { get; set; } = new List<CommentEntity>();
}
