using System;

namespace Nexus.Data.Entities;

public class NotificationEntity
{
    public int Id { get; set; }
    public int RecipientId { get; set; }
    public int ActorId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Preview { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }

    public UserEntity? Recipient { get; set; }
    public UserEntity? Actor { get; set; }
}
