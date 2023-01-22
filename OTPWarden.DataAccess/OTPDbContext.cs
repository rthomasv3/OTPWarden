using Microsoft.EntityFrameworkCore;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess;

public partial class OTPDbContext : DbContext
{
    public OTPDbContext(DbContextOptions options)
        : base(options)
    {
        if (ChangeTracker != null)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }

    public virtual DbSet<UserData> Users { get; set; }

    public virtual DbSet<UserSessionData> UserSessions { get; set; }

    public virtual DbSet<VaultEntryData> VaultEntries { get; set; }

    public virtual DbSet<VaultEntryUrlData> VaultEntryUrls { get; set; }
}
