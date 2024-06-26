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

using System.ComponentModel;
using System.Collections;
using Origam.Services;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel;
public class ControlConverter : TypeConverter
{
    static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list, 
        //but allow free-form entry
        return true;
    }
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        UserControlSchemaItemProvider controls =
            _schema.GetProvider(typeof(UserControlSchemaItemProvider)) as UserControlSchemaItemProvider;
        ArrayList controlArray = new ArrayList(controls.ChildItems.Count);
        foreach (ControlItem control in controls.ChildItems)
        {
            if (control.PanelControlSet == null)
            {
                controlArray.Add(control);
            }
        }
        controlArray.Sort();
        return new StandardValuesCollection(controlArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            UserControlSchemaItemProvider controls =
                _schema.GetProvider(typeof(UserControlSchemaItemProvider)) as UserControlSchemaItemProvider;
            foreach (AbstractSchemaItem item in controls.ChildItems)
            {
                if (item.ToString() == value.ToString())
                    return item as ControlItem;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
    }
}

public class FormControlSetConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		FormSchemaItemProvider forms = _schema.GetProvider(typeof(FormSchemaItemProvider)) as FormSchemaItemProvider;
		ArrayList formArray = new ArrayList(forms.ChildItems.Count);
		foreach(FormControlSet form in forms.ChildItems)
		{
			formArray.Add(form);
		}
        formArray.Add(null);
		formArray.Sort();
		return new StandardValuesCollection(formArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			FormSchemaItemProvider forms = _schema.GetProvider(typeof(FormSchemaItemProvider)) as FormSchemaItemProvider;
			foreach(AbstractSchemaItem item in forms.ChildItems)
			{
				if(item.ToString() == value.ToString())
					return item as FormControlSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class PanelControlSetConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		PanelSchemaItemProvider forms = _schema.GetProvider(typeof(PanelSchemaItemProvider)) as PanelSchemaItemProvider;
		ArrayList formArray = new ArrayList(forms.ChildItems.Count);
		foreach(AbstractSchemaItem item in forms.ChildItems)
		{
			formArray.Add(item);
		}
		formArray.Add(null);
		formArray.Sort();
		return new StandardValuesCollection(formArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			PanelSchemaItemProvider forms = _schema.GetProvider(typeof(PanelSchemaItemProvider)) as PanelSchemaItemProvider;
			foreach(AbstractSchemaItem item in forms.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as PanelControlSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ReportConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		ReportSchemaItemProvider reports = _schema.GetProvider(typeof(ReportSchemaItemProvider)) as ReportSchemaItemProvider;
		
		ArrayList dsArray = new ArrayList(reports.ChildItems.Count);
		foreach(AbstractReport ds in reports.ChildItems)
		{
			dsArray.Add(ds);
		}
		dsArray.Sort();
		return new StandardValuesCollection(dsArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			ReportSchemaItemProvider reports = _schema.GetProvider(typeof(ReportSchemaItemProvider)) as ReportSchemaItemProvider;
			
			foreach(AbstractSchemaItem item in reports.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as AbstractReport;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class GraphicsConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		GraphicsSchemaItemProvider graphics = _schema.GetProvider(typeof(GraphicsSchemaItemProvider)) as GraphicsSchemaItemProvider;
		
		ArrayList dsArray = new ArrayList(graphics.ChildItems.Count);
		foreach(Graphics g in graphics.ChildItems)
		{
			dsArray.Add(g);
		}
		dsArray.Add(null);
		dsArray.Sort();
		return new StandardValuesCollection(dsArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			GraphicsSchemaItemProvider graphics = _schema.GetProvider(typeof(GraphicsSchemaItemProvider)) as GraphicsSchemaItemProvider;
			
			foreach(AbstractSchemaItem item in graphics.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as Graphics;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ChartsConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		ChartSchemaItemProvider forms = _schema.GetProvider(typeof(ChartSchemaItemProvider)) as ChartSchemaItemProvider;
		ArrayList formArray = new ArrayList(forms.ChildItems.Count);
		foreach(AbstractChart form in forms.ChildItems)
		{
			formArray.Add(form);
		}
		formArray.Sort();
		return new StandardValuesCollection(formArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			ChartSchemaItemProvider forms = _schema.GetProvider(typeof(ChartSchemaItemProvider)) as ChartSchemaItemProvider;
			foreach(AbstractSchemaItem item in forms.ChildItems)
			{
				if(item.ToString() == value.ToString())
					return item as AbstractChart;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ChartFormMappingEntityConverter : System.ComponentModel.TypeConverter
{
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		ChartFormMapping currentItem = context.Instance as ChartFormMapping;
		if(currentItem.Screen == null) return new StandardValuesCollection(new ArrayList());
		ArrayList entities = currentItem.Screen.DataStructure.Entities;
		ArrayList entityArray = new ArrayList(entities.Count);
		foreach(DataStructureEntity entity in entities)
		{
			entityArray.Add(entity);
		}
		entityArray.Sort();
		return new StandardValuesCollection(entityArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			ChartFormMapping currentItem = context.Instance as ChartFormMapping;
			if(currentItem.Screen == null) return new StandardValuesCollection(new ArrayList());
			ArrayList entities = currentItem.Screen.DataStructure.Entities;
			foreach(AbstractSchemaItem item in entities)
			{
				if(item.Name == value.ToString())
					return item as DataStructureEntity;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StylesConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
        object control = context.Instance;
        string classPath = control.GetType().FullName;
		StylesSchemaItemProvider styles = _schema.GetProvider(typeof(StylesSchemaItemProvider)) as StylesSchemaItemProvider;
		ArrayList dsArray = new ArrayList(styles.ChildItems.Count);
		foreach(UIStyle st in styles.ChildItems)
		{
            if (st.Widget.ControlType == classPath)
            {
                dsArray.Add(st);
            }
		}
		dsArray.Add(null);
		dsArray.Sort();
		return new StandardValuesCollection(dsArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			StylesSchemaItemProvider styles = _schema.GetProvider(typeof(StylesSchemaItemProvider)) as StylesSchemaItemProvider;
			
			foreach(AbstractSchemaItem item in styles.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as UIStyle;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class TreeStructureConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		TreeStructureSchemaItemProvider trees = _schema.GetProvider(typeof(TreeStructureSchemaItemProvider)) as TreeStructureSchemaItemProvider;
		
		ArrayList dsArray = new ArrayList(trees.ChildItems.Count);
		foreach(TreeStructure ts in trees.ChildItems)
		{
			dsArray.Add(ts);
		}
		dsArray.Add(null);
		dsArray.Sort();
		return new StandardValuesCollection(dsArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			TreeStructureSchemaItemProvider trees = _schema.GetProvider(typeof(TreeStructureSchemaItemProvider)) as TreeStructureSchemaItemProvider;
			
			foreach(AbstractSchemaItem item in trees.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as TreeStructure;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class KeyboardShortcutsConverter : TypeConverter
{
	static ISchemaService _schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return true;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		KeyboardShortcutsSchemaItemProvider shortcuts = 
			_schema.GetProvider(typeof(KeyboardShortcutsSchemaItemProvider)) 
			as KeyboardShortcutsSchemaItemProvider;
		
		ArrayList dsArray = new ArrayList(shortcuts.ChildItems.Count);
		foreach(KeyboardShortcut ks in shortcuts.ChildItems)
		{
			dsArray.Add(ks);
		}
		dsArray.Add(null);
		dsArray.Sort();
		return new StandardValuesCollection(dsArray);
	}
	public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
	{
		if( sourceType == typeof(string) )
			return true;
		else 
			return base.CanConvertFrom(context, sourceType);
	}
	public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
	{
		if( value.GetType() == typeof(string) )
		{
			KeyboardShortcutsSchemaItemProvider shortcuts = 
				_schema.GetProvider(typeof(KeyboardShortcutsSchemaItemProvider)) 
				as KeyboardShortcutsSchemaItemProvider;
			
			foreach(AbstractSchemaItem item in shortcuts.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as KeyboardShortcut;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ControlStylePropertyConverter : System.ComponentModel.TypeConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        //true means show a combobox
        return true;
    }
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        //true will limit to list. false will show the list, 
        //but allow free-form entry
        return true;
    }
    public override System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
    {
        ArrayList styleProperties = 
            ((context.Instance as UIStyleProperty).ParentItem as UIStyle)
            .Widget.ChildItemsByType(ControlStyleProperty.CategoryConst);
        ArrayList propertyArray = new ArrayList(styleProperties.Count);
        foreach (ControlStyleProperty property in styleProperties)
        {
            propertyArray.Add(property);
        }
        propertyArray.Sort();
        return new StandardValuesCollection(propertyArray);
    }
    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
        if (sourceType == typeof(string))
            return true;
        else
            return base.CanConvertFrom(context, sourceType);
    }
    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
        if (value.GetType() == typeof(string))
        {
            ArrayList styleProperties =
                ((context.Instance as UIStyleProperty).ParentItem as UIStyle)
                .Widget.ChildItemsByType(ControlStyleProperty.CategoryConst);
            foreach (AbstractSchemaItem item in styleProperties)
            {
                if (item.Name == value.ToString())
                    return item as ControlStyleProperty;
            }
            return null;
        }
        else
            return base.ConvertFrom(context, culture, value);
    }
}
