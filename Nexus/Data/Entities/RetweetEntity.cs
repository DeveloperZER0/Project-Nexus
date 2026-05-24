using System;

namespace Nexus.Data.Entities;

public class RetweetEntity
{
    public int Id { get; set; }
    public int OriginalPostId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public PostEntity? OriginalPost { get; set; }
    public UserEntity? User { get; set; }
}
