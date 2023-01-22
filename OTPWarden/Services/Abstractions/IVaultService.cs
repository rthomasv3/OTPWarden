using System.Collections.Generic;
using System.Threading.Tasks;
using OTPWarden.Models;

namespace OTPWarden.Services.Abstractions;

public interface IVaultService
{
    Task<IEnumerable<VaultEntry>> GetVaultEntries(long userId);
    Task<VaultEntry> CreateVaultEntry(long userId, string name, string secretKey, IEnumerable<string> urls,
        string algorithm, long? counter, int? digits, string notes);
}
