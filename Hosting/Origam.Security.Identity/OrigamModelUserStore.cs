using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Origam.Security.Identity
{
    public class OrigamModelUserStore : IUserStore<OrigamUser>
    {
        public Task CreateAsync(OrigamUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(OrigamUser user)
        {
            throw new NotImplementedException();
        }

        public Task<OrigamUser> FindByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<OrigamUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(new OrigamUser(userName));
        }

        public Task UpdateAsync(OrigamUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
