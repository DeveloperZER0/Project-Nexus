using System;

namespace Nexus.Data.Entities;

public class LikeEntity
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public PostEntity? Post { get; set; }
    public UserEntity? User { get; set; }
}
