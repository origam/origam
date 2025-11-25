#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.ComponentModel.DataAnnotations;

namespace Origam.Server.Identity.Models;

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
