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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;
using Origam.Services;
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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        var contextArray = new List<ContextStore>();
        ISchemaItem item = context.Instance as ISchemaItem;
        while (item != null)
        {
            // get any context stores on any of parent items
            List<ContextStore> contexts = item.ChildItemsByType<ContextStore>(
                ContextStore.CategoryConst
            );
            foreach (ContextStore store in contexts)
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

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItem item = (context.Instance as ISchemaItem);
            while (item != null)
            {
                // get any context stores on any of parent items
                List<ContextStore> contexts = item.ChildItemsByType<ContextStore>(
                    ContextStore.CategoryConst
                );
                foreach (ContextStore store in contexts)
                {
                    if (store.Name == value.ToString())
                    {
                        return store;
                    }
                }
                // move up to the next item
                item = item.ParentItem;
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        UpdateContextTask updateContextTask = context.Instance as UpdateContextTask;
        if (
            updateContextTask.OutputContextStore == null
            || updateContextTask.OutputContextStore.Structure == null
        )
        {
            //return null;
            return new StandardValuesCollection(new List<DataStructureEntity>());
        }
        DataStructure ds = (DataStructure)updateContextTask.OutputContextStore.Structure;
        List<DataStructureEntity> entities = ds.Entities;
        var entityArray = new List<DataStructureEntity>(entities.Count);
        foreach (DataStructureEntity entity in entities)
        {
            entityArray.Add(entity);
        }
        entityArray.Sort();
        return new StandardValuesCollection(entityArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            UpdateContextTask updateContextTask = context.Instance as UpdateContextTask;
            if (
                updateContextTask.OutputContextStore == null
                || updateContextTask.OutputContextStore.Structure == null
            )
            {
                return null;
            }
            DataStructure ds = (DataStructure)updateContextTask.OutputContextStore.Structure;
            List<DataStructureEntity> entities = ds.Entities;
            foreach (ISchemaItem item in entities)
            {
                if (item.Name == value.ToString())
                {
                    return item as DataStructureEntity;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<ContextStore> contexts;

        WorkflowCallTask task = (context.Instance as ISchemaItem).ParentItem as WorkflowCallTask;
        IWorkflow wf = task.Workflow;
        if (wf == null)
        {
            return null;
        }

        contexts = wf.ChildItemsByType<ContextStore>(ContextStore.CategoryConst);
        var contextArray = new List<ContextStore>(contexts.Count);

        foreach (ContextStore store in contexts)
        {
            contextArray.Add(store);
        }
        contextArray.Sort();
        return new StandardValuesCollection(contextArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<ContextStore> contexts;
            WorkflowCallTask task =
                (context.Instance as ISchemaItem).ParentItem as WorkflowCallTask;
            IWorkflow wf = task.Workflow;
            if (wf == null)
            {
                return null;
            }

            contexts = wf.ChildItemsByType<ContextStore>(ContextStore.CategoryConst);
            foreach (ContextStore store in contexts)
            {
                if (store.Name == value.ToString())
                {
                    return store;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class StateMachineEventParameterMappingContextStoreConverter
    : System.ComponentModel.TypeConverter
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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<ContextStore> contexts;

        StateMachineEvent ev = (context.Instance as ISchemaItem).ParentItem as StateMachineEvent;
        IWorkflow wf = ev.Action;
        if (wf == null)
        {
            return null;
        }

        contexts = wf.ChildItemsByType<ContextStore>(ContextStore.CategoryConst);
        var contextArray = new List<IContextStore>(contexts.Count);
        StateMachineEventParameterMapping mapping =
            context.Instance as StateMachineEventParameterMapping;

        foreach (IContextStore store in contexts)
        {
            if (
                (
                    mapping.Field != null
                    && store.DataType == mapping.Field.DataType
                    && mapping.Type != WorkflowEntityParameterMappingType.ChangedFlag
                )
                || (store.DataType == OrigamDataType.Xml && mapping.Field == null)
                || (
                    mapping.Field != null
                    && store.DataType == OrigamDataType.Boolean
                    && mapping.Type == WorkflowEntityParameterMappingType.ChangedFlag
                )
            )
            {
                contextArray.Add(store);
            }
        }
        contextArray.Sort();
        return new StandardValuesCollection(contextArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<ContextStore> contexts;
            StateMachineEvent ev =
                (context.Instance as ISchemaItem).ParentItem as StateMachineEvent;
            IWorkflow wf = ev.Action;
            if (wf == null)
            {
                return null;
            }

            contexts = wf.ChildItemsByType<ContextStore>(ContextStore.CategoryConst);
            foreach (ContextStore store in contexts)
            {
                if (store.Name == value.ToString())
                {
                    return store;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<IDataEntityColumn> fields;
        List<IDataEntityColumn> fieldArray;

        StateMachine sm = (context.Instance as ISchemaItem).RootItem as StateMachine;
        fields = sm.Entity.EntityColumns;
        fieldArray = new List<IDataEntityColumn>(fields.Count);

        foreach (IDataEntityColumn field in fields)
        {
            fieldArray.Add(field);
        }
        fieldArray.Sort();
        return new StandardValuesCollection(fieldArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<IDataEntityColumn> fields;

            StateMachine sm = (context.Instance as ISchemaItem).RootItem as StateMachine;
            fields = sm.Entity.EntityColumns;
            foreach (IDataEntityColumn field in fields)
            {
                if (field.Name == value.ToString())
                {
                    return field;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<WorkflowTask> stepArray;
        List<WorkflowTask> steps;
        // Get our parent block
        ISchemaItem currentItem = (context.Instance as ISchemaItem).ParentItem.ParentItem;

        while (!(currentItem is IWorkflowBlock || currentItem.ParentItem == null))
        {
            currentItem = currentItem.ParentItem;
        }
        IWorkflowBlock wf = currentItem as IWorkflowBlock;

        if (wf == null)
        {
            return null;
        }

        steps = wf.ChildItemsByType<WorkflowTask>(WorkflowTask.CategoryConst);
        stepArray = new List<WorkflowTask>(steps.Count);

        foreach (WorkflowTask step in steps)
        {
            stepArray.Add(step);
        }
        stepArray.Sort();
        return new StandardValuesCollection(stepArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<WorkflowTask> tasks;
            // Get our parent block
            ISchemaItem currentItem = (context.Instance as ISchemaItem).ParentItem.ParentItem;

            while (!(currentItem is IWorkflowBlock || currentItem.ParentItem == null))
            {
                currentItem = currentItem.ParentItem;
            }
            IWorkflowBlock wf = currentItem as IWorkflowBlock;
            if (wf == null)
            {
                return null;
            }

            tasks = wf.ChildItemsByType<WorkflowTask>(WorkflowTask.CategoryConst);
            foreach (WorkflowTask task in tasks)
            {
                if (task.Name == value.ToString())
                {
                    return task;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<IWorkflowStep> steps;
        // Get our parent block
        ISchemaItem currentItem = (context.Instance as ISchemaItem).ParentItem.ParentItem;
        // Get our parent step to filter it out
        ISchemaItem parentStep = (context.Instance as ISchemaItem).ParentItem;
        while (!(currentItem is IWorkflowBlock || currentItem.ParentItem == null))
        {
            currentItem = currentItem.ParentItem;
        }
        IWorkflowBlock wf = currentItem as IWorkflowBlock;

        if (wf == null)
        {
            return null;
        }

        steps = wf.ChildItemsByType<IWorkflowStep>(AbstractWorkflowStep.CategoryConst);
        var stepArray = new List<IWorkflowStep>(steps.Count);

        foreach (IWorkflowStep step in steps)
        {
            if (step.Id != parentStep?.Id)
            {
                stepArray.Add(step);
            }
        }
        stepArray.Sort();
        return new StandardValuesCollection(stepArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<WorkflowTask> tasks;
            // Get our parent block
            ISchemaItem currentItem = (context.Instance as ISchemaItem).ParentItem.ParentItem;

            while (!(currentItem is IWorkflowBlock || currentItem.ParentItem == null))
            {
                currentItem = currentItem.ParentItem;
            }
            IWorkflowBlock wf = currentItem as IWorkflowBlock;
            if (wf == null)
            {
                return null;
            }

            tasks = wf.ChildItemsByType<WorkflowTask>(WorkflowTask.CategoryConst);
            foreach (WorkflowTask task in tasks)
            {
                if (task.Name == value.ToString())
                {
                    return task;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class ServiceConverter : System.ComponentModel.TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ServiceSchemaItemProvider services =
            _schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;
        var columnArray = new List<IService>(services.ChildItems.Count);
        foreach (IService service in services.ChildItems)
        {
            columnArray.Add(service);
        }
        columnArray.Sort();
        return new StandardValuesCollection(columnArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ServiceSchemaItemProvider services =
                _schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;
            foreach (ISchemaItem item in services.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as Service;
                }
            }
            return null;
        }

        if (value is IService)
        {
            return (value as IService).Name;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ISchemaItemCollection methods;
        IService service = (context.Instance as ServiceMethodCallTask).Service;
        if (service == null)
        {
            return null;
        }

        methods = service.ChildItems;
        var methodArray = new List<ISchemaItem>(methods.Count);

        foreach (ISchemaItem method in methods)
        {
            methodArray.Add(method);
        }
        methodArray.Sort();
        return new StandardValuesCollection(methodArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ISchemaItemCollection methods;
            IService service = (context.Instance as ServiceMethodCallTask).Service as IService;
            if (service == null)
            {
                return null;
            }

            methods = service.ChildItems;
            foreach (ISchemaItem method in methods)
            {
                if (method.Name == value.ToString())
                {
                    return method;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<IDataEntityColumn> fields;
        IDataEntity entity = (context.Instance as StateMachine).Entity;
        if (entity == null)
        {
            return null;
        }

        fields = entity.EntityColumns;
        fields.Add(null);
        fields.Sort();
        return new StandardValuesCollection(fields);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<IDataEntityColumn> fields;
            IDataEntity entity = (context.Instance as StateMachine).Entity;
            if (entity == null)
            {
                return null;
            }

            fields = entity.EntityColumns;
            foreach (IDataEntityColumn field in fields)
            {
                if (field.Name == value.ToString())
                {
                    return field;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<StateMachineState> states = (
            context.Instance as StateMachineState
        ).ChildItemsByType<StateMachineState>(StateMachineState.CategoryConst);
        ;
        states.Sort();
        return new StandardValuesCollection(states);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<StateMachineState> states = (
                context.Instance as StateMachineState
            ).ChildItemsByType<StateMachineState>(StateMachineState.CategoryConst);
            ;
            foreach (StateMachineState state in states)
            {
                if (state.Name == value.ToString())
                {
                    return state;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<StateMachineState> states = (
            (context.Instance as ISchemaItem).RootItem as StateMachine
        ).AllStates();
        states.Sort();
        return new StandardValuesCollection(states);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<StateMachineState> states = (
                (context.Instance as ISchemaItem).RootItem as StateMachine
            ).AllStates();
            foreach (ISchemaItem item in states)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        var mappedFields = new List<FieldMappingItem>();
        List<IDataEntityColumn> fields = (
            (context.Instance as ISchemaItem).RootItem as StateMachine
        )
            .Entity
            .EntityColumns;
        foreach (IDataEntityColumn col in fields)
        {
            if (col is FieldMappingItem item)
            {
                mappedFields.Add(item);
            }
        }
        mappedFields.Sort();
        return new StandardValuesCollection(mappedFields);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            var mappedFields = new List<IDataEntityColumn>();
            List<IDataEntityColumn> fields = (
                (context.Instance as AbstractSchemaItem).RootItem as StateMachine
            )
                .Entity
                .EntityColumns;
            foreach (IDataEntityColumn col in fields)
            {
                if (col is FieldMappingItem)
                {
                    mappedFields.Add(col);
                }
            }
            foreach (IDataEntityColumn item in mappedFields)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class ScheduleTimeConverter : System.ComponentModel.TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ScheduleTimeSchemaItemProvider times =
            _schema.GetProvider(typeof(ScheduleTimeSchemaItemProvider))
            as ScheduleTimeSchemaItemProvider;
        var itemArray = new List<AbstractScheduleTime>(times.ChildItems.Count);
        foreach (AbstractScheduleTime time in times.ChildItems)
        {
            itemArray.Add(time);
        }
        itemArray.Sort();
        return new StandardValuesCollection(itemArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ScheduleTimeSchemaItemProvider times =
                _schema.GetProvider(typeof(ScheduleTimeSchemaItemProvider))
                as ScheduleTimeSchemaItemProvider;
            foreach (ISchemaItem item in times.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as AbstractScheduleTime;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class WorkflowConverter : System.ComponentModel.TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        WorkflowSchemaItemProvider workflows =
            _schema.GetProvider(typeof(WorkflowSchemaItemProvider)) as WorkflowSchemaItemProvider;
        var itemArray = new List<IWorkflow>(workflows.ChildItems.Count);
        foreach (IWorkflow wf in workflows.ChildItems)
        {
            itemArray.Add(wf);
        }
        itemArray.Sort();
        return new StandardValuesCollection(itemArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            WorkflowSchemaItemProvider workflows =
                _schema.GetProvider(typeof(WorkflowSchemaItemProvider))
                as WorkflowSchemaItemProvider;
            foreach (ISchemaItem item in workflows.ChildItems)
            {
                if (item.Name == value.ToString())
                {
                    return item as IWorkflow;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class StateMachineStateLookupReaderConverter : System.ComponentModel.TypeConverter
{
    IDataLookupService _lookupManager =
        ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
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
        if (lookup != _currentLookup)
        {
            _currentLookup = lookup;
            _currentList = null;
        }
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        IDataLookup lookup = ((context.Instance as StateMachineState).RootItem as StateMachine)
            .Field
            .DefaultLookup;
        InitLookup(lookup);
        if (lookup == null)
        {
            return null;
        }

        InitList((Guid)lookup.PrimaryKey["Id"]);
        _currentList.Sort = lookup.ListDisplayMember;
        var list = new List<object>(_currentList.Count);

        foreach (DataRowView rowview in _currentList)
        {
            list.Add(rowview.Row[lookup.ListValueMember]);
        }
        return new StandardValuesCollection(list);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        IDataLookup lookup = ((context.Instance as StateMachineState).RootItem as StateMachine)
            .Field
            .DefaultLookup;
        if (lookup == null | value == null)
        {
            return value;
        }
        InitLookup(lookup);
        _currentList.Sort = lookup.ListDisplayMember;
        int i = _currentList.Find(value);

        if (i == -1)
        {
            return null;
        }

        return _currentList[i].Row[lookup.ListValueMember];
    }

    public override object ConvertTo(
        ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value,
        Type destinationType
    )
    {
        if (value == null)
        {
            return null;
        }

        if (context == null)
        {
            return value.ToString();
        }

        if (destinationType == typeof(string))
        {
            IDataLookup lookup = ((context.Instance as StateMachineState).RootItem as StateMachine)
                .Field
                .DefaultLookup;
            InitLookup(lookup);
            if (lookup == null)
            {
                return value.ToString();
            }

            if (_currentList == null)
            {
                InitList((Guid)lookup.PrimaryKey["Id"]);
            }

            _currentList.Sort = lookup.ListValueMember;
            int i = _currentList.Find(value);
            if (i == -1)
            {
                return value.ToString();
            }

            return _currentList[i].Row[lookup.ListDisplayMember].ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ContextStore currentItem = context.Instance as ContextStore;
        if (!(currentItem.Structure is DataStructure))
        {
            return new StandardValuesCollection(new List<DataStructureRuleSet>());
        }

        List<DataStructureRuleSet> ruleSets = (currentItem.Structure as DataStructure).RuleSets;
        var array = new List<DataStructureRuleSet>(ruleSets.Count);
        foreach (DataStructureRuleSet item in ruleSets)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ContextStore currentItem = context.Instance as ContextStore;
            if (!(currentItem.Structure is DataStructure))
            {
                return null;
            }

            List<DataStructureRuleSet> ruleSets = (currentItem.Structure as DataStructure).RuleSets;
            foreach (DataStructureRuleSet item in ruleSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        ContextStore currentItem = context.Instance as ContextStore;
        if (!(currentItem.Structure is DataStructure))
        {
            return new StandardValuesCollection(new List<DataStructureDefaultSet>());
        }

        List<DataStructureDefaultSet> defaultSets = (
            currentItem.Structure as DataStructure
        ).DefaultSets;
        var array = new List<DataStructureDefaultSet>(defaultSets.Count);
        foreach (DataStructureDefaultSet item in defaultSets)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            ContextStore currentItem = context.Instance as ContextStore;
            if (!(currentItem.Structure is DataStructure))
            {
                return null;
            }

            List<DataStructureDefaultSet> defaultSets = (
                currentItem.Structure as DataStructure
            ).DefaultSets;
            foreach (DataStructureDefaultSet item in defaultSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        WorkQueueClass currentItem = context.Instance as WorkQueueClass;
        if (currentItem.Entity == null)
        {
            return new StandardValuesCollection(new List<EntityFilter>());
        }

        List<EntityFilter> filters = currentItem.Entity.ChildItemsByType<EntityFilter>(
            EntityFilter.CategoryConst
        );
        var array = new List<EntityFilter>(filters.Count);
        foreach (EntityFilter item in filters)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            WorkQueueClass currentItem = context.Instance as WorkQueueClass;
            if (currentItem.Entity == null)
            {
                return null;
            }

            List<EntityFilter> filters = currentItem.Entity.ChildItemsByType<EntityFilter>(
                EntityFilter.CategoryConst
            );
            foreach (EntityFilter item in filters)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        WorkQueueClass currentItem = context.Instance as WorkQueueClass;
        if (currentItem.EntityStructure == null)
        {
            return new StandardValuesCollection(new List<DataStructureMethod>());
        }

        List<DataStructureMethod> methods = currentItem.EntityStructure.Methods;
        var array = new List<DataStructureMethod>(methods.Count);
        foreach (DataStructureMethod item in methods)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            WorkQueueClass currentItem = context.Instance as WorkQueueClass;
            if (currentItem.EntityStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = currentItem.EntityStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}

public class WorkQueueClassNotificationStructureFilterConverter
    : System.ComponentModel.TypeConverter
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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        WorkQueueClass currentItem = context.Instance as WorkQueueClass;
        if (currentItem.NotificationStructure == null)
        {
            return new StandardValuesCollection(new List<DataStructureMethod>());
        }

        List<DataStructureMethod> methods = currentItem.NotificationStructure.Methods;
        var array = new List<DataStructureMethod>(methods.Count);
        foreach (DataStructureMethod item in methods)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            WorkQueueClass currentItem = context.Instance as WorkQueueClass;
            if (currentItem.NotificationStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = currentItem.NotificationStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        WorkQueueClass currentItem = context.Instance as WorkQueueClass;
        if (currentItem.WorkQueueStructure == null)
        {
            return new StandardValuesCollection(new List<DataStructureMethod>());
        }

        List<DataStructureMethod> methods = currentItem.WorkQueueStructure.Methods;
        var array = new List<DataStructureMethod>(methods.Count);
        foreach (DataStructureMethod item in methods)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            WorkQueueClass currentItem = context.Instance as WorkQueueClass;
            if (currentItem.WorkQueueStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = currentItem.WorkQueueStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        WorkQueueClass currentItem = context.Instance as WorkQueueClass;
        if (currentItem.WorkQueueStructure == null)
        {
            return new StandardValuesCollection(new List<DataStructureSortSet>());
        }

        List<DataStructureSortSet> sorts = currentItem.WorkQueueStructure.SortSets;
        var array = new List<DataStructureSortSet>(sorts.Count);
        foreach (DataStructureSortSet item in sorts)
        {
            array.Add(item);
        }
        array.Add(null);

        array.Sort();
        return new StandardValuesCollection(array);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            WorkQueueClass currentItem = context.Instance as WorkQueueClass;
            if (currentItem.WorkQueueStructure == null)
            {
                return null;
            }

            List<DataStructureSortSet> sorts = currentItem.WorkQueueStructure.SortSets;
            foreach (DataStructureSortSet item in sorts)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        List<IDataEntityColumn> fields;
        IDataEntity entity = (
            (context.Instance as WorkQueueClassEntityMapping).ParentItem as WorkQueueClass
        ).Entity;
        if (entity == null)
        {
            return null;
        }

        fields = entity.EntityColumns;
        fields.Sort();
        return new StandardValuesCollection(fields);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            List<IDataEntityColumn> fields;
            IDataEntity entity = (
                (context.Instance as WorkQueueClassEntityMapping).ParentItem as WorkQueueClass
            ).Entity;
            if (entity == null)
            {
                return null;
            }

            fields = entity.EntityColumns;
            foreach (IDataEntityColumn field in fields)
            {
                if (field.Name == value.ToString())
                {
                    return field;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        UIFormTask reference = context.Instance as UIFormTask;

        if (reference.RefreshDataStructure == null)
        {
            return null;
        }

        List<DataStructureMethod> methods = reference.RefreshDataStructure.Methods;
        var methodArray = new List<DataStructureMethod>(methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            methodArray.Add(method);
        }
        methodArray.Add(null);
        methodArray.Sort();
        return new StandardValuesCollection(methodArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            UIFormTask reference = context.Instance as UIFormTask;

            if (reference.RefreshDataStructure == null)
            {
                return null;
            }

            List<DataStructureMethod> methods = reference.RefreshDataStructure.Methods;
            foreach (DataStructureMethod item in methods)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

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

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(
        ITypeDescriptorContext context
    )
    {
        UIFormTask reference = context.Instance as UIFormTask;

        if (reference.RefreshDataStructure == null)
        {
            return null;
        }

        List<DataStructureSortSet> sortSets = reference.RefreshDataStructure.SortSets;
        var sortSetArray = new List<DataStructureSortSet>(sortSets.Count);
        foreach (DataStructureSortSet sortSet in sortSets)
        {
            sortSetArray.Add(sortSet);
        }
        sortSetArray.Add(null);
        sortSetArray.Sort();
        return new StandardValuesCollection(sortSetArray);
    }

    public override bool CanConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Type sourceType
    )
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(
        System.ComponentModel.ITypeDescriptorContext context,
        System.Globalization.CultureInfo culture,
        object value
    )
    {
        if (value.GetType() == typeof(string))
        {
            UIFormTask reference = context.Instance as UIFormTask;

            if (reference.RefreshDataStructure == null)
            {
                return null;
            }

            List<DataStructureSortSet> sortSets = reference.RefreshDataStructure.SortSets;
            foreach (DataStructureSortSet item in sortSets)
            {
                if (item.Name == value.ToString())
                {
                    return item;
                }
            }
            return null;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
