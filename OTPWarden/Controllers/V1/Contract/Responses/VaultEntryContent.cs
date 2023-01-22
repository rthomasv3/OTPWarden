using System;
using System.Collections.Generic;
using System.Linq;

namespace OTPWarden.Controllers.V1.Contract.Responses;

public sealed class VaultEntryContent
{
    public long Id { get; init; }

    public string Name { get; init; }

    public string SecretKey { get; init; }

    public string Issuer { get; init; }

    public string Algorithm { get; init; }

    public int? Digits { get; init; }

    public long? Counter { get; init; }

    public int? Period { get; init; }

    public DateTime? Created { get; init; }

    public string Notes { get; init; }

    public IEnumerable<string> Urls { get; init; } = Enumerable.Empty<string>();
}
