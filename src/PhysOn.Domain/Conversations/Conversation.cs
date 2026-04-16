namespace PhysOn.Domain.Conversations;

public sealed class Conversation
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public Guid? CreatedByAccountId { get; set; }
    public long LastMessageSequence { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public List<ConversationMember> Members { get; } = [];
    public List<PhysOn.Domain.Messages.Message> Messages { get; } = [];
}
