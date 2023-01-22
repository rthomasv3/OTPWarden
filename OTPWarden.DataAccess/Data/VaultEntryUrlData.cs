namespace OTPWarden.DataAccess.Data;

public partial record VaultEntryUrlData
{
    public long Id { get; init; }

    public long VaultEntryId { get; init; }

    public string Url { get; init; }

    public virtual VaultEntryData VaultEntry { get; init; }
}
