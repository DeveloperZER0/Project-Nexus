using System;

namespace Nexus.Data.Entities;

public class MessageEntity
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }

    public ConversationEntity? Conversation { get; set; }
    public UserEntity? Sender { get; set; }
}
