using System.ComponentModel.DataAnnotations;

namespace Origam.Server.IdentityServerGui.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
