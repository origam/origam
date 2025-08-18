using System.ComponentModel.DataAnnotations;

namespace Origam.Server.IdentityServerGui.Account;
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "EmailRequired")]
    [EmailAddress(ErrorMessage = "EmailInvalid")]
    public string Email { get; set; }
    
    public string ReturnUrl { get; set; }
}
