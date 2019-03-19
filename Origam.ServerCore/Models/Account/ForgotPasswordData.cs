using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Models.Account
{
    public class ForgotPasswordData
    {
        [Required]
        public string Email { get; set; }
    }
}