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

using Origam.DA.Common;
using System;
using System.Collections;
using System.ComponentModel;
using Origam.UI;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Origam.DA;
using Origam.DA.Common.ObjectPersistence.Attributes;
using Origam.Schema.ItemCollection;
using InvalidCastException = System.InvalidCastException;

namespace Origam.Schema;
[XmlModelRoot("group")]
[ClassMetaVersion("6.0.0")]
[XmlNamespaceName("g")]
public class SchemaItemGroup : AbstractPersistent, ISchemaItemProvider, 
    IFilePersistent
{
	public SchemaItemGroup()
	{
		PrimaryKey = new ModelElementKey();
	}
	public SchemaItemGroup(Guid extensionId) : this()
	{
		SchemaExtensionId = extensionId;
	}
	public SchemaItemGroup(Key primaryKey) 
		: base(primaryKey, primaryKey.KeyArray)
	{
	}
	public override string ToString() => Name;
	#region Properties
	[XmlAttribute(AttributeName = "rootItemType")] 
	public string RootItemType { get; set; }
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    [XmlParent(typeof(Package))]
    public Guid SchemaExtensionId;
    [Browsable(false)]
	public Package Package
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = SchemaExtensionId
			};
			return (Package)PersistenceProvider.RetrieveInstance(
				typeof(Package), key);
		}
		set => SchemaExtensionId = (Guid)value.PrimaryKey["Id"];
    }
	[Browsable(false)]
	public AbstractSchemaItem ParentItem { get; set; }
    [XmlParent(typeof(SchemaItemGroup))]
    public Guid ParentGroupId;
	[Browsable(false)]
	public SchemaItemGroup ParentGroup
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = ParentGroupId
			};
			return (SchemaItemGroup)PersistenceProvider.RetrieveInstance(
				typeof(SchemaItemGroup), key);
		}
		set
		{
			if(value == null)
			{
				ParentGroupId = Guid.Empty;
			}
			else
			{
				ParentGroupId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	
	[Browsable(false)]
	public string Path => GetPath(this);
	public bool IsFileRootElement => FileParentId == Guid.Empty;
	#endregion
	private string GetPath(SchemaItemGroup item)
	{
		if(ParentGroup == null)
		{
			return Name;
		}
		return ParentGroup.Path + "/" + Name;
	}
	[Browsable(false)]
	public SchemaItemGroup RootGroup => GetRootGroup(this);
	private SchemaItemGroup GetRootGroup(SchemaItemGroup parentItem)
	{
		return parentItem.ParentGroup == null 
			? parentItem : GetRootGroup(parentItem.ParentGroup);
	}
	
	private List<ISchemaItem> GetChildItemsRecursive(AbstractSchemaItem parentItem)
	{
		var items = new List<ISchemaItem>();
		foreach(var childItem in parentItem.ChildItems)
		{
			items.Add(childItem);
			items.AddRange(GetChildItemsRecursive(childItem));
		}
		return items;
	}
	public AbstractSchemaItem GetChildByName(string name, string itemType)
	{
		foreach(var item in ChildItems)
		{
			if((item.Name == name) && (item.ItemType == itemType))
			{
				return item;
			}
		}
		return null;
	}
	
	#region IBrowserNode2 Members
	[Browsable(false)] 
	public bool Hide
	{
		get => !IsPersisted;
		set => throw new InvalidOperationException(
			ResourceUtils.GetString("ErrorSetHide"));
	}
	public bool CanDelete => true;
	public void Delete()
	{
		if(ParentItem?.DerivedFrom != null)
		{
			throw new InvalidOperationException(
				ResourceUtils.GetString("ErrorDeleteDerivedGroup"));
		}
		foreach(IBrowserNode2 node in ChildNodes())
		{
			node.Delete();
		}
		IsDeleted = true;
		Persist();
	}
	public bool CanMove(IBrowserNode2 newNode) => false;
	[Browsable(false)]
	public IBrowserNode2 ParentNode
	{
		get => null;
		set => throw new InvalidOperationException(
			ResourceUtils.GetString("ErrorMoveGroup"));
	}
	public byte[] NodeImage => null;
	[Browsable(false)] 
	public string NodeId => PrimaryKey["Id"].ToString();
	[Browsable(false)]
    public virtual string FontStyle => "Regular";
	#endregion
	#region IBrowserNode Members
	public bool HasChildNodes => this.ChildNodes().Count > 0;
	public bool CanRename => true;
	public BrowserNodeCollection ChildNodes()
	{
		var browserNodeCollection = new BrowserNodeCollection();
		// Child groups
		foreach(IBrowserNode2 node in ChildGroups)
		{
			browserNodeCollection.Add(node);
		}
		// Child nodes
		foreach(IBrowserNode2 node in this.ChildItems)
		{
			if(!((ISchemaItem)node).IsDeleted)
			{
				browserNodeCollection.Add(node);
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
				PersistenceProvider, provider, ParentItem);
			foreach(AbstractSchemaItem item 
			        in provider.ChildItemsByGroup(this))
			{
				ISchemaItemCollection.Add(item);
			}
			return ISchemaItemCollection;
		}
	}
	public List<ISchemaItem> ChildItemsByType(string itemType)
	{
		var list = new List<ISchemaItem>();
		// We look for all child items of our parent schema item that have this group
		// We browse the collection because it has all the items correctly set
		foreach(AbstractSchemaItem item in ParentItem.ChildItemsByGroup(this))
		{
			list.Add(item);
		}
		return list;
	}
	public ArrayList ChildItemsByGroup(SchemaItemGroup group)
	{
		var list = new ArrayList();
		foreach(var item in ChildItems)
		{
			if((item.Group == null && group == null) ||
		    item.Group.PrimaryKey.Equals(group.PrimaryKey))
			{
				list.Add(item);
			}
		}
		return list;
	}
	public SchemaItemGroup GetGroup(string name)
	{
		return ChildGroups.FirstOrDefault(group => group.Name == name);
	}
	public bool HasChildItems => ChildItems.Count > 0;
	public bool HasChildItemsByType(string itemType) 
		=> ChildItemsByType(itemType).Count > 0;
	public bool HasChildItemsByGroup(SchemaItemGroup group) 
		=> ChildItemsByGroup(group).Count > 0;
	public List<SchemaItemGroup> ChildGroups
	{
		get
		{
			// We retrieve all child groups
			var list = PersistenceProvider
				.RetrieveListByGroup<SchemaItemGroup>(PrimaryKey);
			// Set parent for each child
			foreach(var group in list)
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
                };
            }
        }
    }
    public ISchemaItemProvider RootProvider { get; set; } = null;
	public List<ISchemaItem> ChildItemsRecursive
	{
		get
		{
			var items = new List<ISchemaItem>();
			foreach(var item in ChildItems)
			{
				items.Add(item);
				items.AddRange(GetChildItemsRecursive(item));
			}
			return items;
		}
	}
	public bool AutoCreateFolder => false;
	public void ClearCache()
    {
    }
	#endregion
	#region ISchemaItemFactory Members
	public ArrayList ChildItemTypes => new(NewItemTypes);
	[Browsable(false)]
	public Type[] NewItemTypes
	{
		get
		{
			if(ParentItem == null)
			{
				return RootProvider?.NewItemTypes ?? new Type[0];
			}
			return (ParentItem as ISchemaItemFactory).NewItemTypes;
		}
	}
	[Browsable(false)]
	public virtual IList<string> NewTypeNames =>
		ParentItem == null 
			? RootProvider.NewTypeNames 
			: (ParentItem as ISchemaItemFactory).NewTypeNames;
	/// <summary>
	/// By default all NewItemTypes are nameable. Override if only a subset of types can
	/// be populated with NewTypeNames.
	/// </summary>
	[Browsable(false)]
	public virtual Type[] NameableTypes => NewItemTypes;
	public event Action<ISchemaItem> ItemCreated;
	public string RelativeFilePath 
		=> Package.Name + "\\" + RootItemType + "\\" 
		   + Path.Replace("/", "\\") + "\\" 
		   + PersistenceFiles.GroupFileName;
	public bool IsFolder => true;
	public Guid FileParentId
	{
		get => Guid.Empty;
		set { }
	}
	public IDictionary<string, Guid> ParentFolderIds =>
		new Dictionary<string, Guid>
		{
			{
				CategoryFactory.Create(typeof(Package)),
				SchemaExtensionId
			},
			{
				CategoryFactory.Create(typeof(SchemaItemGroup)),
				ParentGroupId
			}
		};
	public virtual T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group) 
		where T : AbstractSchemaItem
	{
		T newItem;
		if(ParentItem == null)
		{
			newItem = RootProvider.NewItem<T>(schemaExtensionId, this);
		}
		else
		{
			newItem = (ParentItem as ISchemaItemFactory).NewItem<T>(
				schemaExtensionId, this);
		}
		ItemCreated?.Invoke(newItem);
		return newItem;
	}
	public bool CanRenameTo(string nameCandidate)
	{
		if(string.IsNullOrEmpty(nameCandidate?.Trim()))
		{
			return false;
		}
		if(nameCandidate.EndsWith(" "))
		{
			return false;
		}
		return GetGroupsOnTheSameLevel()
			.All(x => x.NodeText.ToLower().Trim() 
			          != nameCandidate.ToLower().Trim());
	}
	private IEnumerable<SchemaItemGroup> GetGroupsOnTheSameLevel()
	{
		if(ParentGroup != null)
		{
			return ParentGroup.ChildGroups
				.Where(x => x != this);	
		}
		return PersistenceProvider
			.RetrieveList<SchemaItemGroup>()
			.Where(x => x.RootItemType == RootItemType)
			.Where(x => x.ParentGroupId == Guid.Empty)
			.Where(x => x != this);
	}
	public static string GetNextDefaultName(string defaultName,
		List<SchemaItemGroup> childGroups)
	{
		var defaultNamedGroupsExists= childGroups
			.Any(x => x.NodeText.Contains(defaultName));
		if(!defaultNamedGroupsExists)
		{
			return defaultName;
		}
		var nextGroupNumber = childGroups
		  .Select(x => Regex.Match(x.NodeText, 
			  defaultName + @"\s*(\d*)",RegexOptions.IgnoreCase))
		  .Where(match => match.Success)
		  .Select(match => match.Groups[1].Value)
		  .Select(x => x=="" ? "0" : x)
		  .Select(int.Parse)
		  .Max() + 1;
		return $"{defaultName} {nextGroupNumber}";
	}
	public virtual SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
	{	
		var group = new SchemaItemGroup(schemaExtensionId);
		group.Name = GetNextDefaultName(groupName, ChildGroups);
		group.PersistenceProvider = PersistenceProvider;
		group.RootItemType = RootItemType;
		group.RootProvider = RootProvider;
		group.ParentItem = ParentItem;
		group.ParentGroup = this;
		ChildGroups.Add(group);
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
			SchemaItemGroup group => Name.CompareTo(group.Name),
			_ => throw new InvalidCastException()
		};
	}
	#endregion
}
