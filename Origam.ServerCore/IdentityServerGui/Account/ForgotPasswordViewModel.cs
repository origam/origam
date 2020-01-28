using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.IdentityServerGui.Account
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
