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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Origam.Schema.GuiModel;

namespace Origam.Gui.Designer;
public class TypeDescriptorFilterServiceImpl:ITypeDescriptorFilterService
{
	private IDesignerHost host;
	public TypeDescriptorFilterServiceImpl(IDesignerHost host)
	{
		this.host=host;
	}
#region ITypeDescriptorFilterService Members
	public bool FilterAttributes(IComponent component, IDictionary attributes)
	{
//			Hashtable finalprops = new Hashtable();
//
//			finalprops.Add("Name",attributes["Name"]);
//			finalprops.Add("Left",attributes["Left"]);
//			finalprops.Add("Top",attributes["Top"]);
//			finalprops.Add("Width",attributes["Width"]);
//			finalprops.Add("Height",attributes["Height"]);
//
//			IDesigner designer = host.GetDesigner(component);
//			if (designer is IDesignerFilter)
//			{
//				IDesignerFilter designerFilter = designer as IDesignerFilter;
//				designerFilter.PreFilterAttributes(finalprops);
//				designerFilter.PostFilterAttributes(finalprops);
//				return true;
//			}
//			return false;
		return false;
	}

	public bool FilterEvents(IComponent component, IDictionary events)
	{
//			IDesigner designer = host.GetDesigner(component);
//			if (designer is IDesignerFilter)
//			{
//				IDesignerFilter designerFilter = designer as IDesignerFilter;
//				designerFilter.PreFilterEvents(events);
//				designerFilter.PostFilterEvents(events);
//				return true;
//			}
//			return false;
		return false;
	}

	public bool FilterProperties(IComponent component, IDictionary properties)
	{
		if(!(component is Control))
        {
            return false;
        }

        Control control = component as Control;
		ControlSetItem ctrlSet = control.Tag as ControlSetItem;
		if(ctrlSet == null)
		{
			properties.Clear();
		}
		else
		{
			Hashtable finalprops = new Hashtable();
			// Load all properties we want to see
			foreach(var propItem in ctrlSet.ControlItem.ChildItemsByType<ControlPropertyItem>(ControlPropertyItem.CategoryConst))
			{
				finalprops.Add(propItem.Name, propItem.Name);
			}
			AddInheritedProperties(component, finalprops);
			if(!finalprops.ContainsKey("Size"))
            {
                finalprops.Add("Size", "Size");
            }

            if (!finalprops.ContainsKey("Location"))
            {
                finalprops.Add("Location", "Location");
            }

            if (!finalprops.ContainsKey("DataSource"))
            {
                finalprops.Add("DataSource", "DataSource");
            }

            if (!finalprops.ContainsKey("TextDetached"))
            {
                finalprops.Add("TextDetached", "TextDetached");
            }

            if (!finalprops.ContainsKey("DataBindings"))
            {
                finalprops.Add("DataBindings", "DataBindings");
            }

            if (!finalprops.ContainsKey("CrystalReport"))
            {
                finalprops.Add("CrystalReport", "CrystalReport");
            }

            if (!finalprops.ContainsKey("ParameterMappings"))
            {
                finalprops.Add("ParameterMappings", "ParameterMappings");
            }

            if (!finalprops.ContainsKey("Panel"))
            {
                finalprops.Add("Panel", "Panel");
            }

            if (!finalprops.ContainsKey("DataLookup"))
            {
                finalprops.Add("DataLookup", "DataLookup");
            }

            if (!finalprops.ContainsKey("SchemaItemName"))
            {
                finalprops.Add("SchemaItemName", "SchemaItemName");
            }

            if (!finalprops.ContainsKey("SchemaItemId"))
            {
                finalprops.Add("SchemaItemId", "SchemaItemId");
            }

            if (!finalprops.ContainsKey("Roles"))
            {
                finalprops.Add("Roles", "Roles");
            }

            if (!finalprops.ContainsKey("Features"))
            {
                finalprops.Add("Features", "Features");
            }

            if (!finalprops.ContainsKey("TitleIcon"))
            {
                finalprops.Add("TitleIcon", "TitleIcon");
            }

            if (!finalprops.ContainsKey("Workflow"))
            {
                finalprops.Add("Workflow", "Workflow");
            }

            if (!finalprops.ContainsKey("Icon"))
            {
                finalprops.Add("Icon", "Icon");
            }

            if (!finalprops.ContainsKey("ThumbnailWidthConstant"))
            {
                finalprops.Add("ThumbnailWidthConstant", "ThumbnailWidthConstant");
            }

            if (!finalprops.ContainsKey("ThumbnailHeightConstant"))
            {
                finalprops.Add("ThumbnailHeightConstant", "ThumbnailHeightConstant");
            }

            if (!finalprops.ContainsKey("BlobLookup"))
            {
                finalprops.Add("BlobLookup", "BlobLookup");
            }

            if (!finalprops.ContainsKey("StorageTypeDefaultConstant"))
            {
                finalprops.Add("StorageTypeDefaultConstant", "StorageTypeDefaultConstant");
            }

            if (!finalprops.ContainsKey("DefaultCompressionConstant"))
            {
                finalprops.Add("DefaultCompressionConstant", "DefaultCompressionConstant");
            }

            if (!finalprops.ContainsKey("PipelineStateLookup"))
            {
                finalprops.Add("PipelineStateLookup", "PipelineStateLookup");
            }

            if (!finalprops.ContainsKey("IndependentDataSource"))
            {
                finalprops.Add("IndependentDataSource", "IndependentDataSource");
            }

            if (!finalprops.ContainsKey("IndependentDataSourceFilter"))
            {
                finalprops.Add("IndependentDataSourceFilter", "IndependentDataSourceFilter7");
            }

            if (!finalprops.ContainsKey("IndependentDataSourceSort"))
            {
                finalprops.Add("IndependentDataSourceSort", "IndependentDataSourceSort");
            }

            if (!finalprops.ContainsKey("ComponentBindings"))
            {
                finalprops.Add("ComponentBindings", "ComponentBindings");
            }

            if (!finalprops.ContainsKey("Style"))
            {
                finalprops.Add("Style", "Style");
            }

            if (!finalprops.ContainsKey("Tree"))
            {
                finalprops.Add("Tree", "Tree");
            }

            if (!finalprops.ContainsKey("ValueConstant"))
            {
                finalprops.Add("ValueConstant", "ValueConstant");
            }

            if (!finalprops.ContainsKey("CalendarRowHeight"))
            {
                finalprops.Add("CalendarRowHeight", "CalendarRowHeight");
            }

            if (!finalprops.ContainsKey("MappingCondition"))
            {
                finalprops.Add("MappingCondition", "MappingCondition");
            }

            if (!finalprops.ContainsKey("RequestSaveAfterChange"))
            {
                finalprops.Add("RequestSaveAfterChange", "RequestSaveAfterChange");
            }

            if (!finalprops.ContainsKey("CustomNumberFormat"))
            {
                finalprops.Add("CustomNumberFormat", "CustomNumberFormat");
            }
            // needed for C1.TextBox
            if (!finalprops.ContainsKey("VisualStyle"))
            {
                finalprops.Add("VisualStyle", "VisualStyle");
            }

            if (!finalprops.ContainsKey("CalendarViewStyle"))
            {
                finalprops.Add("CalendarViewStyle", "CalendarViewStyle");
            }

            var keys = new List<object>(properties.Count);
			// Sometimes keys are not exactly the member names, they are renamed (e.g. Feature_118 instead of just Feature.
			// Therefore we must dig into the member definition to get the real name.
			foreach(DictionaryEntry entry in properties)
			{
				MemberDescriptor md = entry.Value as MemberDescriptor;
				if(md == null)
				{
					keys.Add(entry.Key);
				}
				else
				{
					keys.Add(md.Name);
				}
			}
			foreach(object key in keys)
			{
				if(! finalprops.ContainsKey(key))
				{
					properties.Remove(key);
				}
			}
		}
		
		return false;
	}
	private static void AddInheritedProperties(IComponent component,
		Hashtable finalProps)
	{
		ControlItem inheritorItem = DynamicTypeFactory
			.GetAssociatedControlItem(component.GetType());
		if (inheritorItem == null)
        {
            return;
        }

        foreach (var propItem in inheritorItem.ChildItemsByType<ControlPropertyItem>(
			ControlPropertyItem.CategoryConst))
		{
			finalProps[propItem.Name] = propItem.Name;
		}
	}
	#endregion
}
