#region license

/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace Origam.ServerCore.Authorization
{
    public class PersistedGrantStore: IPersistedGrantStore
    {
        private readonly List<PersistedGrant> grants = new List<PersistedGrant>();
        
        public Task StoreAsync(PersistedGrant grant)
        {
            grants.Add(grant);
            return Task.CompletedTask;
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return Task.FromResult(grants.FirstOrDefault(x => x.Key == key));
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            return Task.FromResult(grants.Select(x=>x));
        }

        public Task RemoveAsync(string key)
        {
            grants.RemoveAll(x => x.Key == key);
            return Task.CompletedTask;
        }

        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            grants.RemoveAll(x => x.SubjectId == subjectId && x.ClientId == clientId);
            return Task.CompletedTask;
        }

        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            grants.RemoveAll(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type);
            return Task.CompletedTask;
        }
    }
}