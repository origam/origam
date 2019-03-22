#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Origam.Security.Common;

namespace Origam.Security.Identity
{
    // based on Microsoft.AspNet.Identity.PasswordValidator
    public class OrigamPasswordValidator : IIdentityValidator<string>
    {
        private int minimumLength;

        private int numberOfRequiredNonAlphanumericChars;

        public OrigamPasswordValidator(
            int minimumLength, int numberOfRequiredNonAlphanumericChars)
        {
            this.minimumLength = minimumLength;
            this.numberOfRequiredNonAlphanumericChars 
                = numberOfRequiredNonAlphanumericChars;
        }


        public Task<IdentityResult> ValidateAsync(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(item) || (item.Length < minimumLength))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, 
                    Resources.PasswordTooShort, minimumLength));
            }
            int nonAlphanumericCharsCount 
                = item.Count(c => !IsLetterOrDigit(c));
            if (nonAlphanumericCharsCount
            < numberOfRequiredNonAlphanumericChars)
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture,
                    Resources.PasswordNotEnoughNonAlphanumericChars,
                    numberOfRequiredNonAlphanumericChars));
            }
            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            return Task.FromResult(
                IdentityResult.Failed(string.Join(" ", errors)));
        }

        /// <summary>
        ///     Returns true if the character is a digit between '0' and '9'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        ///     Returns true if the character is between 'a' and 'z'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsLower(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        /// <summary>
        ///     Returns true if the character is between 'A' and 'Z'
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsUpper(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        /// <summary>
        ///     Returns true if the character is upper, lower, or a digit
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public virtual bool IsLetterOrDigit(char c)
        {
            return IsUpper(c) || IsLower(c) || IsDigit(c);
        }
    }
}
