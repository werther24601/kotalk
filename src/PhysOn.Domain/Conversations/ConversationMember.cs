using PhysOn.Domain.Accounts;

namespace PhysOn.Domain.Conversations;

public sealed class ConversationMember
{
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }
    public ConversationRole Role { get; set; } = ConversationRole.Member;
    public DateTimeOffset JoinedAt { get; set; }
    public long LastReadSequence { get; set; }
    public bool IsMuted { get; set; }
    public int? PinOrder { get; set; }
}
