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
using Origam.UI;

namespace Origam.Schema;
public class NonpersistentSchemaItemNode : IBrowserNode2, ISchemaItemFactory
{
	public NonpersistentSchemaItemNode() {}
	#region IBrowserNode2 Members
	public bool CanMove(IBrowserNode2 newNode) =>
		// TODO:  Add NonpersistentSchemaItemNode.CanMove implementation
		false;
	private IBrowserNode2 _parentNode;
	public IBrowserNode2 ParentNode
	{
		get => _parentNode;
		set => _parentNode = value;
	}
	public bool CanDelete => false;
	public void Delete()
	{
		throw new InvalidOperationException(
			ResourceUtils.GetString("ErrorDeleteIndividual"));
	}
	public bool Hide
	{
		get =>
			// TODO:  Add NonpersistentSchemaItemNode.Hide getter implementation
			false;
		set
		{
			// TODO:  Add NonpersistentSchemaItemNode.Hide setter implementation
		}
	}
	public byte[] NodeImage => null;
	#endregion
	#region IBrowserNode Members
	public bool HasChildNodes => this.ChildNodes().Count > 0;
	public bool CanRename => false;
	public BrowserNodeCollection ChildNodes()
	{
		var result = new BrowserNodeCollection();
		ISchemaItem parent;
		if(ParentNode is not SchemaItemAncestor ancestor)
		{
			parent = ParentNode as ISchemaItem;
		}
		else
		{
			parent = ancestor.Ancestor;
		}
		if(NodeText == "_Ancestors")
		{
			if(parent != null)
			{
				// All ancestors
				foreach(IBrowserNode node in parent.AllAncestors)
				{
					result.Add(node);
				}
			}
		}
		else
		{
			// All other child items
			foreach(var item in parent.ChildItems)
			{
                // and only own (not derived) items, they will be returned by SchemaItemAncestor
                if(item.DerivedFrom == null & item.IsDeleted == false)
                {
                    if(parent.UseFolders)
                    {
                        var description 
                            = SchemaItemDescription(item.GetType()) 
                              ?? item.ItemType;
                        if (NodeText == description)
                        {
                            result.Add(item);
                        }
                    }
                }
			}
		}
		return result;
	}
	public string NodeId => ParentNode.NodeId;
	private string _nodeText = "";
	public string NodeText
	{
		get => _nodeText;
		set => _nodeText = value;
	}
	public string Icon => "38_folder-categories-1.png";
	public virtual string FontStyle => "Regular";
	#endregion
	#region ISchemaItemFactory Members
	public virtual T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group) 
		where T : class, ISchemaItem
	{
		T newItem;
		if(ParentNode is ISchemaItemFactory)
		{
			newItem = (ParentNode as ISchemaItemFactory).NewItem<T>(
				schemaExtensionId, group);
		}
		else
		{
			throw new Exception(
				ResourceUtils.GetString("ErrorUnknownParent"));
		}
		ItemCreated?.Invoke(newItem);
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
			if(ParentNode is not ISchemaItemFactory parent)
			{
				return new Type[] { };
			}
			var types = new List<Type>();
			foreach(var type in parent.NewItemTypes)
			{
				var description = SchemaItemDescription(type);
				if(description == NodeText)
				{
					types.Add(type);
				}
			}
			return types.ToArray();
		}
	}
	public virtual IList<string> NewTypeNames
	{
		get
		{
			if(ParentNode is ISchemaItemFactory parent)
			{
				return parent.NewTypeNames;
			}
			return new List<string>();
		}
	}
	public virtual Type[] NameableTypes => NewItemTypes;
	public event Action<ISchemaItem> ItemCreated;
	#endregion
	private string SchemaItemDescription(Type type)
	{
		var schemaItemDescriptionAttribute = type.SchemaItemDescription();
		return schemaItemDescriptionAttribute?.FolderName;
	}
	#region IComparable Members
	public int CompareTo(object obj)
	{
		if(obj is not IBrowserNode other)
		{
			return -1;
		}
		if(obj is NonpersistentSchemaItemNode nonpersistentSchemaItemNode)
		{
			return NodeText.CompareTo(nonpersistentSchemaItemNode.NodeText);
		}
		throw new InvalidCastException();
	}
    #endregion
}
