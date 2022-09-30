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

namespace Origam.Schema
{
	/// <summary>
	/// Summary description for SchemaItemGroup.
	/// </summary>
	[XmlModelRoot("group")]
    [ClassMetaVersion("6.0.0")]
	[XmlNamespaceName("g")]
    public class SchemaItemGroup : AbstractPersistent, IBrowserNode2, 
        ISchemaItemFactory, ISchemaItemProvider, IComparable, IFilePersistent
	{
		public SchemaItemGroup()
		{
			this.PrimaryKey = new ModelElementKey();
		}

		public SchemaItemGroup(Guid extensionId) : this()
		{
			this.SchemaExtensionId = extensionId;
		}

		public SchemaItemGroup(Key primaryKey) : base(primaryKey, primaryKey.KeyArray)
		{
		}

		public override string ToString() => this.Name;

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
				ModelElementKey key = new ModelElementKey();
				key.Id = this.SchemaExtensionId;

				return (Package)this.PersistenceProvider.RetrieveInstance(typeof(Package), key);
			}
			set => this.SchemaExtensionId = (Guid)value.PrimaryKey["Id"];
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
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ParentGroupId;

				return (SchemaItemGroup)this.PersistenceProvider.RetrieveInstance(typeof(SchemaItemGroup), key);
			}
			set
			{
				if(value == null)
				{
					this.ParentGroupId = Guid.Empty;
				}
				else
				{
					this.ParentGroupId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}
		
		[Browsable(false)]
		public string Path => GetPath(this);

		public bool IsFileRootElement => FileParentId == Guid.Empty;
		#endregion

		private string GetPath(SchemaItemGroup item)
		{
			if(this.ParentGroup == null)
			{
				return this.Name;
			}
			else
			{
				return this.ParentGroup.Path + "/" + this.Name;
			}
		}

		[Browsable(false)]
		public SchemaItemGroup RootGroup => this.GetRootGroup(this);

		private SchemaItemGroup GetRootGroup(SchemaItemGroup parentItem)
		{
			if(parentItem.ParentGroup == null)
				return parentItem;
			else
				return GetRootGroup(parentItem.ParentGroup);
		}
		
		private ArrayList GetChildItemsRecursive(AbstractSchemaItem parentItem)
		{
			ArrayList items = new ArrayList();

			foreach(AbstractSchemaItem childItem in parentItem.ChildItems)
			{
				items.Add(childItem);
				items.AddRange(GetChildItemsRecursive(childItem));
			}

			return items;
		}

		public AbstractSchemaItem GetChildByName(string name, string itemType)
		{
			foreach(AbstractSchemaItem item in this.ChildItems)
			{
				if(item.Name == name & item.ItemType == itemType)
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
			get => !this.IsPersisted;
			set
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorSetHide"));
			}
		}

		public bool CanDelete => true;

		public void Delete()
		{
			if(this.ParentItem != null)
				if(this.ParentItem.DerivedFrom != null)
					throw new InvalidOperationException(ResourceUtils.GetString("ErrorDeleteDerivedGroup"));

			foreach(IBrowserNode2 nod in this.ChildNodes())
				nod.Delete();

			this.IsDeleted = true;
			this.Persist();
		}

		public bool CanMove(IBrowserNode2 newNode) => false;

		[Browsable(false)]
		public IBrowserNode2 ParentNode
		{
			get => null;
			set
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorMoveGroup"));
			}
		}

		public byte[] NodeImage => null;

		[Browsable(false)] 
		public string NodeId => this.PrimaryKey["Id"].ToString();

		[Browsable(false)]
        public virtual string FontStyle => "Regular";
		#endregion

		#region IBrowserNode Members

		public bool HasChildNodes => this.ChildNodes().Count > 0;

		public bool CanRename => true;

		public BrowserNodeCollection ChildNodes()
		{
			BrowserNodeCollection col = new BrowserNodeCollection();

			// Child groups
			foreach(IBrowserNode2 nod in this.ChildGroups)
				col.Add(nod);

			// Child nodes
			foreach(IBrowserNode2 nod in this.ChildItems)
			{
				if(!(nod as ISchemaItem).IsDeleted)
				{
					col.Add(nod);
				}
			}

			return col;
		}

		public string NodeText
		{
			get => this.Name;
			set
			{
				this.Name = value;
				this.Persist();
			}
		}

		public string NodeToolTipText => null;

		public string Icon => "37_folder-3.png";
		#endregion

		#region ISchemaItemProvider Members

		public SchemaItemCollection ChildItems
		{
			get
			{
				// We look for all child items of our parent schema item that have this group
				// We browse the collection because it has all the items correctly set
				ISchemaItemProvider provider;
				if(this.ParentItem != null)
					provider = this.ParentItem;
				else
				{
					provider = this.RootProvider;
				}

				SchemaItemCollection col = new SchemaItemCollection(this.PersistenceProvider, provider, this.ParentItem);

				foreach(AbstractSchemaItem item in provider.ChildItemsByGroup(this))
				{
					col.Add(item);
				}

				return col;
			}
		}

		public ArrayList ChildItemsByType(string itemType)
		{
			ArrayList list = new ArrayList();

			// We look for all child items of our parent schema item that have this group
			// We browse the collection because it has all the items correctly set
			foreach(AbstractSchemaItem item in this.ParentItem.ChildItemsByGroup(this))
			{
				list.Add(item);
			}

			return list;
		}

		public ArrayList ChildItemsByGroup(SchemaItemGroup group)
		{
			ArrayList list = new ArrayList();

			foreach(AbstractSchemaItem item in this.ChildItems)
			{
				if((item.Group == null && group == null) || item.Group.PrimaryKey.Equals(group.PrimaryKey))
					list.Add(item);
			}

			return list;
		}

		public SchemaItemGroup GetGroup(string name)
		{
			foreach(SchemaItemGroup group in this.ChildGroups)
			{
				if(group.Name == name) return group;
			}

			return null;
		}

		public bool HasChildItems => this.ChildItems.Count > 0;

		public bool HasChildItemsByType(string itemType) => this.ChildItemsByType(itemType).Count > 0;

		public bool HasChildItemsByGroup(SchemaItemGroup group) => this.ChildItemsByGroup(group).Count > 0;

		public List<SchemaItemGroup> ChildGroups
		{
			get
			{
				// We retrive all child groups
				List<SchemaItemGroup> list = this.PersistenceProvider.RetrieveListByGroup<SchemaItemGroup>( PrimaryKey);

				// Set parent for each child
				foreach(SchemaItemGroup group in list)
				{
					group.RootProvider = this.RootProvider;
					group.ParentItem = this.ParentItem;
					group.ParentGroup = this;
				}

				return list;
			}
		}

		public ISchemaItemProvider RootProvider { get; set; } = null;

		public ArrayList ChildItemsRecursive
		{
			get
			{
				ArrayList items = new ArrayList();

				foreach(AbstractSchemaItem item in this.ChildItems)
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

		public ArrayList ChildItemTypes => new ArrayList(this.NewItemTypes);

		[Browsable(false)]
		public Type[] NewItemTypes
		{
			get
			{
				if(this.ParentItem == null)
					return RootProvider?.NewItemTypes ?? new Type[0];
				else
					return (this.ParentItem as ISchemaItemFactory).NewItemTypes;
			}
		}

		[Browsable(false)]
		public virtual IList<string> NewTypeNames
		{
			get
			{
				if(this.ParentItem == null)
					return this.RootProvider.NewTypeNames;
				else
					return (this.ParentItem as ISchemaItemFactory).NewTypeNames;
			}
		}

		/// <summary>
		/// By default all NewItemTypes are nameable. Override if only a subset of types can
		/// be populated with NewTypeNames.
		/// </summary>
		[Browsable(false)]
		public virtual Type[] NameableTypes => NewItemTypes;

		public event Action<ISchemaItem> ItemCreated;

		public string RelativeFilePath => Package.Name + "\\" + RootItemType + "\\" + Path.Replace("/", "\\") + "\\"+PersistenceFiles.GroupFileName;

		public bool IsFolder => true;

		public Guid FileParentId
		{
			get => Guid.Empty;
			set { }
		}

		public IDictionary<string, Guid> ParentFolderIds
        {
            get
            {
	            return 
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
            }
        }

        public AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem newItem;

			if(this.ParentItem == null)
				newItem = this.RootProvider.NewItem(type, schemaExtensionId, this);
			else
				newItem = (this.ParentItem as ISchemaItemFactory).NewItem(type, schemaExtensionId, this);
			ItemCreated?.Invoke(newItem);
			return newItem;
		}

		public bool CanRenameTo(string nameCandidate)
		{
			if (string.IsNullOrEmpty(nameCandidate?.Trim())) return false;
			if (nameCandidate.EndsWith(" ")) return false;
			return GetGroupsOnTheSameLevel()
				.All(x => x.NodeText.ToLower().Trim() != nameCandidate.ToLower().Trim());
		}

		private IEnumerable<SchemaItemGroup> GetGroupsOnTheSameLevel()
		{
			if (ParentGroup != null)
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
			bool defaultNamedGroupsExist= childGroups
				.Any(x => x.NodeText.Contains(defaultName));
			if (!defaultNamedGroupsExist) return defaultName;
			
			var nextGroupNumber = childGroups
			  .Select(x => Regex.Match(x.NodeText, defaultName + @"\s*(\d*)",RegexOptions.IgnoreCase))
			  .Where(match => match.Success)
			  .Select(match => match.Groups[1].Value)
			  .Select(x => x=="" ? "0" : x)
			  .Select(int.Parse)
			  .Max() + 1;
			return $"{defaultName} {nextGroupNumber}";
		}

		public virtual SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
		{	
			SchemaItemGroup group = new SchemaItemGroup(schemaExtensionId);
			group.Name = GetNextDefaultName(groupName, ChildGroups);
			group.PersistenceProvider = this.PersistenceProvider;
			group.RootItemType = this.RootItemType;
			group.RootProvider = this.RootProvider;
			group.ParentItem = this.ParentItem;
			group.ParentGroup = this;
			this.ChildGroups.Add(group);
			group.Persist();

			return group;
		}

		#endregion

		#region IComparable Members
		public int CompareTo(object obj)
		{
			ISchemaItem item = obj as ISchemaItem;
			SchemaItemGroup group = obj as SchemaItemGroup;

			if(item != null)
			{
				return -1;
			}
			else if(group != null)
			{
				return this.Name.CompareTo(group.Name);
			}
			else
			{
				throw new InvalidCastException();
			}
		}
		#endregion
	}
}
