using System.Linq;
using Microsoft.AspNet.Identity;

namespace Origam.Security.Identity
{
    public static class IdentityResultExtensions
    {
        public static InternalIdentityResult ToInternalIdentityResult(this IdentityResult result)
        {
            return result.Succeeded 
                ? InternalIdentityResult.Success 
                : InternalIdentityResult.Failed(result.Errors.ToArray());
        }
    }
}