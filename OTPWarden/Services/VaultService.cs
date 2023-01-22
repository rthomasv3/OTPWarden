using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OTPWarden.DataAccess.Abstractions;
using OTPWarden.DataAccess.Data;
using OTPWarden.Models;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Services;

public sealed class VaultService : IVaultService
{
    #region Fields

    private readonly IVaultEntryRepository _vaultEntryRepository;
    private readonly ICryptographyService _cryptographyService;
    private readonly IKeyService _keyService;

    #endregion

    #region Constructor

    public VaultService(IVaultEntryRepository vaultEntryRepository, ICryptographyService cryptographyService, IKeyService keyService)
    {
        _vaultEntryRepository = vaultEntryRepository ?? throw new ArgumentNullException(nameof(vaultEntryRepository));
        _cryptographyService = cryptographyService ?? throw new ArgumentNullException(nameof(cryptographyService));
        _keyService = keyService ?? throw new ArgumentNullException(nameof(keyService));
    }

    #endregion

    #region Public Methods

    public async Task<IEnumerable<VaultEntry>> GetVaultEntries(long userId)
    {
        List<VaultEntry> vaultEntries = new List<VaultEntry>();
        IEnumerable<VaultEntryData> userVaultEntries = await _vaultEntryRepository.GetUserVaultEntries(userId);

        foreach (VaultEntryData vaultEntryData in userVaultEntries)
        {
            vaultEntries.Add(DecryptVaultEntry(vaultEntryData));
        }

        return vaultEntries;
    }

    public async Task<VaultEntry> CreateVaultEntry(long userId, string name, string secretKey, IEnumerable<string> urls,
        string algorithm, long? counter, int? digits, string notes)
    {
        string userEncryptionKey = _keyService.GetUserKey(userId);

        string encryptedNotes = null;
        List<string> encryptedUrls = new List<string>();
        string encryptedSecretKey = _cryptographyService.Encrypt(secretKey, userEncryptionKey);

        if (urls != null)
        {
            foreach (string url in urls)
            {
                string encryptedUrl = _cryptographyService.Encrypt(url, userEncryptionKey);

                if (encryptedUrl != null)
                {
                    encryptedUrls.Add(encryptedUrl);
                }
            }
        }

        if (notes != null)
        {
            encryptedNotes = _cryptographyService.Encrypt(notes, userEncryptionKey);
        }

        VaultEntryData vaultEntryData = await _vaultEntryRepository.CreateVaultEntry(userId, name, encryptedSecretKey,
            encryptedUrls, algorithm, counter, digits, encryptedNotes);

        return DecryptVaultEntry(vaultEntryData);
    }

    #endregion

    #region Private Methods

    private VaultEntry DecryptVaultEntry(VaultEntryData vaultEntryData)
    {
        string userDataDecryptionKey = _keyService.GetUserKey(vaultEntryData.UserId);

        string notes = !String.IsNullOrEmpty(vaultEntryData.Notes) ?
            _cryptographyService.Decrypt(vaultEntryData.Notes, userDataDecryptionKey) : String.Empty;
        string secretKey = !String.IsNullOrEmpty(vaultEntryData.SecretKey) ?
            _cryptographyService.Decrypt(vaultEntryData.SecretKey, userDataDecryptionKey) : String.Empty;

        List<string> urls = new List<string>();

        foreach (VaultEntryUrlData vaultEntryUrlData in vaultEntryData.VaultEntryUrls)
        {
            if (!String.IsNullOrEmpty(vaultEntryUrlData.Url))
            {
                urls.Add(_cryptographyService.Decrypt(vaultEntryUrlData.Url, userDataDecryptionKey));
            }
        }

        return new VaultEntry()
        {
            Algorithm = vaultEntryData.Algorithm,
            Counter = vaultEntryData.Counter,
            Created = vaultEntryData.Created,
            Digits = vaultEntryData.Digits,
            Id = vaultEntryData.Id,
            Issuer = vaultEntryData.Issuer,
            Name = vaultEntryData.Name,
            Notes = notes,
            Period = vaultEntryData.Period,
            SecretKey = secretKey,
            Urls = urls
        };
    }

    #endregion
}
