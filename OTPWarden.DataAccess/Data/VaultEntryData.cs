using System;
using System.Collections.Generic;

namespace OTPWarden.DataAccess.Data;

public partial record VaultEntryData
{
    public long Id { get; init; }

    public long UserId { get; init; }

    public string Name { get; init; }

    public string SecretKey { get; init; }

    public string Issuer { get; init; }

    public string Algorithm { get; init; }

    public int? Digits { get; init; }

    public long? Counter { get; init; }

    public int? Period { get; init; }

    public DateTime? Created { get; init; }

    public string Notes { get; init; }

    public virtual UserData User { get; init; }

    public virtual ICollection<VaultEntryUrlData> VaultEntryUrls { get; init; } = new List<VaultEntryUrlData>();
}
