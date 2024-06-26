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
using System.ComponentModel;
using System.Data;
using System.Collections;
using Origam.Services;
using Origam.Schema;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.NamespaceMapping;
using Origam.Extensions;
using Origam.UI;

namespace Origam.Workbench.Services;
public delegate void SchemaServiceEventHandler(object sender, SchemaServiceEventArgs e);

public class SchemaServiceEventArgs : System.EventArgs
{
	ISchemaItemProvider provider;
	
	public ISchemaItemProvider Provider 
	{
		get 
		{
			return provider;
		}
	}
	
	public SchemaServiceEventArgs(ISchemaItemProvider provider)
	{
		this.provider = provider;
	}
}
public class SchemaService : AbstractService, ISchemaService
{
	public event SchemaServiceEventHandler ProviderAdded;
	public event SchemaServiceEventHandler ProviderRemoved;
	public event EventHandler ActiveNodeChanged;
	public event EventHandler<bool> SchemaLoaded;
	public event EventHandler SchemaChanged;
	public event CancelEventHandler SchemaUnloading;
    public event EventHandler SchemaUnloaded;
    public SchemaService()
    {
    }
    /// <summary>
    /// Initializes the service with a specific package id - no matter what package is loaded - used for tests.
    /// </summary>
    /// <param name="activeSchemaExtensionId"></param>
    public SchemaService(Guid activeSchemaExtensionId)
    {
        _activeSchemaExtensionId = activeSchemaExtensionId;
    }
    
    private Hashtable _providers = new Hashtable();
	
	#region Public Properties
	public ArrayList LoadedPackages
	{
		get
		{
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			return persistence.SchemaProvider.RetrieveList<Package>(null).ToArrayList();
		}
	}
	public ArrayList AllPackages
	{
		get
		{
			IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			return persistence.SchemaListProvider.RetrieveList<Package>(null).ToArrayList();
		}
	}
	protected Guid _activeSchemaExtensionId;
	public Guid ActiveSchemaExtensionId
	{
		get
		{
			return _activeSchemaExtensionId;
		}
	}
    private Guid _storageSchemaExtensionId = Guid.Empty;
    public Guid StorageSchemaExtensionId
    {
        get
        {
            if (_storageSchemaExtensionId == Guid.Empty)
            {
                return _activeSchemaExtensionId;
            }
            return _storageSchemaExtensionId;
        }
        set
        {
            _storageSchemaExtensionId = value;
        }
    }
    protected Package _activeExtension = null;
	public Package ActiveExtension
	{
		get
		{
			return _activeExtension;
		}
	}
	protected bool _isSchemaLoaded = false;
	public bool IsSchemaLoaded
	{
		get
		{
			return _isSchemaLoaded;
		}
	}
	
	#endregion
	#region Public Methods
	public bool IsItemFromExtension(object item)
	{
		if(item is AbstractSchemaItem)
		{
			if((item as AbstractSchemaItem).SchemaExtensionId != this.ActiveSchemaExtensionId)
			{
				return false;
			}
		}
		else if (item is SchemaItemGroup)
		{
			if((item as SchemaItemGroup).SchemaExtensionId != this.ActiveSchemaExtensionId)
			{
				return false;
			}
		}
		
		return true;
	}
	public bool CanDeleteItem(object item)
	{
		if(item is AbstractSchemaItem || item is SchemaItemGroup)
		{
			return IsItemFromExtension(item);
		}
		if(item is Package)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public bool CanEditItem(object item)
	{
		if(item is AbstractSchemaItem)
		{
			if(! IsItemFromExtension(item)) return false;
			// check if the item is checked out by the user
			return true;
		}
		else if(item is Package)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public virtual bool UnloadSchema()
	{
		CancelEventArgs e = new CancelEventArgs(false);
        _isSchemaLoaded = false;
        OnSchemaUnloading(e);
		if(e.Cancel)
		{
            _isSchemaLoaded = true;
			return false;
		}
		IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		if(persistence != null)
		{
			persistence.SchemaProvider.InstancePersisted -= SchemaProvider_InstancePersisted;
		}
		RemoveAllProviders();
		
		_lastAddedNodeParent = null;
		_lastAddedType = null;
		_activeExtension = null;
		_activeSchemaExtensionId = Guid.Empty;
		SecurityManager.Reset();
        OnSchemaUnloaded();
		return true;
	}
	public bool LoadSchema(Guid schemaExtensionId, bool isInteractive = false)
	{
		if( ! UnloadSchema()) return false;
	
		IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		persistence.SchemaProvider.InstancePersisted += SchemaProvider_InstancePersisted;
		PropertyToNamespaceMapping.Init();
		Package extension = persistence.LoadSchema(schemaExtensionId);
		
		_activeSchemaExtensionId = (Guid)extension.PrimaryKey["Id"];
		_activeExtension = persistence.SchemaProvider.RetrieveInstance(typeof(Package), extension.PrimaryKey) as Package;
		OnSchemaLoaded(isInteractive);
		return true;
	}
	
	public void AddProvider(AbstractSchemaItemProvider provider)
	{
		_providers.Add(provider.GetType(), provider);
		provider.PersistenceProvider = (ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService).SchemaProvider;
		foreach(Package package in this.LoadedPackages)
		{
			package.AddProvider(provider);
		}
		OnProviderAdded(new SchemaServiceEventArgs(provider));
	}
    public void ClearProviderCaches()
    {
        foreach(ISchemaItemProvider provider in Providers)
        {
            provider.ClearCache();
        }
    }
	public void RemoveProvider(ISchemaItemProvider provider)
	{
#if ! ORIGAM_CLIENT
        provider.ClearCache();
#endif
        provider.PersistenceProvider = null;
		_providers.Remove(provider.GetType());
		foreach(Package package in this.LoadedPackages)
		{
			foreach (IBrowserNode group in package.ChildNodes())	
			{
				if(group.ChildNodes().Contains(provider))
				{
					group.ChildNodes().Remove(provider);
				}
			}
		}
		OnProvidedRemoved(new SchemaServiceEventArgs(provider));
	}
	public void RemoveAllProviders()
	{
		ArrayList keys = new ArrayList(_providers.Keys);
		foreach(object key in keys)
		{
			this.RemoveProvider(_providers[key] as ISchemaItemProvider);
		}
	}
	public ISchemaItemProvider[] Providers
	{
		get
		{
			ISchemaItemProvider[] result = new ISchemaItemProvider[_providers.Count];
			int i = 0;
			foreach(ISchemaItemProvider provider in _providers.Values)
			{
				result[i] = provider;
				i++;
			}
			return result;
		}
	}
	public ISchemaItemProvider GetProvider(Type type)
	{
		if(type.IsInterface)
		{
			foreach(DictionaryEntry entry in _providers)
			{
				foreach(Type interf in entry.Value.GetType().GetInterfaces())
				{
					if(type.Equals(interf)) return entry.Value as ISchemaItemProvider;
				}
			}
		}
		else
		{
			return _providers[type] as ISchemaItemProvider;
		}
		return null;
	}
	public T GetProvider<T>()  where T : ISchemaItemProvider
		=> (T)GetProvider(typeof(T));
	public ISchemaItemProvider GetProvider(string type)
	{
		foreach(Type t in _providers.Keys)
		{
			if(t.FullName == type)
			{
				return _providers[t] as ISchemaItemProvider;
			}
		}
		return null;
	}
	public void AddSchemaItem(AbstractSchemaItem item)
	{
	}
	public void RemoveSchemaItem(AbstractSchemaItem item)
	{
	}
	public void UpdateSchemaItem(AbstractSchemaItem item)
	{
	}
	
	protected ISchemaItemFactory _lastAddedNodeParent;
	public ISchemaItemFactory LastAddedNodeParent
	{
		get
		{
			return _lastAddedNodeParent;
		}
		set
		{
			_lastAddedNodeParent = value;
		}
	}
	protected Type _lastAddedType;
	public Type LastAddedType
	{
		get
		{
			return _lastAddedType;
		}
		set
		{
			_lastAddedType = value;
		}
	}
	#endregion
	#region Events
	protected void OnProviderAdded(SchemaServiceEventArgs e)
	{
		if (ProviderAdded != null) 
		{
			ProviderAdded(this, e);
		}
	}
	
	protected void OnProvidedRemoved(SchemaServiceEventArgs e)
	{
		if (ProviderRemoved != null) 
		{
			ProviderRemoved(this, e);
		}
	}
	protected void OnActiveNodeChanged(EventArgs e)
	{
		if (ActiveNodeChanged != null) 
		{
			ActiveNodeChanged(this, e);
		}
	}
	protected void OnSchemaLoaded(bool isInteractive)
	{
		_isSchemaLoaded = true;
		if (SchemaLoaded != null) 
		{
			SchemaLoaded(this, isInteractive);
		}
	}
	protected void OnSchemaUnloading(CancelEventArgs e)
	{
		if (SchemaUnloading != null) 
		{
			SchemaUnloading(this, e);
		}
	}
    protected void OnSchemaUnloaded()
    {
        if (SchemaUnloaded != null)
        {
            SchemaUnloaded(this, EventArgs.Empty);
        }
    }
    
    protected void OnSchemaChanged(object sender, EventArgs e)
	{
		if (SchemaChanged != null) 
		{
			SchemaChanged(sender, e);
		}
	}
	#endregion
	#region Event Handlers
	protected virtual void SchemaProvider_InstancePersisted(object sender, IPersistent persistedObject)
	{
		OnSchemaChanged(persistedObject, EventArgs.Empty);
	}
	#endregion
}
