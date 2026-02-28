using GameLibrary.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameLibrary.Api.Data;

public sealed class GameLibraryDbContext(DbContextOptions<GameLibraryDbContext> options) : DbContext(options)
{
    public DbSet<PlatformAccountEntity> PlatformAccounts => Set<PlatformAccountEntity>();
    public DbSet<OwnedGameEntity> OwnedGames => Set<OwnedGameEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlatformAccountEntity>(entity =>
        {
            entity.ToTable("platform_accounts");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.Platform)
                .HasColumnName("platform")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.AccountName)
                .HasColumnName("account_name")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.ExternalAccountId)
                .HasColumnName("external_account_id")
                .HasMaxLength(128);

            entity.Property(x => x.CredentialType)
                .HasColumnName("credential_type")
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.CredentialValue)
                .HasColumnName("credential_value")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasColumnType("datetime(6)");

            entity.Property(x => x.UpdatedAtUtc)
                .HasColumnName("updated_at")
                .HasColumnType("datetime(6)");

            entity.Property(x => x.LastSyncedAtUtc)
                .HasColumnName("last_synced_at")
                .HasColumnType("datetime(6)");

            entity.HasIndex(x => new { x.Platform, x.AccountName })
                .HasDatabaseName("uk_platform_account_name")
                .IsUnique();

            entity.HasIndex(x => new { x.Platform, x.ExternalAccountId })
                .HasDatabaseName("idx_platform_external_account");
        });

        modelBuilder.Entity<OwnedGameEntity>(entity =>
        {
            entity.ToTable("owned_games");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.AccountId)
                .HasColumnName("account_id");

            entity.Property(x => x.Platform)
                .HasColumnName("platform")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.AccountName)
                .HasColumnName("account_name")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.ExternalGameId)
                .HasColumnName("external_game_id")
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(x => x.NormalizedTitle)
                .HasColumnName("normalized_title")
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(x => x.SyncedAtUtc)
                .HasColumnName("synced_at")
                .HasColumnType("datetime(6)");

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at")
                .HasColumnType("datetime(6)");

            entity.HasIndex(x => new { x.AccountId, x.ExternalGameId })
                .HasDatabaseName("uk_account_external_game")
                .IsUnique();

            entity.HasIndex(x => x.NormalizedTitle)
                .HasDatabaseName("idx_normalized_title");

            entity.HasIndex(x => new { x.Platform, x.AccountName })
                .HasDatabaseName("idx_platform_account");

            entity.HasOne(x => x.Account)
                .WithMany(x => x.OwnedGames)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_owned_games_account");
        });
    }
}
