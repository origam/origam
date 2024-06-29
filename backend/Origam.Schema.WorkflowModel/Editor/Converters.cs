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
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Origam.Services;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;
public class ContextStoreConverter : System.ComponentModel.TypeConverter
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
		ArrayList contextArray = new ArrayList();
		AbstractSchemaItem item = (context.Instance as AbstractSchemaItem);
		while(item != null)
		{
			// get any context stores on any of parent items
			List<ISchemaItem> contexts = item.ChildItemsByType(ContextStore.CategoryConst);
			foreach(AbstractSchemaItem store in contexts)
			{
				contextArray.Add(store);
			}
			// move up to the next item
			item = item.ParentItem;
		}
		contextArray.Add(null);
		contextArray.Sort();
		return new StandardValuesCollection(contextArray);
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
			AbstractSchemaItem item = (context.Instance as AbstractSchemaItem);
			while(item != null)
			{
				// get any context stores on any of parent items
				List<ISchemaItem> contexts = item.ChildItemsByType(ContextStore.CategoryConst);
				foreach(AbstractSchemaItem store in contexts)
				{
					if(store.Name == value.ToString())
						return store as AbstractSchemaItem;
				}
				// move up to the next item
				item = item.ParentItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ContextStoreEntityConverter : System.ComponentModel.TypeConverter
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
		return false;
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		UpdateContextTask updateContextTask = context.Instance as UpdateContextTask;
		if (updateContextTask.OutputContextStore == null || updateContextTask.OutputContextStore.Structure == null)
		{
			//return null;
			return new StandardValuesCollection(new ArrayList());
		}
		DataStructure ds  = (DataStructure) updateContextTask.OutputContextStore.Structure;
		List<DataStructureEntity> entities = ds.Entities;
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
			UpdateContextTask updateContextTask = context.Instance as UpdateContextTask;
			if (updateContextTask.OutputContextStore == null || updateContextTask.OutputContextStore.Structure == null)
			{
				return null;
			}
			DataStructure ds  = (DataStructure) updateContextTask.OutputContextStore.Structure;
			List<DataStructureEntity> entities = ds.Entities;
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
public class WorkflowCallTargetContextStoreConverter : System.ComponentModel.TypeConverter
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
		ArrayList contextArray;
		List<ISchemaItem> contexts;
		
		WorkflowCallTask task = (context.Instance as AbstractSchemaItem).ParentItem as WorkflowCallTask;
		IWorkflow wf = task.Workflow;
		if(wf == null)
		{
			return null;
		}
		else
		{
			contexts = wf.ChildItemsByType(ContextStore.CategoryConst);
		}
		contextArray = new ArrayList(contexts.Count);
		
		foreach(AbstractSchemaItem store in contexts)
		{
			contextArray.Add(store);
		}
		contextArray.Sort();
		return new StandardValuesCollection(contextArray);
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
			List<ISchemaItem> contexts;
			WorkflowCallTask task = (context.Instance as AbstractSchemaItem).ParentItem as WorkflowCallTask;
			IWorkflow wf = task.Workflow;
			if(wf == null)
			{
				return null;
			}
			else
			{
				contexts = wf.ChildItemsByType(ContextStore.CategoryConst);
			}
			foreach(AbstractSchemaItem store in contexts)
			{
				if(store.Name == value.ToString())
					return store as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineEventParameterMappingContextStoreConverter : System.ComponentModel.TypeConverter
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
		ArrayList contextArray;
		List<ISchemaItem> contexts;
		
		StateMachineEvent ev = (context.Instance as AbstractSchemaItem).ParentItem as StateMachineEvent;
		IWorkflow wf = ev.Action;
		if(wf == null)
		{
			return null;
		}
		else
		{
			contexts = wf.ChildItemsByType(ContextStore.CategoryConst);
		}
		contextArray = new ArrayList(contexts.Count);
		StateMachineEventParameterMapping mapping = context.Instance as StateMachineEventParameterMapping;
		
		foreach(IContextStore store in contexts)
		{
			if((mapping.Field != null && store.DataType == mapping.Field.DataType && mapping.Type != WorkflowEntityParameterMappingType.ChangedFlag)
				|| (store.DataType == OrigamDataType.Xml && mapping.Field == null)
				|| (mapping.Field != null && store.DataType == OrigamDataType.Boolean && mapping.Type == WorkflowEntityParameterMappingType.ChangedFlag)
				)
			{
				contextArray.Add(store);
			}
		}
		contextArray.Sort();
		return new StandardValuesCollection(contextArray);
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
			List<ISchemaItem> contexts;
			StateMachineEvent ev = (context.Instance as AbstractSchemaItem).ParentItem as StateMachineEvent;
			IWorkflow wf = ev.Action;
			if(wf == null)
			{
				return null;
			}
			else
			{
				contexts = wf.ChildItemsByType(ContextStore.CategoryConst);
			}
			foreach(AbstractSchemaItem store in contexts)
			{
				if(store.Name == value.ToString())
					return store as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineAllFieldConverter : System.ComponentModel.TypeConverter
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
		List<ISchemaItem> fields;
		ArrayList fieldArray;
		
		StateMachine sm = (context.Instance as AbstractSchemaItem).RootItem as StateMachine;
		fields = sm.Entity.EntityColumns;
		fieldArray = new ArrayList(fields.Count);
		
		foreach(AbstractSchemaItem field in fields)
		{
			fieldArray.Add(field);
		}
		fieldArray.Sort();
		return new StandardValuesCollection(fieldArray);
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
			List<ISchemaItem> fields;
		
			StateMachine sm = (context.Instance as AbstractSchemaItem).RootItem as StateMachine;
			fields = sm.Entity.EntityColumns;
			foreach(AbstractSchemaItem field in fields)
			{
				if(field.Name == value.ToString())
					return field as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkflowStepConverter : System.ComponentModel.TypeConverter
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
		ArrayList stepArray;
		List<ISchemaItem> steps;
		// Get our parent block
		ISchemaItem currentItem = (context.Instance as AbstractSchemaItem).ParentItem.ParentItem;
		
		while(! (currentItem is IWorkflowBlock || currentItem.ParentItem == null))
		{
			currentItem = currentItem.ParentItem;
		} 
		IWorkflowBlock wf = currentItem as IWorkflowBlock;
			
		if(wf == null)
		{
			return null;
		}
		else
		{
			steps = wf.ChildItemsByType(WorkflowTask.CategoryConst);
		}
		stepArray = new ArrayList(steps.Count);
		
		foreach(AbstractSchemaItem step in steps)
		{
			stepArray.Add(step);
		}
		stepArray.Sort();
		return new StandardValuesCollection(stepArray);
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
			List<ISchemaItem> tasks;
			// Get our parent block
			ISchemaItem currentItem = (context.Instance as AbstractSchemaItem).ParentItem.ParentItem;
		
			while(! (currentItem is IWorkflowBlock || currentItem.ParentItem == null))
			{
				currentItem = currentItem.ParentItem;
			} 
			IWorkflowBlock wf = currentItem as IWorkflowBlock;
			if(wf == null)
			{
				return null;
			}
			else
			{
				tasks = wf.ChildItemsByType(WorkflowTask.CategoryConst);
			}
			foreach(AbstractSchemaItem task in tasks)
			{
				if(task.Name == value.ToString())
					return task as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkflowStepFilteredConverter : System.ComponentModel.TypeConverter
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
		ArrayList stepArray;
		List<ISchemaItem> steps;
		// Get our parent block
		ISchemaItem currentItem = (context.Instance as AbstractSchemaItem).ParentItem.ParentItem;
        // Get our parent step to filter it out
        ISchemaItem parentStep = (context.Instance as AbstractSchemaItem).ParentItem;
		while(! (currentItem is IWorkflowBlock || currentItem.ParentItem == null))
		{
			currentItem = currentItem.ParentItem;
		} 
		IWorkflowBlock wf = currentItem as IWorkflowBlock;
			
		if(wf == null)
		{
			return null;
		}
		else
		{
			steps = wf.ChildItemsByType(WorkflowTask.CategoryConst);
		}
		stepArray = new ArrayList(steps.Count);
		
		foreach(AbstractSchemaItem step in steps)
		{
            if(step.Id != parentStep?.Id)
            {
                stepArray.Add(step);
            }
		}
		stepArray.Sort();
		return new StandardValuesCollection(stepArray);
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
			List<ISchemaItem> tasks;
			// Get our parent block
			ISchemaItem currentItem = (context.Instance as AbstractSchemaItem).ParentItem.ParentItem;
		
			while(! (currentItem is IWorkflowBlock || currentItem.ParentItem == null))
			{
				currentItem = currentItem.ParentItem;
			} 
			IWorkflowBlock wf = currentItem as IWorkflowBlock;
			if(wf == null)
			{
				return null;
			}
			else
			{
				tasks = wf.ChildItemsByType(WorkflowTask.CategoryConst);
			}
			foreach(AbstractSchemaItem task in tasks)
			{
				if(task.Name == value.ToString())
					return task as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ServiceConverter : System.ComponentModel.TypeConverter
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
		ServiceSchemaItemProvider services = _schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;
		ArrayList columnArray = new ArrayList(services.ChildItems.Count);
		foreach(IService service in services.ChildItems)
		{
			columnArray.Add(service);
		}
		columnArray.Sort();
		return new StandardValuesCollection(columnArray);
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
			ServiceSchemaItemProvider services = _schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;
			foreach(AbstractSchemaItem item in services.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as Service;
			}
			return null;
		}
		else if (value is IService)
		{
			return (value as IService).Name;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}

public class ServiceMethodConverter : System.ComponentModel.TypeConverter
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
		ArrayList methodArray;
		SchemaItemCollection methods;
		IService service = (context.Instance as ServiceMethodCallTask).Service as IService;
		if(service == null)
		{
			return null;
		}
		else
		{
			methods = service.ChildItems;
		}
		methodArray = new ArrayList(methods.Count);
		
		foreach(AbstractSchemaItem method in methods)
		{
			methodArray.Add(method);
		}
		methodArray.Sort();
		return new StandardValuesCollection(methodArray);
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
			SchemaItemCollection methods;
			IService service = (context.Instance as ServiceMethodCallTask).Service as IService;
			if(service == null)
			{
				return null;
			}
			else
			{
				methods = service.ChildItems;
			}
			foreach(AbstractSchemaItem method in methods)
			{
				if(method.Name == value.ToString())
					return method as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineEntityFieldConverter : System.ComponentModel.TypeConverter
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
		List<ISchemaItem> fields;
		IDataEntity entity = (context.Instance as StateMachine).Entity;
		if(entity == null)
		{
			return null;
		}
		else
		{
			fields = entity.EntityColumns;
		}
		fields.Add(null);
		fields.Sort();
		return new StandardValuesCollection(fields);
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
			List<ISchemaItem> fields;
			IDataEntity entity = (context.Instance as StateMachine).Entity;
			if(entity == null)
			{
				return null;
			}
			else
			{
				fields = entity.EntityColumns;
			}
			foreach(AbstractSchemaItem field in fields)
			{
				if(field.Name == value.ToString())
					return field as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineSubstateConverter : System.ComponentModel.TypeConverter
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
		List<ISchemaItem> states = (context.Instance as StateMachineState).ChildItemsByType(StateMachineState.CategoryConst);;
		states.Sort();
		return new StandardValuesCollection(states);
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
			List<ISchemaItem> states = (context.Instance as StateMachineState).ChildItemsByType(StateMachineState.CategoryConst);;
			foreach(AbstractSchemaItem state in states)
			{
				if(state.Name == value.ToString())
					return state as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineStateConverter : System.ComponentModel.TypeConverter
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
		ArrayList states = ((context.Instance as AbstractSchemaItem).RootItem as StateMachine).AllStates();
		states.Sort();
		return new StandardValuesCollection(states);
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
			ArrayList states = ((context.Instance as AbstractSchemaItem).RootItem as StateMachine).AllStates();
			foreach(AbstractSchemaItem item in states)
			{
				if(item.Name == value.ToString())
					return item as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineMappedFieldConverter : System.ComponentModel.TypeConverter
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
		ArrayList mappedFields = new ArrayList();
		List<ISchemaItem> fields = ((context.Instance as AbstractSchemaItem).RootItem as StateMachine).Entity.EntityColumns;
		foreach(IDataEntityColumn col in fields)
		{
			if(col is FieldMappingItem)
			{
				mappedFields.Add(col);
			}
		}
		mappedFields.Sort();
		return new StandardValuesCollection(mappedFields);
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
			ArrayList mappedFields = new ArrayList();
			List<ISchemaItem> fields = ((context.Instance as AbstractSchemaItem).RootItem as StateMachine).Entity.EntityColumns;
			foreach(IDataEntityColumn col in fields)
			{
				if(col is FieldMappingItem)
				{
					mappedFields.Add(col);
				}
			}
			foreach(AbstractSchemaItem item in mappedFields)
			{
				if(item.Name == value.ToString())
					return item as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ScheduleTimeConverter : System.ComponentModel.TypeConverter
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
		ScheduleTimeSchemaItemProvider times = _schema.GetProvider(typeof(ScheduleTimeSchemaItemProvider)) as ScheduleTimeSchemaItemProvider;
		ArrayList itemArray = new ArrayList(times.ChildItems.Count);
		foreach(AbstractScheduleTime time in times.ChildItems)
		{
			itemArray.Add(time);
		}
		itemArray.Sort();
		return new StandardValuesCollection(itemArray);
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
			ScheduleTimeSchemaItemProvider times = _schema.GetProvider(typeof(ScheduleTimeSchemaItemProvider)) as ScheduleTimeSchemaItemProvider;
			foreach(AbstractSchemaItem item in times.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as AbstractScheduleTime;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkflowConverter : System.ComponentModel.TypeConverter
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
		WorkflowSchemaItemProvider workflows = _schema.GetProvider(typeof(WorkflowSchemaItemProvider)) as WorkflowSchemaItemProvider;
		ArrayList itemArray = new ArrayList(workflows.ChildItems.Count);
		foreach(IWorkflow wf in workflows.ChildItems)
		{
			itemArray.Add(wf);
		}
		itemArray.Sort();
		return new StandardValuesCollection(itemArray);
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
			WorkflowSchemaItemProvider workflows = _schema.GetProvider(typeof(WorkflowSchemaItemProvider)) as WorkflowSchemaItemProvider;
			foreach(AbstractSchemaItem item in workflows.ChildItems)
			{
				if(item.Name == value.ToString())
					return item as IWorkflow;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class StateMachineStateLookupReaderConverter : System.ComponentModel.TypeConverter
{
	IDataLookupService _lookupManager = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
	DataView _currentList;
	IDataLookup _currentLookup;
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		//true means show a combobox
		return true;
	}
	public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	{
		//true will limit to list. false will show the list, 
		//but allow free-form entry
		return false;
	}
	private void InitList(Guid id)
	{
		_currentList = _lookupManager.GetList(id, null);
	}
	private void InitLookup(IDataLookup lookup)
	{
		if(lookup != _currentLookup)
		{
			_currentLookup = lookup;
			_currentList = null;
		}
	}
	public override System.ComponentModel.TypeConverter.StandardValuesCollection 
		GetStandardValues(ITypeDescriptorContext context)
	{
		IDataLookup lookup = ((context.Instance as StateMachineState).RootItem as StateMachine).Field.DefaultLookup;
		InitLookup(lookup);
		if(lookup == null) return null;
		InitList((Guid)lookup.PrimaryKey["Id"]);
		_currentList.Sort = lookup.ListDisplayMember;
		ArrayList list = new ArrayList(_currentList.Count);
		
		foreach(DataRowView rowview in _currentList)
		{
			list.Add(rowview.Row[lookup.ListValueMember]);
		}
		return new StandardValuesCollection(list);
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
		IDataLookup lookup = ((context.Instance as StateMachineState).RootItem as StateMachine).Field.DefaultLookup;
		if(lookup == null | value == null)
		{
			return value;
		}
		else
		{
			InitLookup(lookup);
			_currentList.Sort = lookup.ListDisplayMember;
			int i = _currentList.Find(value);
			
			if(i == -1) return null;
			return _currentList[i].Row[lookup.ListValueMember];
		}
	}
	public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
	{
		if(value == null) return null;
		if(context == null) return value.ToString();
		if(destinationType == typeof(string))
		{
			IDataLookup lookup = ((context.Instance as StateMachineState).RootItem as StateMachine).Field.DefaultLookup;
			InitLookup(lookup);
			if(lookup == null) return value.ToString();
			
			if(_currentList == null) InitList((Guid)lookup.PrimaryKey["Id"]);
			_currentList.Sort = lookup.ListValueMember;
			int i = _currentList.Find(value);
			if(i == -1) return value.ToString();
			return _currentList[i].Row[lookup.ListDisplayMember].ToString();
		}
		else
		{
			return base.ConvertTo (context, culture, value, destinationType);
		}
	}
}
public class ContextStoreRuleSetConverter : System.ComponentModel.TypeConverter
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
		ContextStore currentItem = context.Instance as ContextStore;
		if(!(currentItem.Structure is DataStructure)) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> ruleSets = (currentItem.Structure as DataStructure).RuleSets;
		ArrayList array = new ArrayList(ruleSets.Count);
		foreach(AbstractSchemaItem item in ruleSets)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			ContextStore currentItem = context.Instance as ContextStore;
			if(!(currentItem.Structure is DataStructure)) return null;
			List<ISchemaItem> ruleSets = (currentItem.Structure as DataStructure).RuleSets;
			foreach(AbstractSchemaItem item in ruleSets)
			{
				if(item.Name == value.ToString())
					return item as DataStructureRuleSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class ContextStoreDefaultSetConverter : System.ComponentModel.TypeConverter
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
		ContextStore currentItem = context.Instance as ContextStore;
		if(!(currentItem.Structure is DataStructure)) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> defaultSets = (currentItem.Structure as DataStructure).DefaultSets;
		ArrayList array = new ArrayList(defaultSets.Count);
		foreach(AbstractSchemaItem item in defaultSets)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			ContextStore currentItem = context.Instance as ContextStore;
			if(!(currentItem.Structure is DataStructure)) return null;
			List<ISchemaItem> defaultSets = (currentItem.Structure as DataStructure).DefaultSets;
			foreach(AbstractSchemaItem item in defaultSets)
			{
				if(item.Name == value.ToString())
					return item as DataStructureDefaultSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkQueueClassFilterConverter : System.ComponentModel.TypeConverter
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
		WorkQueueClass currentItem = context.Instance as WorkQueueClass;
		if(currentItem.Entity == null) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> filters = (currentItem.Entity as IDataEntity).ChildItemsByType(EntityFilter.CategoryConst);
		ArrayList array = new ArrayList(filters.Count);
		foreach(AbstractSchemaItem item in filters)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			WorkQueueClass currentItem = context.Instance as WorkQueueClass;
			if(currentItem.Entity == null) return null;
			List<ISchemaItem> filters = (currentItem.Entity as IDataEntity).ChildItemsByType(EntityFilter.CategoryConst);
			foreach(AbstractSchemaItem item in filters)
			{
				if(item.Name == value.ToString())
					return item as EntityFilter;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkQueueClassEntityStructureFilterConverter : System.ComponentModel.TypeConverter
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
		WorkQueueClass currentItem = context.Instance as WorkQueueClass;
		if(currentItem.EntityStructure == null) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> methods = currentItem.EntityStructure.Methods;
		ArrayList array = new ArrayList(methods.Count);
		foreach(AbstractSchemaItem item in methods)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			WorkQueueClass currentItem = context.Instance as WorkQueueClass;
			if(currentItem.EntityStructure == null) return null;
			List<ISchemaItem> methods = currentItem.EntityStructure.Methods;
			foreach(AbstractSchemaItem item in methods)
			{
				if(item.Name == value.ToString())
					return item as DataStructureMethod;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkQueueClassNotificationStructureFilterConverter : System.ComponentModel.TypeConverter
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
		WorkQueueClass currentItem = context.Instance as WorkQueueClass;
		if(currentItem.NotificationStructure == null) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> methods = currentItem.NotificationStructure.Methods;
		ArrayList array = new ArrayList(methods.Count);
		foreach(AbstractSchemaItem item in methods)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			WorkQueueClass currentItem = context.Instance as WorkQueueClass;
			if(currentItem.NotificationStructure == null) return null;
			List<ISchemaItem> methods = currentItem.NotificationStructure.Methods;
			foreach(AbstractSchemaItem item in methods)
			{
				if(item.Name == value.ToString())
					return item as DataStructureMethod;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class WorkQueueClassWQDataStructureFilterConverter : System.ComponentModel.TypeConverter
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
		WorkQueueClass currentItem = context.Instance as WorkQueueClass;
		if(currentItem.WorkQueueStructure == null) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> methods = currentItem.WorkQueueStructure.Methods;
		ArrayList array = new ArrayList(methods.Count);
		foreach(AbstractSchemaItem item in methods)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			WorkQueueClass currentItem = context.Instance as WorkQueueClass;
			if(currentItem.WorkQueueStructure == null) return null;
			List<ISchemaItem> methods = currentItem.WorkQueueStructure.Methods;
			foreach(AbstractSchemaItem item in methods)
			{
				if(item.Name == value.ToString())
					return item as DataStructureMethod;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}

public class WorkQueueClassWQDataStructureSortSetConverter : System.ComponentModel.TypeConverter
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
		WorkQueueClass currentItem = context.Instance as WorkQueueClass;
		if(currentItem.WorkQueueStructure == null) return new StandardValuesCollection(new ArrayList());
		List<ISchemaItem> sorts = currentItem.WorkQueueStructure.SortSets;
		ArrayList array = new ArrayList(sorts.Count);
		foreach(AbstractSchemaItem item in sorts)
		{
			array.Add(item);
		}
		array.Add(null);
		
		array.Sort();
		return new StandardValuesCollection(array);
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
			WorkQueueClass currentItem = context.Instance as WorkQueueClass;
			if(currentItem.WorkQueueStructure == null) return null;
			List<ISchemaItem> sorts = currentItem.WorkQueueStructure.SortSets;
			foreach(AbstractSchemaItem item in sorts)
			{
				if(item.Name == value.ToString())
					return item as DataStructureSortSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}

public class WorkQueueClassEntityMappingFieldConverter : System.ComponentModel.TypeConverter
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
		List<ISchemaItem> fields;
		IDataEntity entity = ((context.Instance as WorkQueueClassEntityMapping).ParentItem as WorkQueueClass).Entity;
		if(entity == null)
		{
			return null;
		}
		else
		{
			fields = entity.EntityColumns;
		}
		fields.Sort();
		return new StandardValuesCollection(fields);
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
			List<ISchemaItem> fields;
			IDataEntity entity = ((context.Instance as WorkQueueClassEntityMapping).ParentItem as WorkQueueClass).Entity;
			if(entity == null)
			{
				return null;
			}
			else
			{
				fields = entity.EntityColumns;
			}
			foreach(AbstractSchemaItem field in fields)
			{
				if(field.Name == value.ToString())
					return field as AbstractSchemaItem;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class UIFormTaskMethodConverter : System.ComponentModel.TypeConverter
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
		UIFormTask reference = context.Instance as UIFormTask;
		
		if(reference.RefreshDataStructure == null) return null;
		List<ISchemaItem> methods = reference.RefreshDataStructure.Methods;
		ArrayList methodArray = new ArrayList(methods.Count);
		foreach(DataStructureMethod method in methods)
		{
			methodArray.Add(method);
		}
		methodArray.Add(null);
		methodArray.Sort();
		return new StandardValuesCollection(methodArray);
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
			UIFormTask reference = context.Instance as UIFormTask;
		
			if(reference.RefreshDataStructure == null) return null;
			List<ISchemaItem> methods = reference.RefreshDataStructure.Methods;
			foreach(DataStructureMethod item in methods)
			{
				if(item.Name == value.ToString())
					return item as DataStructureMethod;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
public class UIFormTaskSortSetConverter : System.ComponentModel.TypeConverter
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
		UIFormTask reference = context.Instance as UIFormTask;
		
		if(reference.RefreshDataStructure == null) return null;
		List<ISchemaItem> sortSets = reference.RefreshDataStructure.SortSets;
		ArrayList sortSetArray = new ArrayList(sortSets.Count);
		foreach(DataStructureSortSet sortSet in sortSets)
		{
			sortSetArray.Add(sortSet);
		}
		sortSetArray.Add(null);
		sortSetArray.Sort();
		return new StandardValuesCollection(sortSetArray);
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
			UIFormTask reference = context.Instance as UIFormTask;
		
			if(reference.RefreshDataStructure == null) return null;
			List<ISchemaItem> sortSets = reference.RefreshDataStructure.SortSets;
			foreach(DataStructureSortSet item in sortSets)
			{
				if(item.Name == value.ToString())
					return item as DataStructureSortSet;
			}
			return null;
		}
		else
			return base.ConvertFrom(context, culture, value);
	}
}
