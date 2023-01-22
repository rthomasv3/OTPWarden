using Microsoft.EntityFrameworkCore;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess;

public partial class SqliteDbContext : OTPDbContext
{
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.HasIndex(e => e.Email).HasDatabaseName("IX_User_Email");

            entity.ToTable("User", "otp");

            entity.HasIndex(e => e.Email, "User_Email").IsUnique();

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Created).HasPrecision(2);
            entity.Property(e => e.Email).HasMaxLength(1024);
            entity.Property(e => e.LastLogin).HasPrecision(2);
            entity.Property(e => e.Password).HasMaxLength(1024);
            entity.Property(e => e.Username).HasMaxLength(256);
        });

        modelBuilder.Entity<UserSessionData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserSession_pkey");

            entity.HasIndex(e => e.IsLoggedIn).HasDatabaseName("IX_UserSession_IsLoggedIn");
            entity.HasIndex(e => e.RefreshToken).HasDatabaseName("IX_UserSession_RefreshToken");
            entity.HasIndex(e => e.RefreshTokenExpiration).HasDatabaseName("IX_UserSession_RefreshTokenExpiration");

            entity.ToTable("UserSession", "otp");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.UserDevice).HasMaxLength(2048);
            entity.Property(e => e.IpAddress).HasMaxLength(512);
            entity.Property(e => e.RefreshToken).HasMaxLength(1024);
            entity.Property(e => e.IsLoggedIn).HasDefaultValue(false);
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.Created).HasPrecision(2);
            entity.Property(e => e.RefreshTokenExpiration).HasPrecision(2);
            entity.Property(e => e.Updated).HasPrecision(2);

            entity.HasOne(d => d.User).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSession_User");
        });

        modelBuilder.Entity<VaultEntryData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("VaultEntry_pkey");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_VaultEntry_UserId");

            entity.ToTable("VaultEntry", "otp");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Algorithm).HasMaxLength(512);
            entity.Property(e => e.Name).HasMaxLength(512);
            entity.Property(e => e.Created).HasPrecision(2);
            entity.Property(e => e.Issuer).HasMaxLength(1024);
            entity.Property(e => e.Notes).HasMaxLength(2056);
            entity.Property(e => e.SecretKey).HasMaxLength(2056);

            entity.HasOne(d => d.User).WithMany(p => p.VaultEntries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VaultEntry_User");
        });

        modelBuilder.Entity<VaultEntryUrlData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("VaultEntryUrl_pkey");

            entity.ToTable("VaultEntryUrl", "otp");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Url).HasMaxLength(2056);

            entity.HasOne(d => d.VaultEntry).WithMany(p => p.VaultEntryUrls)
                .HasForeignKey(d => d.VaultEntryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VaultEntryUrl_VaultEntry");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

