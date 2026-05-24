using System;
using System.Collections.Generic;

namespace Nexus.Data.Entities;

public class ConversationEntity
{
    public int Id { get; set; }
    public string? LastMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ConversationParticipantEntity> Participants { get; set; } = new List<ConversationParticipantEntity>();
    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
}
