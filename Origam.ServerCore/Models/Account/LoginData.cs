using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Models.Account
{
    public class LoginData
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}