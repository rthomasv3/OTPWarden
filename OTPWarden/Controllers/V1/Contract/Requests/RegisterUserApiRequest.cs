namespace OTPWarden.Controllers.V1.Contract.Requests;

public sealed class RegisterUserApiRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
