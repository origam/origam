#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

using Origam.UI;

namespace Origam.Schema
{
	/// <summary>
	/// Summary description for NonpersistentSchemaItemNode.
	/// </summary>
	public class NonpersistentSchemaItemNode : IBrowserNode2, ISchemaItemFactory, IComparable
	{
		public NonpersistentSchemaItemNode()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region IBrowserNode2 Members

		public bool CanMove(IBrowserNode2 newNode)
		{
			// TODO:  Add NonpersistentSchemaItemNode.CanMove implementation
			return false;
		}

		private IBrowserNode2 _parentNode;
		public IBrowserNode2 ParentNode
		{
			get
			{
				return _parentNode;
			}
			set
			{
				_parentNode = value;
			}
		}

		public bool CanDelete
		{
			get
			{
				return false;
			}
		}

		public void Delete()
		{
			throw new InvalidOperationException(ResourceUtils.GetString("ErrorDeleteIndividual"));
		}

		public bool Hide
		{
			get
			{
				// TODO:  Add NonpersistentSchemaItemNode.Hide getter implementation
				return false;
			}
			set
			{
				// TODO:  Add NonpersistentSchemaItemNode.Hide setter implementation
			}
		}

		public byte[] NodeImage
		{
			get
			{
				return null;
			}
		}
		#endregion

		#region IBrowserNode Members

		public bool HasChildNodes
		{
			get
			{
				return this.ChildNodes().Count > 0;
			}
		}

		public bool CanRename
		{
			get
			{
				return false;
			}
		}

		public BrowserNodeCollection ChildNodes()
		{
			BrowserNodeCollection result = new BrowserNodeCollection();

			SchemaItemAncestor ancestor = this.ParentNode as SchemaItemAncestor;
			AbstractSchemaItem parent;
			if(ancestor == null)
			{
				parent = this.ParentNode as AbstractSchemaItem;
			}
			else
			{
				parent = ancestor.Ancestor;
			}

			if(this.NodeText == "_Ancestors")
			{
				if(parent != null)
				{
					// All ancestors
					foreach(IBrowserNode nod in parent.AllAncestors)
					{
						result.Add(nod);
					}
				}
			}
			else
			{
				// All other child items
				foreach(AbstractSchemaItem item in parent.ChildItems)
				{
                    // and only own (not derived) items, they will be returned by SchemaItemAncestor
                    if (item.DerivedFrom == null & item.IsDeleted == false)
                    {
                        if (parent.UseFolders)
                        {
                            string description = SchemaItemDescription(item.GetType());
                            if (description == null) description = item.ItemType;

                            if (this.NodeText == description)
                            {
                                result.Add(item);
                            }
                        }
                    }
				}
			}

			return result;
		}

		public string NodeId
		{
			get
			{
				return this.ParentNode.NodeId;
			}
		}

		private string _nodeText = "";
		public string NodeText
		{
			get
			{
				return _nodeText;
			}
			set
			{
				_nodeText = value;
			}
		}

		private string _icon = "";
		public string Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				_icon = value;
			}
		}

        public virtual string FontStyle
        {
            get
            {
                return "Regular";
            }
        }
        #endregion

		#region ISchemaItemFactory Members

		public AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem newItem;

			if(this.ParentNode is ISchemaItemFactory)
			{
				newItem = (this.ParentNode as ISchemaItemFactory).NewItem(type, schemaExtensionId, group);
			}
			else
			{
				throw new Exception(ResourceUtils.GetString("ErrorUnknownParent"));
			}

			return newItem;
		}

		public SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
		{
			// TODO:  Add NonpersistentSchemaItemNode.NewGroup implementation
			return null;
		}

		public Type[] NewItemTypes
		{
			get
			{
				ISchemaItemFactory parent = this.ParentNode as ISchemaItemFactory;
				if(parent != null)
				{
					ArrayList types = new ArrayList();

					foreach(Type type in parent.NewItemTypes)
					{
						string description = SchemaItemDescription(type);
						if(description == this.NodeText)
						{
							types.Add(type);
						}
					}

					return types.ToArray(typeof(Type)) as Type[];
				}
				else
				{
					return new Type[] {};
				}
			}
		}

		public virtual string[] NewTypeNames
		{
			get
			{
				ISchemaItemFactory parent = this.ParentNode as ISchemaItemFactory;
				if(parent != null)
				{
					return parent.NewTypeNames;
				}
				else
				{
					return new string[] {};
				}
			}
		}

		public virtual Type[] NameableTypes
		{
			get
			{
				return NewItemTypes;
			}
		}
		#endregion

		private string SchemaItemDescription(Type type)
		{
			SchemaItemDescriptionAttribute attr = SchemaItemDescriptionAtt(type);
            if (attr == null)
            {
                return null;
            }
            else
            {
                return attr.FolderName;
            }
		}

		private SchemaItemDescriptionAttribute SchemaItemDescriptionAtt(Type type)
		{
			object[] attributes = type.GetCustomAttributes(typeof(SchemaItemDescriptionAttribute), true);

			if(attributes != null && attributes.Length > 0)
				return attributes[0] as SchemaItemDescriptionAttribute;
			else
				return null;

		}
		#region IComparable Members
		public int CompareTo(object obj)
		{
			NonpersistentSchemaItemNode npsin = obj as NonpersistentSchemaItemNode;
			IBrowserNode other = obj as IBrowserNode;

			if(other == null)
			{
				return -1;
			}
			else if(npsin != null)
			{
				return this.NodeText.CompareTo(npsin.NodeText);
			}
			else
			{
				throw new InvalidCastException();
			}
		}
		#endregion
	}
}
