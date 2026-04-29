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
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.Common.ObjectPersistence.Attributes;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;
using Origam.UI;
using InvalidCastException = System.InvalidCastException;

namespace Origam.Schema;

[XmlModelRoot(category: "group")]
[ClassMetaVersion(versionStr: "6.0.0")]
[XmlNamespaceName(xmlNamespaceName: "g")]
public class SchemaItemGroup : AbstractPersistent, ISchemaItemProvider, IFilePersistent
{
    public SchemaItemGroup()
    {
        PrimaryKey = new ModelElementKey();
    }

    public SchemaItemGroup(Guid extensionId)
        : this()
    {
        SchemaExtensionId = extensionId;
    }

    public SchemaItemGroup(Key primaryKey)
        : base(primaryKey: primaryKey, correctKeys: primaryKey.KeyArray) { }

    public override string ToString() => Name;

    #region Properties
    [XmlAttribute(AttributeName = "rootItemType")]
    public string RootItemType { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }

    [XmlParent(type: typeof(Package))]
    public Guid SchemaExtensionId;

    [Browsable(browsable: false)]
    public Package Package
    {
        get
        {
            var key = new ModelElementKey { Id = SchemaExtensionId };
            return (Package)
                PersistenceProvider.RetrieveInstance(type: typeof(Package), primaryKey: key);
        }
        set => SchemaExtensionId = (Guid)value.PrimaryKey[key: "Id"];
    }

    [Browsable(browsable: false)]
    public ISchemaItem ParentItem { get; set; }

    [XmlParent(type: typeof(SchemaItemGroup))]
    public Guid ParentGroupId;

    [Browsable(browsable: false)]
    public SchemaItemGroup ParentGroup
    {
        get
        {
            var key = new ModelElementKey { Id = ParentGroupId };
            return (SchemaItemGroup)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(SchemaItemGroup),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                ParentGroupId = Guid.Empty;
            }
            else
            {
                ParentGroupId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [Browsable(browsable: false)]
    public string Path => GetPath(item: this);
    public bool IsFileRootElement => FileParentId == Guid.Empty;
    #endregion
    private string GetPath(SchemaItemGroup item)
    {
        if (ParentGroup == null)
        {
            return Name;
        }
        return System.IO.Path.Combine(path1: ParentGroup.Path, path2: Name);
    }

    [Browsable(browsable: false)]
    public SchemaItemGroup RootGroup => GetRootGroup(parentItem: this);

    private SchemaItemGroup GetRootGroup(SchemaItemGroup parentItem)
    {
        return parentItem.ParentGroup == null
            ? parentItem
            : GetRootGroup(parentItem: parentItem.ParentGroup);
    }

    private List<ISchemaItem> GetChildItemsRecursive(ISchemaItem parentItem)
    {
        var items = new List<ISchemaItem>();
        foreach (var childItem in parentItem.ChildItems)
        {
            items.Add(item: childItem);
            items.AddRange(collection: GetChildItemsRecursive(parentItem: childItem));
        }
        return items;
    }

    public ISchemaItem GetChildByName(string name, string itemType)
    {
        foreach (var item in ChildItems)
        {
            if ((item.Name == name) && (item.ItemType == itemType))
            {
                return item;
            }
        }
        return null;
    }

    #region IBrowserNode2 Members
    [Browsable(browsable: false)]
    public bool Hide
    {
        get => !IsPersisted;
        set =>
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorSetHide")
            );
    }
    public bool CanDelete => true;

    public void Delete()
    {
        if (ParentItem?.DerivedFrom != null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorDeleteDerivedGroup")
            );
        }
        foreach (IBrowserNode2 node in ChildNodes())
        {
            node.Delete();
        }
        IsDeleted = true;
        Persist();
    }

    public bool CanMove(IBrowserNode2 newNode) => false;

    [Browsable(browsable: false)]
    public IBrowserNode2 ParentNode
    {
        get => null;
        set =>
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorMoveGroup")
            );
    }
    public byte[] NodeImage => null;

    [Browsable(browsable: false)]
    public string NodeId => PrimaryKey[key: "Id"].ToString();

    [Browsable(browsable: false)]
    public virtual string FontStyle => "Regular";
    #endregion
    #region IBrowserNode Members
    public bool HasChildNodes => this.ChildNodes().Count > 0;
    public bool CanRename => true;

    public BrowserNodeCollection ChildNodes()
    {
        var browserNodeCollection = new BrowserNodeCollection();
        // Child groups
        foreach (IBrowserNode2 node in ChildGroups)
        {
            browserNodeCollection.Add(value: node);
        }
        // Child nodes
        foreach (IBrowserNode2 node in this.ChildItems)
        {
            if (!((ISchemaItem)node).IsDeleted)
            {
                browserNodeCollection.Add(value: node);
            }
        }
        return browserNodeCollection;
    }

    public string NodeText
    {
        get => Name;
        set
        {
            Name = value;
            Persist();
        }
    }
    public string NodeToolTipText => null;
    public string Icon => "37_folder-3.png";
    #endregion
    #region ISchemaItemProvider Members
    public ISchemaItemCollection ChildItems
    {
        get
        {
            // We look for all child items of our parent schema item that have this group
            // We browse the collection because it has all the items correctly set
            var provider = ParentItem ?? RootProvider;
            var ISchemaItemCollection = SchemaItemCollection.Create(
                persistence: PersistenceProvider,
                provider: provider,
                parentItem: ParentItem
            );
            foreach (ISchemaItem item in provider.ChildItemsByGroup(group: this))
            {
                ISchemaItemCollection.Add(item: item);
            }
            return ISchemaItemCollection;
        }
    }

    public List<T> ChildItemsByType<T>(string itemType)
        where T : ISchemaItem
    {
        var list = new List<T>();
        // We look for all child items of our parent schema item that have this group
        // We browse the collection because it has all the items correctly set
        foreach (ISchemaItem item in ParentItem.ChildItemsByGroup(group: this))
        {
            list.Add(item: (T)item);
        }
        return list;
    }

    public List<ISchemaItem> ChildItemsByGroup(SchemaItemGroup group)
    {
        var list = new List<ISchemaItem>();
        foreach (var item in ChildItems)
        {
            if (
                (item.Group == null && group == null)
                || item.Group.PrimaryKey.Equals(obj: group.PrimaryKey)
            )
            {
                list.Add(item: item);
            }
        }
        return list;
    }

    public SchemaItemGroup GetGroup(string name)
    {
        return ChildGroups.FirstOrDefault(predicate: group => group.Name == name);
    }

    public bool HasChildItems => ChildItems.Count > 0;

    public bool HasChildItemsByType(string itemType) =>
        ChildItemsByType<ISchemaItem>(itemType: itemType).Count > 0;

    public bool HasChildItemsByGroup(SchemaItemGroup group) =>
        ChildItemsByGroup(group: group).Count > 0;

    public List<SchemaItemGroup> ChildGroups
    {
        get
        {
            // We retrieve all child groups
            var list = PersistenceProvider.RetrieveListByGroup<SchemaItemGroup>(
                primaryKey: PrimaryKey
            );
            // Set parent for each child
            foreach (var group in list)
            {
                group.RootProvider = RootProvider;
                group.ParentItem = ParentItem;
                group.ParentGroup = this;
            }
            return list;
        }
    }
    public IEnumerable<SchemaItemGroup> ChildGroupsRecursive
    {
        get
        {
            foreach (var childGroup in ChildGroups)
            {
                yield return childGroup;
                foreach (var innerChildGroup in childGroup.ChildGroupsRecursive)
                {
                    yield return innerChildGroup;
                }
                ;
            }
        }
    }
    public ISchemaItemProvider RootProvider { get; set; } = null;
    public List<ISchemaItem> ChildItemsRecursive
    {
        get
        {
            var items = new List<ISchemaItem>();
            foreach (var item in ChildItems)
            {
                items.Add(item: item);
                items.AddRange(collection: GetChildItemsRecursive(parentItem: item));
            }
            return items;
        }
    }
    public bool AutoCreateFolder => false;

    public void ClearCache() { }
    #endregion
    #region ISchemaItemFactory Members
    public List<Type> ChildItemTypes => new(collection: NewItemTypes);

    [Browsable(browsable: false)]
    public Type[] NewItemTypes
    {
        get
        {
            if (ParentItem == null)
            {
                return RootProvider?.NewItemTypes ?? new Type[0];
            }
            return (ParentItem as ISchemaItemFactory).NewItemTypes;
        }
    }

    [Browsable(browsable: false)]
    public virtual IList<string> NewTypeNames =>
        ParentItem == null
            ? RootProvider.NewTypeNames
            : (ParentItem as ISchemaItemFactory).NewTypeNames;

    /// <summary>
    /// By default all NewItemTypes are nameable. Override if only a subset of types can
    /// be populated with NewTypeNames.
    /// </summary>
    [Browsable(browsable: false)]
    public virtual Type[] NameableTypes => NewItemTypes;
    public event Action<ISchemaItem> ItemCreated;
    public string RelativeFilePath =>
        System.IO.Path.Combine(
            path1: Package.Name,
            path2: RootItemType,
            path3: Path.Replace(oldChar: '/', newChar: System.IO.Path.DirectorySeparatorChar)
                .Replace(oldChar: '\\', newChar: System.IO.Path.DirectorySeparatorChar),
            path4: PersistenceFiles.GroupFileName
        );
    public bool IsFolder => true;
    public Guid FileParentId
    {
        get => Guid.Empty;
        set { }
    }
    public IDictionary<string, Guid> ParentFolderIds =>
        new Dictionary<string, Guid>
        {
            { CategoryFactory.Create(type: typeof(Package)), SchemaExtensionId },
            { CategoryFactory.Create(type: typeof(SchemaItemGroup)), ParentGroupId },
        };

    public virtual T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
        where T : class, ISchemaItem
    {
        T newItem;
        if (ParentItem == null)
        {
            newItem = RootProvider.NewItem<T>(schemaExtensionId: schemaExtensionId, group: this);
        }
        else
        {
            newItem = (ParentItem as ISchemaItemFactory).NewItem<T>(
                schemaExtensionId: schemaExtensionId,
                group: this
            );
        }
        ItemCreated?.Invoke(obj: newItem);
        return newItem;
    }

    public bool CanRenameTo(string nameCandidate)
    {
        if (string.IsNullOrEmpty(value: nameCandidate?.Trim()))
        {
            return false;
        }
        if (nameCandidate.EndsWith(value: " "))
        {
            return false;
        }
        return GetGroupsOnTheSameLevel()
            .All(predicate: x => x.NodeText.ToLower().Trim() != nameCandidate.ToLower().Trim());
    }

    private IEnumerable<SchemaItemGroup> GetGroupsOnTheSameLevel()
    {
        if (ParentGroup != null)
        {
            return ParentGroup.ChildGroups.Where(predicate: x => x != this);
        }
        return PersistenceProvider
            .RetrieveList<SchemaItemGroup>()
            .Where(predicate: x => x.RootItemType == RootItemType)
            .Where(predicate: x => x.ParentGroupId == Guid.Empty)
            .Where(predicate: x => x != this);
    }

    public static string GetNextDefaultName(string defaultName, List<SchemaItemGroup> childGroups)
    {
        var defaultNamedGroupsExists = childGroups.Any(predicate: x =>
            x.NodeText.Contains(value: defaultName)
        );
        if (!defaultNamedGroupsExists)
        {
            return defaultName;
        }
        var nextGroupNumber =
            childGroups
                .Select(selector: x =>
                    Regex.Match(
                        input: x.NodeText,
                        pattern: defaultName + @"\s*(\d*)",
                        options: RegexOptions.IgnoreCase
                    )
                )
                .Where(predicate: match => match.Success)
                .Select(selector: match => match.Groups[groupnum: 1].Value)
                .Select(selector: x => x == "" ? "0" : x)
                .Select(selector: int.Parse)
                .Max() + 1;
        return $"{defaultName} {nextGroupNumber}";
    }

    public virtual SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
    {
        var group = new SchemaItemGroup(extensionId: schemaExtensionId);
        group.Name = GetNextDefaultName(defaultName: groupName, childGroups: ChildGroups);
        group.PersistenceProvider = PersistenceProvider;
        group.RootItemType = RootItemType;
        group.RootProvider = RootProvider;
        group.ParentItem = ParentItem;
        group.ParentGroup = this;
        ChildGroups.Add(item: group);
        group.Persist();
        return group;
    }
    #endregion
    #region IComparable Members
    public int CompareTo(object obj)
    {
        return obj switch
        {
            ISchemaItem item => -1,
            SchemaItemGroup group => Name.CompareTo(strB: group.Name),
            _ => throw new InvalidCastException(),
        };
    }
    #endregion
}
