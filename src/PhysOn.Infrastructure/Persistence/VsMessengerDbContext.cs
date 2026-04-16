using Microsoft.EntityFrameworkCore;
using PhysOn.Application.Abstractions;
using PhysOn.Domain.Accounts;
using PhysOn.Domain.Conversations;
using PhysOn.Domain.Invites;
using PhysOn.Domain.Messages;

namespace PhysOn.Infrastructure.Persistence;

public sealed class PhysOnDbContext : DbContext, IAppDbContext
{
    public PhysOnDbContext(DbContextOptions<PhysOnDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Invite> Invites => Set<Invite>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMember> ConversationMembers => Set<ConversationMember>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(40);
            entity.Property(x => x.StatusMessage).HasMaxLength(120);
            entity.Property(x => x.Locale).HasMaxLength(10);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.InstallId).HasMaxLength(100);
            entity.Property(x => x.Platform).HasMaxLength(16);
            entity.Property(x => x.DeviceName).HasMaxLength(80);
            entity.Property(x => x.AppVersion).HasMaxLength(32);
            entity.HasOne(x => x.Account)
                .WithMany(x => x.Devices)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RefreshTokenHash).HasMaxLength(128);
            entity.HasIndex(x => x.RefreshTokenHash).IsUnique();
            entity.HasOne(x => x.Account)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Device)
                .WithMany()
                .HasForeignKey(x => x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invite>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CodeHash).HasMaxLength(128);
            entity.HasIndex(x => x.CodeHash).IsUnique();
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(16);
        });

        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasKey(x => new { x.ConversationId, x.AccountId });
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(16);
            entity.HasOne(x => x.Conversation)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.AccountId, x.PinOrder });
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MessageType).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.BodyText).HasMaxLength(4000);
            entity.HasIndex(x => new { x.ConversationId, x.ClientRequestId }).IsUnique();
            entity.HasIndex(x => new { x.ConversationId, x.ServerSequence }).IsUnique();
            entity.HasOne(x => x.Conversation)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.SenderAccount)
                .WithMany()
                .HasForeignKey(x => x.SenderAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
