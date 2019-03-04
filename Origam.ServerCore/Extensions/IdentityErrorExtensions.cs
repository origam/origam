using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Origam.ServerCore.Extensions
{
    public static class IdentityErrorExtensions
    {
        public static string ToErrorMessage(this IEnumerable<IdentityError> errors)
        {
            return errors
                .Select(error => error.Description)
                .Aggregate((allErrors, error) => allErrors += "\n" + error);
        }
    }
}