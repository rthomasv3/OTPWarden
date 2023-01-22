using System.Collections.Generic;
using System.Linq;

namespace OTPWarden.Controllers.V1.Contract.Responses;

public sealed class GetUserVaultApiResponse : ApiResponse
{
    public long UserId { get; set; }
    public IEnumerable<VaultEntryContent> VaultEntries { get; set; } = Enumerable.Empty<VaultEntryContent>();
}
