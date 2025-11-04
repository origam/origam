// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Origam.Server.IdentityServerGui.Account;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "EmailRequired")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "PasswordRequired")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "PasswordsDontMatch")]
    public string ConfirmPassword { get; set; }
    public string Code { get; set; }
    public string ReturnUrl { get; set; }
}
