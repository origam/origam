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
using System.Collections.Generic;
using Origam.DA;

namespace Origam.Schema
{
	/// <summary>
	/// Summary description for AncestorItem.
	/// </summary>
	[XmlModelRoot("ancestor")]
    [ClassMetaVersion("6.0.0")]
	public class SchemaItemAncestor : AbstractPersistent, IBrowserNode2, 
        ICloneable, IComparable, IFilePersistent
	{
		public SchemaItemAncestor()
		{
			this.PrimaryKey = new ModelElementKey();
		}

		public SchemaItemAncestor(Key primaryKey) : base(primaryKey, new ModelElementKey().KeyArray)	{}

		private AbstractSchemaItem _schemaItem;
		[Browsable(false)]
		public AbstractSchemaItem SchemaItem
		{
			get
			{
				return _schemaItem;
			}
			set
			{
				_schemaItem = value;
			}
		}

		private Guid _ancestorId;

		[Browsable(false)]
		public Guid AncestorId
		{
			get
			{
				return _ancestorId;
			}
			set
			{
				_ancestorId = value;
			}
		}

		[TypeConverter(typeof(AncestorItemConverter))]
        [XmlReference("ancestor", "AncestorId")]
		public AbstractSchemaItem Ancestor
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.AncestorId;

				try
				{
					return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
				}
				catch
				{
					return null;
				}
			}
			set
			{
				this.AncestorId = (Guid)value.PrimaryKey["Id"];
			}
		}

		#region IBrowserNode2 Members
		[Browsable(false)] 
		public bool Hide
		{
			get
			{
				return !this.IsPersisted;
			}
			set
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorSetHide"));
			}
		}

		[Browsable(false)]
		public bool CanDelete
		{
			get
			{
				return false;
			}
		}

		public void Delete()
		{
			this.IsDeleted = true;
			this.Ancestor.AllAncestors.Remove(this);
		}

		public bool CanMove(IBrowserNode2 newNode)
		{
			return false;
		}

		[Browsable(false)]
		public IBrowserNode2 ParentNode
		{
			get
			{
				return null;
			}
			set
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErorrMoveAncestor"));
			}
		}

		[Browsable(false)] 
		public byte[] NodeImage
		{
			get
			{
				return null;
			}
		}

		[Browsable(false)] 
		public string NodeId
		{
			get
			{
				return this.PrimaryKey["Id"].ToString();
			}
		}

        [Browsable(false)]
        public virtual string FontStyle
        {
            get
            {
                return "Regular";
            }
        }
        #endregion

		#region IBrowserNode Members

		[Browsable(false)]
		public bool HasChildNodes
		{
			get
			{
				return this.ChildNodes().Count > 0;
			}
		}

		[Browsable(false)]
		public bool CanRename
		{
			get
			{
				return false;
			}
		}

		public BrowserNodeCollection ChildNodes()
		{
			BrowserNodeCollection col = new BrowserNodeCollection();
			Hashtable folders = new Hashtable();

//			// All groups
//			foreach(SchemaItemGroup group in this.SchemaItem.ChildGroups)
//				col.Add(group);

			// All child items derived by this ancestor
			foreach(AbstractSchemaItem item in this.SchemaItem.ChildItems)
			{
				if(item.DerivedFrom != null && item.DerivedFrom.PrimaryKey.Equals(this.Ancestor.PrimaryKey) & item.IsDeleted == false)
				{
					if(this.Ancestor.UseFolders)
					{
						SchemaItemDescriptionAttribute attr = item.GetType().SchemaItemDescription();
						string description = attr == null ? item.ItemType : attr.FolderName;
						if(description == null) description = item.ItemType;

						if(! folders.Contains(description))
						{
							NonpersistentSchemaItemNode folder = new NonpersistentSchemaItemNode();
							folder.ParentNode = this;
							folder.NodeText = description;
							col.Add(folder);
							folders.Add(description, folder);
						}
					}
					else
					{
						col.Add(item);
					}
				}
			}

			return col;		
		}

		[Browsable(false)]
		public string NodeText
		{
			get
			{
				return this.Ancestor.Name;
			}
			set
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorRenameAncestor"));
			}
		}

		[Browsable(false)]
		public string NodeToolTipText
		{
			get
			{
				// TODO:  Add SchemaItemAncestor.NodeToolTipText getter implementation
				return null;
			}
		}

		[Browsable(false)]
		public string Icon
		{
			get
			{
				return "3";
			}
		}

        public string RelativeFilePath
        {
            get
            {
                return SchemaItem?.RelativeFilePath ?? "";
            }
        }

        public Guid FileParentId
        {
            get => SchemaItem?.Id ?? Guid.Empty;
            set { }
	    }

	    public bool IsFolder
        {
            get
            {
                return false;
            }
        }

        public IDictionary<string, Guid> ParentFolderIds =>
	        new Dictionary<string, Guid>
	        {
		        {
			        CategoryFactory.Create(typeof(Package)),
			        SchemaItem.SchemaExtensionId
				},
				{
					CategoryFactory.Create(typeof(SchemaItemGroup)),
					SchemaItem.GroupId
				}
	        };

		public string Path
        {
            get
            {
                return "";
            }
        }

		public bool IsFileRootElement => FileParentId == Guid.Empty;

		public override string ToString()
		{
			if(this.Ancestor != null)
			{
				return this.Ancestor.Name;
			}
			else
			{
				return "Unspecified";
			}
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			SchemaItemAncestor newItem = new SchemaItemAncestor();

			newItem._ancestorId = this._ancestorId;
			newItem.PersistenceProvider = this.PersistenceProvider;
			
			return newItem;
		}

		#endregion

		#region IComparable Members
		public int CompareTo(object obj)
		{
			SchemaItemAncestor anc = obj as SchemaItemAncestor;
			IBrowserNode other = obj as IBrowserNode;

			if(other != null)
			{
				return -1;
			}
			else if(anc != null)
			{
				return this.NodeText.CompareTo(anc.NodeText);
			}
			else
			{
				throw new InvalidCastException();
			}
		}
        #endregion
    }
}
