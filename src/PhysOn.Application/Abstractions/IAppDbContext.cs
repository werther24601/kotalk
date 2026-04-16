using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PhysOn.Domain.Accounts;
using PhysOn.Domain.Conversations;
using PhysOn.Domain.Invites;
using PhysOn.Domain.Messages;

namespace PhysOn.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Device> Devices { get; }
    DbSet<Session> Sessions { get; }
    DbSet<Invite> Invites { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMember> ConversationMembers { get; }
    DbSet<Message> Messages { get; }
    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
