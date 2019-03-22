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
