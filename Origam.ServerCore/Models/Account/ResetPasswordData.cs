using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Models.Account
{
    public class ResetPasswordData
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RePassword { get; set; }
    }
}