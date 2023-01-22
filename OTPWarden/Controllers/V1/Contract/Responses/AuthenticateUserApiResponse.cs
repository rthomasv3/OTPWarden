using System.Text.Json.Serialization;

namespace OTPWarden.Controllers.V1.Contract.Responses;

public sealed class AuthenticateUserApiResponse : ApiResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
}
