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
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;
using Origam.UI;

namespace Origam.Schema;

public abstract class AbstractSchemaItemProvider : ISchemaItemProvider
{
    public AbstractSchemaItemProvider() { }

    // String for root item type
    public abstract string RootItemType { get; }
    #region ISchemaItemProvider Members
    ISchemaItemCollection _childItems;
#if ! ORIGAM_CLIENT
    bool _childItemsPopulated = false;
#endif
    public virtual ISchemaItemCollection ChildItems
    {
        get
        {
            if (_childItems == null)
            {
                _childItems = SchemaItemCollection.Create(
                    persistence: this.PersistenceProvider,
                    provider: this,
                    parentItem: null
                );
            }
            ISchemaItemCollection childItems;
#if ! ORIGAM_CLIENT
            // caching does not work properly with model localization
            // so we do it only for architect
            if (_childItemsPopulated)
            {
                childItems = _childItems;
            }
            else
            {
#endif
                childItems = LoadChildItems();
#if ! ORIGAM_CLIENT
                _childItems = childItems;
                _childItemsPopulated = true;
            }
#endif
            return childItems;
        }
    }

    public ISchemaItemCollection LoadChildItems()
    {
        ISchemaItemCollection childItems = SchemaItemCollection.Create(
            persistence: this.PersistenceProvider,
            provider: this,
            parentItem: null
        );
        childItems.AddRange(value: ChildItemsByType<ISchemaItem>(itemType: RootItemType).ToArray());
        return childItems;
    }

    public virtual List<T> ChildItemsByType<T>(string itemType)
        where T : ISchemaItem
    {
        List<T> list = PersistenceProvider.RetrieveListByCategory<T>(category: itemType);
        var result = new List<T>();
        foreach (T item in list)
        {
            if (item.ParentItemId == Guid.Empty)
            {
                item.RootProvider = this;
                result.Add(item: item);
            }
        }
        return result;
    }

    public virtual List<ISchemaItem> ChildItemsByGroup(SchemaItemGroup group)
    {
        var list = new List<ISchemaItem>();
        foreach (ISchemaItem item in ChildItems)
        {
            if (item.Group == null | group == null)
            {
                if (item.Group == null & group == null)
                {
                    list.Add(item: item);
                }
            }
            else if (item.Group.PrimaryKey.Equals(obj: group.PrimaryKey))
            {
                list.Add(item: item);
            }
        }
        return list;
    }

    public bool HasChildItems
    {
        get { return this.ChildItems.Count > 0; }
    }

    public bool HasChildItemsByType(string itemType)
    {
        return ChildItemsByType<AbstractSchemaItem>(itemType: itemType).Count > 0;
    }

    public bool HasChildItemsByGroup(SchemaItemGroup group)
    {
        return this.ChildItemsByGroup(group: group).Count > 0;
    }

    IPersistenceProvider _persistenceProvider;
    public IPersistenceProvider PersistenceProvider
    {
        get { return _persistenceProvider; }
        set
        {
#if ! ORIGAM_CLIENT
            if (_persistenceProvider != null)
            {
                _persistenceProvider.InstancePersisted -= _persistenceProvider_InstancePersisted;
                System.Diagnostics.Debug.Assert(_childItems == null || _childItems.Count == 0);
            }
#endif
            _persistenceProvider = value;
#if ! ORIGAM_CLIENT
            if (_persistenceProvider != null)
            {
                _persistenceProvider.InstancePersisted += _persistenceProvider_InstancePersisted;
                System.Diagnostics.Debug.Assert(_childItems == null || _childItems.Count == 0);
            }
#endif
        }
    }

#if ! ORIGAM_CLIENT
    void _persistenceProvider_InstancePersisted(object sender, IPersistent persistedObject)
    {
        ISchemaItem persistedItem = persistedObject as ISchemaItem;
        if (persistedItem != null)
        {
            if (persistedItem.IsDeleted)
            {
                if (persistedItem.RootProvider != null && persistedItem.RootProvider.Equals(this))
                {
                    ClearCache();
                }
            }
            else
            {
                if (!_childItemsPopulated)
                {
                    return;
                }
                foreach (ISchemaItem item in this.ChildItems)
                {
                    if (item.PrimaryKey.Equals(persistedItem.PrimaryKey))
                    {
                        ClearCache();
                        break;
                    }
                }
            }
        }
    }
#endif

    public void ClearCache()
    {
#if ! ORIGAM_CLIENT
        if (_childItemsPopulated)
        {
            _childItemsPopulated = false;
            _childItems.DeleteItemsOnClear = false;
            _childItems.Clear();
            _childItems.DeleteItemsOnClear = true;
        }
#endif
    }

    private ISchemaItemProvider _rootProvider = null;
    public ISchemaItemProvider RootProvider
    {
        get { return _rootProvider; }
        set { _rootProvider = value; }
    }
    public List<ISchemaItem> ChildItemsRecursive
    {
        get
        {
            var items = new List<ISchemaItem>();
            foreach (ISchemaItem item in this.ChildItems)
            {
                items.Add(item: item);
                items.AddRange(collection: GetChildItemsRecursive(parentItem: item));
            }
            return items;
        }
    }
    public virtual bool AutoCreateFolder
    {
        get { return false; }
    }
    public abstract string Group { get; }
    #endregion
    #region IBrowserNode Members
    [Browsable(browsable: false)]
    public bool Hide
    {
        get { return false; }
        set
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorSetHide")
            );
        }
    }
    public bool HasChildNodes
    {
        get { return this.ChildNodes().Count > 0; }
    }
    public bool CanRename
    {
        get { return false; }
    }
    public bool CanDelete
    {
        get { return false; }
    }

    public void Delete()
    {
        throw new InvalidOperationException(
            message: ResourceUtils.GetString(key: "ErrorDeleteProvider")
        );
    }

    public abstract string Icon { get; }
    public virtual byte[] NodeImage
    {
        get { return null; }
    }
    public List<SchemaItemGroup> ChildGroups
    {
        get
        {
            List<SchemaItemGroup> result = new List<SchemaItemGroup>();
            List<SchemaItemGroup> list =
                this.PersistenceProvider.RetrieveListByGroup<SchemaItemGroup>(
                    primaryKey: new ModelElementKey(id: Guid.Empty)
                );
            foreach (SchemaItemGroup group in list)
            {
                if (group.RootItemType == RootItemType)
                {
                    result.Add(item: group);
                    group.RootProvider = this;
                }
            }
            return result;
        }
    }

    public Origam.UI.BrowserNodeCollection ChildNodes()
    {
        BrowserNodeCollection col = new BrowserNodeCollection();
        // get root groups
        foreach (IBrowserNode2 nod in this.ChildGroups)
        {
            col.Add(value: nod);
        }
        // only return nodes without groups
        foreach (ISchemaItem item in this.ChildItems)
        {
            if (item.Group == null)
            {
                col.Add(value: item);
            }
        }
        return col;
    }

    [Browsable(browsable: false)]
    public string NodeId
    {
        get { return this.GetType().ToString(); }
    }
    public virtual string NodeText
    {
        get { return "Schema Items"; }
        set
        {
            throw new ArgumentException(message: ResourceUtils.GetString(key: "ErrorRenameNode"));
        }
    }
    public virtual string NodeToolTipText
    {
        get
        {
            // TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
            return null;
        }
    }

    public bool CanMove(IBrowserNode2 newNode)
    {
        return false;
    }

    [Browsable(browsable: false)]
    public IBrowserNode2 ParentNode
    {
        get { return null; }
        set
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorMoveProvider")
            );
        }
    }

    [Browsable(browsable: false)]
    public virtual string FontStyle
    {
        get { return "Regular"; }
    }
    #endregion
    #region ISchemaItemFactory Members
    public virtual T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
        where T : class, ISchemaItem
    {
        return NewItem<T>(schemaExtensionId: schemaExtensionId, group: group, itemName: null);
    }

    protected T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group, string itemName)
        where T : ISchemaItem
    {
        T item;
        if (((IList)NewItemTypes).Contains(value: typeof(T)))
        {
            item = (T)
                typeof(T)
                    .GetConstructor(types: new Type[] { typeof(Guid) })
                    .Invoke(parameters: new object[] { schemaExtensionId });
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "type",
                actualValue: typeof(T),
                message: ResourceUtils.GetString(
                    key: "ErrorTypeNotSupported",
                    args: this.GetType().Name
                )
            );
        }
        item.Group = group;
        item.RootProvider = this;
        item.PersistenceProvider = PersistenceProvider;
        if (!string.IsNullOrEmpty(value: itemName))
        {
            item.Name = itemName;
        }
        ChildItems.Add(item: item);
        ItemCreated?.Invoke(obj: item);
        return item;
    }

    public virtual SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
    {
        SchemaItemGroup group = new SchemaItemGroup(extensionId: schemaExtensionId);
        group.Name = SchemaItemGroup.GetNextDefaultName(
            defaultName: groupName,
            childGroups: ChildGroups
        );
        group.PersistenceProvider = this.PersistenceProvider;
        group.RootItemType = this.RootItemType;
        group.RootProvider = this;
        group.Persist();
        return group;
    }

    private List<Type> _childItemTypes = new();

    [Browsable(browsable: false)]
    public List<Type> ChildItemTypes
    {
        get
        {
            foreach (Type[] entry in ExtensionChildItemTypes)
            {
                if (
                    (entry[0].Equals(o: GetType()) || GetType().IsSubclassOf(c: entry[0]))
                    && !_childItemTypes.Contains(item: entry[1])
                )
                {
                    _childItemTypes.Add(item: entry[1]);
                }
            }
            return _childItemTypes;
        }
    }

    public static List<Type[]> ExtensionChildItemTypes { get; } = new();

    [Browsable(browsable: false)]
    public virtual Type[] NewItemTypes
    {
        get { return ChildItemTypes.ToArray(); }
    }

    [Browsable(browsable: false)]
    public virtual IList<string> NewTypeNames
    {
        get { return new List<string>(); }
    }

    /// <summary>
    /// By default all NewItemTypes are nameable. Override if only a subset of types can
    /// be populated with NewTypeNames.
    /// </summary>
    [Browsable(browsable: false)]
    public virtual Type[] NameableTypes
    {
        get { return NewItemTypes; }
    }
    public event Action<ISchemaItem> ItemCreated;
    #endregion
    private List<ISchemaItem> GetChildItemsRecursive(ISchemaItem parentItem)
    {
        var items = new List<ISchemaItem>();
        foreach (ISchemaItem childItem in parentItem.ChildItems)
        {
            items.Add(item: childItem);
            items.AddRange(collection: GetChildItemsRecursive(parentItem: childItem));
        }
        return items;
    }

    public SchemaItemGroup GetGroup(string name)
    {
        foreach (SchemaItemGroup group in this.ChildGroups)
        {
            if (group.Name == name)
            {
                return group;
            }
        }
        return null;
    }

    public ISchemaItem GetChildByName(string name, string itemType)
    {
        foreach (ISchemaItem item in this.ChildItems)
        {
            if (item.Name == name & item.ItemType == itemType)
            {
                return item;
            }
        }
        return null;
    }

    #region IComparable Members
    public int CompareTo(object obj)
    {
        AbstractSchemaItemProvider compareItem = obj as AbstractSchemaItemProvider;
        if (compareItem == null)
        {
            throw new InvalidCastException();
        }

        return this.NodeText.CompareTo(strB: compareItem.NodeText);
    }
    #endregion
}
