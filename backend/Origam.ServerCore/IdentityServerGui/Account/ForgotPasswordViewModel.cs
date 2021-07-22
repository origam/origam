using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.IdentityServerGui.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "EmailRequired")]
        [EmailAddress]
        public string Email { get; set; }
    }
}
