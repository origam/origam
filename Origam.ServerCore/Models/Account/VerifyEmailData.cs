using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Models.Account
{
    public class VerifyEmailData
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Token { get; set; }
    }
}