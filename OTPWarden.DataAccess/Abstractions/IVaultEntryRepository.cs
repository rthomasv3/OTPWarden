using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OTPWarden.DataAccess.Data;

namespace OTPWarden.DataAccess.Abstractions;

public interface IVaultEntryRepository
{
    Expression<Func<VaultEntryData, VaultEntryData>> DefaultVaultEntryDataMapper { get; }

    Task<VaultEntryData> CreateVaultEntry(long userId, string name, string encryptedSecretKey,
        IEnumerable<string> encryptedUrls, string algorithm, long? counter, int? digits,
        string encryptedNotes);

    Task<IEnumerable<VaultEntryData>> GetUserVaultEntries(long userId, Expression<Func<VaultEntryData, VaultEntryData>> mapper = null);
}
