using System.Collections.Generic;

namespace Origam.Security.Identity
{
    public class InternalIdentityResult
    {
        public bool Succeeded { get; }
        public IEnumerable<string> Errors { get; }

        public static InternalIdentityResult Success { get; } =
            new InternalIdentityResult(true);

        public static InternalIdentityResult Failed(params string[] errors)
        {
            return new InternalIdentityResult(errors);
        }
        
        public InternalIdentityResult(params string[] errors)
            : this((IEnumerable<string>) errors)
        {
        }

        public InternalIdentityResult(IEnumerable<string> errors)
        {
            Succeeded = false;
            Errors = errors ?? new []{"Unknown Error"};
        }

        private InternalIdentityResult(bool success)
        {
            Succeeded = success;
            Errors = new string[0];
        }
    }
}