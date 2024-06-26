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
using System.ComponentModel.Design;

namespace Origam.Schema;
/// <summary>
/// Summary description for SchemaItemAncestorCollectionEditor.
/// </summary>
public class SchemaItemAncestorCollectionEditor : System.ComponentModel.Design.CollectionEditor
{
	public SchemaItemAncestorCollectionEditor(Type type) : base(type)
	{
	}
	protected override object CreateInstance(Type itemType)
	{	
		if(itemType != typeof(SchemaItemAncestor))
			throw new ArgumentOutOfRangeException("itemType", itemType, ResourceUtils.GetString("ErrorSchemaItemAncestorOnly"));
		AbstractSchemaItem parentItem = (this.Context.Instance as AbstractSchemaItem);
		
		SchemaItemAncestor ancestor = new SchemaItemAncestor();
		ancestor.PersistenceProvider = parentItem.PersistenceProvider;
		ancestor.SchemaItem = parentItem;
		return ancestor;
	}
	protected override void DestroyInstance(object instance)
	{
		if(! (instance is SchemaItemAncestor))
			throw new ArgumentOutOfRangeException("instance", instance, ResourceUtils.GetString("ErrorSchemaItemAncestorOnly"));
		if(!(instance as SchemaItemAncestor).IsPersisted)
			throw new Exception(ResourceUtils.GetString("ErrorSchemaItemAncestorPersistOnly"));
		(instance as SchemaItemAncestor).IsDeleted = true;
		(instance as SchemaItemAncestor).Persist();
		//base.DestroyInstance (instance);
	}
	protected override object[] GetItems(object editValue)
	{
		// we have to filter out all deleted items
	
		object[] result = new object[(editValue as SchemaItemAncestorCollection).Count];
		int i = 0;
		foreach(SchemaItemAncestor item in (editValue as SchemaItemAncestorCollection))
		{
			if(!item.IsDeleted) result[i] = item;
			
			i++;
		}
		return result;
	}
	public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		IDesignerHost des = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;
		object win = provider.GetService(typeof(System.Windows.Forms.Design.IWindowsFormsEditorService)) as System.Windows.Forms.Design.IWindowsFormsEditorService;
		System.Windows.Forms.PropertyGrid grid = win.GetType().GetProperty("Parent").GetValue(win, null) as System.Windows.Forms.PropertyGrid;
		Origam.UI.IViewContent form = grid.Parent as Origam.UI.IViewContent;
		form.IsDirty = true;
		return base.EditValue (context, provider, value);
	}
}
