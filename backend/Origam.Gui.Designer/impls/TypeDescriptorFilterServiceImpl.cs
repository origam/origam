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

public class TypeDescriptorFilterServiceImpl : ITypeDescriptorFilterService
{
    private IDesignerHost host;

    public TypeDescriptorFilterServiceImpl(IDesignerHost host)
    {
        this.host = host;
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
        if (!(component is Control))
        {
            return false;
        }

        Control control = component as Control;
        ControlSetItem ctrlSet = control.Tag as ControlSetItem;
        if (ctrlSet == null)
        {
            properties.Clear();
        }
        else
        {
            Hashtable finalprops = new Hashtable();
            // Load all properties we want to see
            foreach (
                var propItem in ctrlSet.ControlItem.ChildItemsByType<ControlPropertyItem>(
                    itemType: ControlPropertyItem.CategoryConst
                )
            )
            {
                finalprops.Add(key: propItem.Name, value: propItem.Name);
            }
            AddInheritedProperties(component: component, finalProps: finalprops);
            if (!finalprops.ContainsKey(key: "Size"))
            {
                finalprops.Add(key: "Size", value: "Size");
            }

            if (!finalprops.ContainsKey(key: "Location"))
            {
                finalprops.Add(key: "Location", value: "Location");
            }

            if (!finalprops.ContainsKey(key: "DataSource"))
            {
                finalprops.Add(key: "DataSource", value: "DataSource");
            }

            if (!finalprops.ContainsKey(key: "TextDetached"))
            {
                finalprops.Add(key: "TextDetached", value: "TextDetached");
            }

            if (!finalprops.ContainsKey(key: "DataBindings"))
            {
                finalprops.Add(key: "DataBindings", value: "DataBindings");
            }

            if (!finalprops.ContainsKey(key: "CrystalReport"))
            {
                finalprops.Add(key: "CrystalReport", value: "CrystalReport");
            }

            if (!finalprops.ContainsKey(key: "ParameterMappings"))
            {
                finalprops.Add(key: "ParameterMappings", value: "ParameterMappings");
            }

            if (!finalprops.ContainsKey(key: "Panel"))
            {
                finalprops.Add(key: "Panel", value: "Panel");
            }

            if (!finalprops.ContainsKey(key: "DataLookup"))
            {
                finalprops.Add(key: "DataLookup", value: "DataLookup");
            }

            if (!finalprops.ContainsKey(key: "SchemaItemName"))
            {
                finalprops.Add(key: "SchemaItemName", value: "SchemaItemName");
            }

            if (!finalprops.ContainsKey(key: "SchemaItemId"))
            {
                finalprops.Add(key: "SchemaItemId", value: "SchemaItemId");
            }

            if (!finalprops.ContainsKey(key: "Roles"))
            {
                finalprops.Add(key: "Roles", value: "Roles");
            }

            if (!finalprops.ContainsKey(key: "Features"))
            {
                finalprops.Add(key: "Features", value: "Features");
            }

            if (!finalprops.ContainsKey(key: "TitleIcon"))
            {
                finalprops.Add(key: "TitleIcon", value: "TitleIcon");
            }

            if (!finalprops.ContainsKey(key: "Workflow"))
            {
                finalprops.Add(key: "Workflow", value: "Workflow");
            }

            if (!finalprops.ContainsKey(key: "Icon"))
            {
                finalprops.Add(key: "Icon", value: "Icon");
            }

            if (!finalprops.ContainsKey(key: "ThumbnailWidthConstant"))
            {
                finalprops.Add(key: "ThumbnailWidthConstant", value: "ThumbnailWidthConstant");
            }

            if (!finalprops.ContainsKey(key: "ThumbnailHeightConstant"))
            {
                finalprops.Add(key: "ThumbnailHeightConstant", value: "ThumbnailHeightConstant");
            }

            if (!finalprops.ContainsKey(key: "BlobLookup"))
            {
                finalprops.Add(key: "BlobLookup", value: "BlobLookup");
            }

            if (!finalprops.ContainsKey(key: "StorageTypeDefaultConstant"))
            {
                finalprops.Add(
                    key: "StorageTypeDefaultConstant",
                    value: "StorageTypeDefaultConstant"
                );
            }

            if (!finalprops.ContainsKey(key: "DefaultCompressionConstant"))
            {
                finalprops.Add(
                    key: "DefaultCompressionConstant",
                    value: "DefaultCompressionConstant"
                );
            }

            if (!finalprops.ContainsKey(key: "PipelineStateLookup"))
            {
                finalprops.Add(key: "PipelineStateLookup", value: "PipelineStateLookup");
            }

            if (!finalprops.ContainsKey(key: "IndependentDataSource"))
            {
                finalprops.Add(key: "IndependentDataSource", value: "IndependentDataSource");
            }

            if (!finalprops.ContainsKey(key: "IndependentDataSourceFilter"))
            {
                finalprops.Add(
                    key: "IndependentDataSourceFilter",
                    value: "IndependentDataSourceFilter7"
                );
            }

            if (!finalprops.ContainsKey(key: "IndependentDataSourceSort"))
            {
                finalprops.Add(
                    key: "IndependentDataSourceSort",
                    value: "IndependentDataSourceSort"
                );
            }

            if (!finalprops.ContainsKey(key: "ComponentBindings"))
            {
                finalprops.Add(key: "ComponentBindings", value: "ComponentBindings");
            }

            if (!finalprops.ContainsKey(key: "Style"))
            {
                finalprops.Add(key: "Style", value: "Style");
            }

            if (!finalprops.ContainsKey(key: "Tree"))
            {
                finalprops.Add(key: "Tree", value: "Tree");
            }

            if (!finalprops.ContainsKey(key: "ValueConstant"))
            {
                finalprops.Add(key: "ValueConstant", value: "ValueConstant");
            }

            if (!finalprops.ContainsKey(key: "CalendarRowHeight"))
            {
                finalprops.Add(key: "CalendarRowHeight", value: "CalendarRowHeight");
            }

            if (!finalprops.ContainsKey(key: "MappingCondition"))
            {
                finalprops.Add(key: "MappingCondition", value: "MappingCondition");
            }

            if (!finalprops.ContainsKey(key: "RequestSaveAfterChange"))
            {
                finalprops.Add(key: "RequestSaveAfterChange", value: "RequestSaveAfterChange");
            }

            if (!finalprops.ContainsKey(key: "CustomNumberFormat"))
            {
                finalprops.Add(key: "CustomNumberFormat", value: "CustomNumberFormat");
            }
            // needed for C1.TextBox
            if (!finalprops.ContainsKey(key: "VisualStyle"))
            {
                finalprops.Add(key: "VisualStyle", value: "VisualStyle");
            }

            if (!finalprops.ContainsKey(key: "CalendarViewStyle"))
            {
                finalprops.Add(key: "CalendarViewStyle", value: "CalendarViewStyle");
            }

            var keys = new List<object>(capacity: properties.Count);
            // Sometimes keys are not exactly the member names, they are renamed (e.g. Feature_118 instead of just Feature.
            // Therefore we must dig into the member definition to get the real name.
            foreach (DictionaryEntry entry in properties)
            {
                MemberDescriptor md = entry.Value as MemberDescriptor;
                if (md == null)
                {
                    keys.Add(item: entry.Key);
                }
                else
                {
                    keys.Add(item: md.Name);
                }
            }
            foreach (object key in keys)
            {
                if (!finalprops.ContainsKey(key: key))
                {
                    properties.Remove(key: key);
                }
            }
        }

        return false;
    }

    private static void AddInheritedProperties(IComponent component, Hashtable finalProps)
    {
        ControlItem inheritorItem = DynamicTypeFactory.GetAssociatedControlItem(
            maybeDynamicType: component.GetType()
        );
        if (inheritorItem == null)
        {
            return;
        }

        foreach (
            var propItem in inheritorItem.ChildItemsByType<ControlPropertyItem>(
                itemType: ControlPropertyItem.CategoryConst
            )
        )
        {
            finalProps[key: propItem.Name] = propItem.Name;
        }
    }
    #endregion
}
