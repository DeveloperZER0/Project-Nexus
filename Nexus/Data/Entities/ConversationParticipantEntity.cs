namespace Nexus.Data.Entities;

public class ConversationParticipantEntity
{
    public int ConversationId { get; set; }
    public int UserId { get; set; }

    public ConversationEntity? Conversation { get; set; }
    public UserEntity? User { get; set; }
}
