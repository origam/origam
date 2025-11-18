namespace Origam.Server.Identity.Models;

public class ResetPasswordViewModel
{
    public string ReturnUrl { get; set; }
    public string ConfirmPassword { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public string Password { get; set; }
}
