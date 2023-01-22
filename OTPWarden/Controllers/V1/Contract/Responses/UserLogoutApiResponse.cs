namespace OTPWarden.Controllers.V1.Contract.Responses;

public sealed class UserLogoutApiResponse : ApiResponse
{
    public bool LoggedOutSuccessfully { get; set; }
}
