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
using System.ComponentModel;
using System.Linq;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.NamespaceMapping;
using Origam.Schema;
using Origam.Services;
using Origam.UI;

namespace Origam.Workbench.Services;

public delegate void SchemaServiceEventHandler(object sender, SchemaServiceEventArgs e);

public class SchemaServiceEventArgs : System.EventArgs
{
    ISchemaItemProvider provider;

    public ISchemaItemProvider Provider
    {
        get { return provider; }
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

    public SchemaService() { }

    /// <summary>
    /// Initializes the service with a specific package id - no matter what package is loaded - used for tests.
    /// </summary>
    /// <param name="activeSchemaExtensionId"></param>
    public SchemaService(Guid activeSchemaExtensionId)
    {
        _activeSchemaExtensionId = activeSchemaExtensionId;
    }

    private Dictionary<Type, ISchemaItemProvider> _providers = new();

    #region Public Properties
    public List<Package> LoadedPackages
    {
        get
        {
            IPersistenceService persistence =
                ServiceManager.Services.GetService<IPersistenceService>();
            return persistence.SchemaProvider.RetrieveList<Package>(filter: null);
        }
    }
    public List<Package> AllPackages
    {
        get
        {
            IPersistenceService persistence =
                ServiceManager.Services.GetService<IPersistenceService>();
            return persistence.SchemaListProvider.RetrieveList<Package>(filter: null);
        }
    }
    protected Guid _activeSchemaExtensionId;
    public Guid ActiveSchemaExtensionId
    {
        get { return _activeSchemaExtensionId; }
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
        set { _storageSchemaExtensionId = value; }
    }
    protected Package _activeExtension = null;
    public Package ActiveExtension
    {
        get { return _activeExtension; }
    }
    protected bool _isSchemaLoaded = false;
    public bool IsSchemaLoaded
    {
        get { return _isSchemaLoaded; }
    }

    #endregion
    #region Public Methods
    public bool IsItemFromExtension(object item)
    {
        if (item is ISchemaItem)
        {
            if ((item as ISchemaItem).SchemaExtensionId != this.ActiveSchemaExtensionId)
            {
                return false;
            }
        }
        else if (item is SchemaItemGroup)
        {
            if ((item as SchemaItemGroup).SchemaExtensionId != this.ActiveSchemaExtensionId)
            {
                return false;
            }
        }

        return true;
    }

    public bool CanDeleteItem(object item)
    {
        if (item is ISchemaItem || item is SchemaItemGroup)
        {
            return IsItemFromExtension(item: item);
        }
        if (item is Package)
        {
            return true;
        }

        return false;
    }

    public bool CanEditItem(object item)
    {
        if (item is ISchemaItem)
        {
            if (!IsItemFromExtension(item: item))
            {
                return false;
            }
            // check if the item is checked out by the user
            return true;
        }

        if (item is Package)
        {
            return true;
        }

        return false;
    }

    public virtual bool UnloadSchema()
    {
        CancelEventArgs e = new CancelEventArgs(cancel: false);
        _isSchemaLoaded = false;
        OnSchemaUnloading(e: e);
        if (e.Cancel)
        {
            _isSchemaLoaded = true;
            return false;
        }
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        if (persistence != null)
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
        if (!UnloadSchema())
        {
            return false;
        }

        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        persistence.SchemaProvider.InstancePersisted += SchemaProvider_InstancePersisted;
        PropertyToNamespaceMapping.Init();
        Package extension = persistence.LoadSchema(schemaExtensionId: schemaExtensionId);

        _activeSchemaExtensionId = (Guid)extension.PrimaryKey[key: "Id"];
        _activeExtension =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(Package),
                primaryKey: extension.PrimaryKey
            ) as Package;
        OnSchemaLoaded(isInteractive: isInteractive);
        return true;
    }

    public void AddProvider(AbstractSchemaItemProvider provider)
    {
        _providers.Add(key: provider.GetType(), value: provider);
        provider.PersistenceProvider = (
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService
        ).SchemaProvider;
        foreach (Package package in LoadedPackages)
        {
            package.AddProvider(provider: provider);
        }
        OnProviderAdded(e: new SchemaServiceEventArgs(provider: provider));
    }

    public void ClearProviderCaches()
    {
        foreach (ISchemaItemProvider provider in Providers)
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
        _providers.Remove(key: provider.GetType());
        foreach (Package package in this.LoadedPackages)
        {
            foreach (IBrowserNode group in package.ChildNodes())
            {
                if (group.ChildNodes().Contains(value: provider))
                {
                    group.ChildNodes().Remove(value: provider);
                }
            }
        }
        OnProvidedRemoved(e: new SchemaServiceEventArgs(provider: provider));
    }

    public void RemoveAllProviders()
    {
        var keys = _providers.Keys.ToList();
        foreach (Type key in keys)
        {
            RemoveProvider(provider: _providers[key: key]);
        }
    }

    public ISchemaItemProvider[] Providers
    {
        get
        {
            ISchemaItemProvider[] result = new ISchemaItemProvider[_providers.Count];
            int i = 0;
            foreach (ISchemaItemProvider provider in _providers.Values)
            {
                result[i] = provider;
                i++;
            }
            return result;
        }
    }

    public ISchemaItemProvider GetProvider(Type type)
    {
        if (type.IsInterface)
        {
            foreach (var entry in _providers)
            {
                foreach (Type interf in entry.Value.GetType().GetInterfaces())
                {
                    if (type.Equals(o: interf))
                    {
                        return entry.Value;
                    }
                }
            }
        }
        else
        {
            _providers.TryGetValue(key: type, value: out var provider);
            return provider;
        }
        return null;
    }

    public T GetProvider<T>()
        where T : ISchemaItemProvider => (T)GetProvider(type: typeof(T));

    public ISchemaItemProvider GetProvider(string type)
    {
        foreach (Type t in _providers.Keys)
        {
            if (t.FullName == type)
            {
                return _providers[key: t];
            }
        }
        return null;
    }

    public void AddSchemaItem(ISchemaItem item) { }

    public void RemoveSchemaItem(ISchemaItem item) { }

    public void UpdateSchemaItem(ISchemaItem item) { }

    protected ISchemaItemFactory _lastAddedNodeParent;
    public ISchemaItemFactory LastAddedNodeParent
    {
        get { return _lastAddedNodeParent; }
        set { _lastAddedNodeParent = value; }
    }
    protected Type _lastAddedType;
    public Type LastAddedType
    {
        get { return _lastAddedType; }
        set { _lastAddedType = value; }
    }
    #endregion
    #region Events
    protected void OnProviderAdded(SchemaServiceEventArgs e)
    {
        if (ProviderAdded != null)
        {
            ProviderAdded(sender: this, e: e);
        }
    }

    protected void OnProvidedRemoved(SchemaServiceEventArgs e)
    {
        if (ProviderRemoved != null)
        {
            ProviderRemoved(sender: this, e: e);
        }
    }

    protected void OnActiveNodeChanged(EventArgs e)
    {
        if (ActiveNodeChanged != null)
        {
            ActiveNodeChanged(sender: this, e: e);
        }
    }

    protected void OnSchemaLoaded(bool isInteractive)
    {
        _isSchemaLoaded = true;
        if (SchemaLoaded != null)
        {
            SchemaLoaded(sender: this, e: isInteractive);
        }
    }

    protected void OnSchemaUnloading(CancelEventArgs e)
    {
        if (SchemaUnloading != null)
        {
            SchemaUnloading(sender: this, e: e);
        }
    }

    protected void OnSchemaUnloaded()
    {
        if (SchemaUnloaded != null)
        {
            SchemaUnloaded(sender: this, e: EventArgs.Empty);
        }
    }

    protected void OnSchemaChanged(object sender, EventArgs e)
    {
        if (SchemaChanged != null)
        {
            SchemaChanged(sender: sender, e: e);
        }
    }
    #endregion
    #region Event Handlers
    protected virtual void SchemaProvider_InstancePersisted(
        object sender,
        IPersistent persistedObject
    )
    {
        OnSchemaChanged(sender: persistedObject, e: EventArgs.Empty);
    }
    #endregion
}
