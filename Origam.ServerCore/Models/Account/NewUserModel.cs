using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Models.Account
{
    public class NewUserModel
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string RePassword { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string Name { get; set; }
     }
}