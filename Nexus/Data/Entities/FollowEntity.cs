using System;

namespace Nexus.Data.Entities;

public class FollowEntity
{
    public int FollowerId { get; set; }
    public int FollowingId { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserEntity? Follower { get; set; }
    public UserEntity? Following { get; set; }
}
