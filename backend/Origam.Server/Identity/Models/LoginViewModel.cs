using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Origam.Server.Identity.Models;

public class LoginViewModel
{
    [Required]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
    public bool EnableLocalLogin { get; set; }
    public string ReturnUrl { get; set; }
    public List<ExternalProvider> VisibleExternalProviders { get; set; }
}
