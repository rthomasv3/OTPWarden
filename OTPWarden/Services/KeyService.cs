using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using OTPWarden.Services.Abstractions;

namespace OTPWarden.Services;

public sealed class KeyService : IKeyService
{
    #region Fields

    private readonly ConcurrentDictionary<long, string> _userKeys = new ConcurrentDictionary<long, string>();

    #endregion

    #region Public Methods

    public void AddUserKey(long userId, string password)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] userDataHash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{userId}:{password}"));
        _userKeys[userId] = Convert.ToBase64String(userDataHash);
    }

    public string GetUserKey(long userId)
    {
        _userKeys.TryGetValue(userId, out string key);
        return key;
    }

    public void RemoveUserKey(long userId)
    {
        _userKeys.TryRemove(userId, out _);
    }

    #endregion
}
