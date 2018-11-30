using System;
using System.Collections.Generic;

namespace Origam.ServerCore
{
    public class MenuLookupIndex
    {
        private readonly Dictionary<Guid, ICollection<Guid>> menuToAllowedLookups = 
            new Dictionary<Guid, ICollection<Guid>>();
            
        public void AddIfNotPresent(Guid menuId, HashSet<Guid> containedLookups)
        {
            if (menuToAllowedLookups.ContainsKey(menuId)) return;
            menuToAllowedLookups.Add(menuId, containedLookups);
        }

        public bool IsAllowed(Guid menuItemId, Guid lookupId)
        {
            if (!menuToAllowedLookups.ContainsKey(menuItemId)) return false;
            return menuToAllowedLookups[menuItemId].Contains(lookupId);
        }
    }
}