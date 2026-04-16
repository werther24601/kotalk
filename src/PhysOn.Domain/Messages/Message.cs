namespace PhysOn.Domain.Messages;

public sealed class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public PhysOn.Domain.Conversations.Conversation? Conversation { get; set; }
    public Guid SenderAccountId { get; set; }
    public PhysOn.Domain.Accounts.Account? SenderAccount { get; set; }
    public Guid ClientRequestId { get; set; }
    public long ServerSequence { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string BodyText { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
