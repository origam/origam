using System.ComponentModel.DataAnnotations;

namespace Origam.Server.IdentityServerGui.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "EmailRequired")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "UserNameRequired")]
    [DataType(DataType.Text)]
    public string UserName { get; set; }

    [Required(ErrorMessage = "NameRequired")]
    [StringLength(100)]
    [DataType(DataType.Text)]
    public string Name { get; set; }

    [Required(ErrorMessage = "FirstNameRequired")]
    [StringLength(100)]
    [DataType(DataType.Text)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "PasswordRequired")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "PasswordsDontMatch")]
    public string ConfirmPassword { get; set; }
    public string Code { get; set; }
}
