namespace OTPWarden.Controllers.V1.Contract.Responses;

public sealed class CreateVaultEntryApiResponse : ApiResponse
{
    public VaultEntryContent VaultEntry { get; init; }
}
