using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess;

public sealed class VaultEntryRepository : IVaultEntryRepository
{
    #region Fields

    private readonly OTPDbContext _context;

    #endregion

    #region Constructor

    public VaultEntryRepository(OTPDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion

    #region Properties

    public Expression<Func<VaultEntryData, VaultEntryData>> DefaultVaultEntryDataMapper
    {
        get
        {
            return x => new VaultEntryData()
            {
                Algorithm = x.Algorithm,
                Counter = x.Counter,
                Created = x.Created,
                Digits = x.Digits,
                Id = x.Id,
                Issuer = x.Issuer,
                Name = x.Name,
                Notes = x.Notes,
                Period = x.Period,
                SecretKey = x.SecretKey,
                User = x.User,
                UserId = x.UserId,
                VaultEntryUrls = x.VaultEntryUrls
            };
        }
    }

    #endregion

    #region Public Methods

    public async Task<VaultEntryData> CreateVaultEntry(long userId, string name, string encryptedSecretKey,
        IEnumerable<string> encryptedUrls, string algorithm, long? counter, int? digits,
        string encryptedNotes)
    {
        VaultEntryData vaultEntryData = new VaultEntryData()
        {
            Algorithm = algorithm,
            Counter = counter,
            Digits = digits,
            Notes = encryptedNotes,
            SecretKey = encryptedSecretKey,
            UserId = userId,
        };
        _context.VaultEntries.Add(vaultEntryData);

        if (encryptedUrls != null)
        {
            foreach (string url in encryptedUrls)
            {
                vaultEntryData.VaultEntryUrls.Add(new VaultEntryUrlData()
                {
                    Url = url,
                    VaultEntry = vaultEntryData
                });
            }
        }

        await _context.SaveChangesAsync();

        return vaultEntryData;
    }

    public async Task<IEnumerable<VaultEntryData>> GetUserVaultEntries(long userId,
        Expression<Func<VaultEntryData, VaultEntryData>> mapper = null)
    {
        return await _context.VaultEntries
            .Where(x => x.UserId == userId)
            .Select(mapper ?? DefaultVaultEntryDataMapper)
            .ToListAsync();
    }

    #endregion
}
