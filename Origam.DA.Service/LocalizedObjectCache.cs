#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Threading;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service
{
    class LocalizedObjectCache
    {
        private readonly Dictionary<string, Dictionary<Guid, IFilePersistent>> objectDict
            = new Dictionary<string, Dictionary<Guid, IFilePersistent>>();

        private string locale => Thread.CurrentThread.CurrentUICulture.Name;

        public IEnumerable<KeyValuePair<Guid, IFilePersistent>> LocalizedPairs => 
            objectDict.ContainsKey(locale)
                ? objectDict[locale].Cast<KeyValuePair<Guid, IFilePersistent>>()
                : Enumerable.Empty<KeyValuePair<Guid, IFilePersistent>>();

        public LocalizedObjectCache(LocalizedObjectCache other)
        {
            foreach (var pair in other.objectDict)
            {
                var innerDict = new Dictionary<Guid, IFilePersistent>(pair.Value);
                objectDict.Add(pair.Key, innerDict);
            }
        }

        public LocalizedObjectCache()
        {
        }

        public bool Contains(Guid id)
        {
            return objectDict.ContainsKey(locale) &&
                   objectDict[locale].ContainsKey(id);
        }

        public Maybe<IFilePersistent> Get(Guid id)
        {
            return Contains(id)
                ? Maybe<IFilePersistent>.From(objectDict[locale][id])
                : null;
        }

        public void Add(Guid id, IFilePersistent instance)
        {
            if (!objectDict.ContainsKey(locale))
            {
                objectDict.Add(locale, new Dictionary<Guid, IFilePersistent>());
            }
            objectDict[locale].Add(id, instance);
        }
        public IFilePersistent this[Guid id] {
            set {
                if (!objectDict.ContainsKey(locale))
                {
                    objectDict.Add(locale, new Dictionary<Guid, IFilePersistent>());
                }
                objectDict[locale][id] = value;
            }
        }

        public void Remove(Guid id)
        {
            foreach (var dictionary in objectDict.Values)
            {
                dictionary.Remove(id);
            }
        }

        public void AddRange(IEnumerable<IFilePersistent> newInstances)
        {
            foreach (IFilePersistent instance in newInstances)
            {
                Add(instance.Id, instance);
            }
        }
    }
}