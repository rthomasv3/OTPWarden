using System.Collections.Generic;
using System.Linq;

namespace OTPWarden.Controllers.V1.Contract.Requests;

public sealed class CreateVaultEntryApiRequest
{
    public string Name { get; set; }
    public string SecretKey { get; set; }

    public string Issuer { get; set; }

    public string Algorithm { get; set; }

    public int? Digits { get; set; }

    public long? Counter { get; set; }

    public int? Period { get; set; }

    public string Notes { get; set; }

    public IEnumerable<string> Urls { get; set; } = Enumerable.Empty<string>();
}
