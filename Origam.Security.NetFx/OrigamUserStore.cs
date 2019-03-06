using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Origam.Security.Identity
{
    public class OrigamUserStore : IUserStore<OrigamUser>
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
            throw new NotImplementedException();
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