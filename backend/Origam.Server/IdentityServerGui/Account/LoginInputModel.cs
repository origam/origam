// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Origam.Server.IdentityServerGui.Account;

public class LoginInputModel
{
    [Required(ErrorMessage = "UserNameRequired")]
    public string Username { get; set; }

    [Required(ErrorMessage = "PasswordRequired")]
    public string Password { get; set; }
    public string ReturnUrl { get; set; }
}
