using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.IdentityServerGui.Account
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "StringTooShort", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string UserName { get; set; }   
        
        [Required]
        [StringLength(100)]
        [DataType(DataType.Text)]
        public string Name { get; set; } 
        
        [Required]
        [StringLength(100)]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(100, ErrorMessage = "StringTooShort", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "PasswordsDontMatch")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}