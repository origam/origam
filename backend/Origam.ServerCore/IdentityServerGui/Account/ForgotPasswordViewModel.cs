using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.IdentityServerGui.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress(ErrorMessage = "EmailInvalid")]
        public string Email { get; set; }
    }
}
