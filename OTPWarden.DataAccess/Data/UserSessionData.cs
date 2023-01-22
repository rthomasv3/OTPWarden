using System;

namespace OTPWarden.DataAccess.Data;

public partial record UserSessionData
{
    public long Id { get; init; }

    public long UserId { get; init; }

    public string UserDevice { get; init; }

    public string IpAddress { get; init; }

    public bool IsLoggedIn { get; init; }

    public string RefreshToken { get; init; }

    public DateTime RefreshTokenExpiration { get; init; }

    public bool IsRevoked { get; init; }

    public DateTime Created { get; init; }

    public DateTime? Updated { get; init; }

    public virtual UserData User { get; init; }
}
