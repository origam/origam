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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Origam;
/// <summary>
/// Summary description for OrigamUserContext.
/// </summary>
public class OrigamUserContext
{
	private static IDictionary<string, Hashtable> _contexts
        = new Dictionary<string, Hashtable>();
    private static object _lock = new object();
	public static Hashtable Context => GetContext(UserKey());
    public static void Reset()
    { 
        string currentUsername = UserKey();
        Reset(currentUsername);
    }
    
    public static void Reset(string username)
    {
        lock (_lock)
        {
            DisposeCachedObjects(username);
            _contexts.Remove(username);
        }
    }
    public static void ResetAll()
    {
        lock (_lock)
        {
            foreach (var contextEntry in _contexts)
            {
                DisposeCachedObjects(contextEntry.Key);
            }
            _contexts.Clear();
        }
    }
    private static Hashtable GetContext(string key)
    {
        lock (_lock)
        {
            if (!_contexts.ContainsKey(key))
            {
                _contexts.Add(key, new Hashtable());
            }
            return _contexts[key] as Hashtable;
        }
    }
    private static void DisposeCachedObjects(string username)
    {
        Hashtable context = GetContext(username);
        foreach (DictionaryEntry entry in context)
        {
            IDisposable disposableObject = entry.Value as IDisposable;
            if (disposableObject != null)
            {
                disposableObject.Dispose();
            }
        }
    }
    private static string UserKey()
    {            
        if (!SecurityManager.CurrentPrincipal.Identity.IsAuthenticated)
        {
            return "guest";
        }            
        return SecurityManager.CurrentPrincipal.Identity.Name;
    }
    public static Hashtable GetContextItem(string cacheName)
    {
        lock (_lock)
        {
            if (!Context.Contains(cacheName))
            {
                Context.Add(cacheName, new Hashtable());
            }
            return (Hashtable)Context[cacheName];
        }
    }
}
