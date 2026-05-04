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
                itemType: ContextStore.CategoryConst
            );
            foreach (ContextStore store in contexts)
            {
                contextArray.Add(item: store);
            }
            // move up to the next item
            item = item.ParentItem;
        }
        contextArray.Add(item: null);
        contextArray.Sort();
        return new StandardValuesCollection(values: contextArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
                    itemType: ContextStore.CategoryConst
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<DataStructureEntity>());
        }
        DataStructure ds = (DataStructure)updateContextTask.OutputContextStore.Structure;
        List<DataStructureEntity> entities = ds.Entities;
        var entityArray = new List<DataStructureEntity>(capacity: entities.Count);
        foreach (DataStructureEntity entity in entities)
        {
            entityArray.Add(item: entity);
        }
        entityArray.Sort();
        return new StandardValuesCollection(values: entityArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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

        contexts = wf.ChildItemsByType<ContextStore>(itemType: ContextStore.CategoryConst);
        var contextArray = new List<ContextStore>(capacity: contexts.Count);

        foreach (ContextStore store in contexts)
        {
            contextArray.Add(item: store);
        }
        contextArray.Sort();
        return new StandardValuesCollection(values: contextArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

            contexts = wf.ChildItemsByType<ContextStore>(itemType: ContextStore.CategoryConst);
            foreach (ContextStore store in contexts)
            {
                if (store.Name == value.ToString())
                {
                    return store;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
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

        contexts = wf.ChildItemsByType<ContextStore>(itemType: ContextStore.CategoryConst);
        var contextArray = new List<IContextStore>(capacity: contexts.Count);
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
                contextArray.Add(item: store);
            }
        }
        contextArray.Sort();
        return new StandardValuesCollection(values: contextArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

            contexts = wf.ChildItemsByType<ContextStore>(itemType: ContextStore.CategoryConst);
            foreach (ContextStore store in contexts)
            {
                if (store.Name == value.ToString())
                {
                    return store;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        fieldArray = new List<IDataEntityColumn>(capacity: fields.Count);

        foreach (IDataEntityColumn field in fields)
        {
            fieldArray.Add(item: field);
        }
        fieldArray.Sort();
        return new StandardValuesCollection(values: fieldArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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

        steps = wf.ChildItemsByType<WorkflowTask>(itemType: WorkflowTask.CategoryConst);
        stepArray = new List<WorkflowTask>(capacity: steps.Count);

        foreach (WorkflowTask step in steps)
        {
            stepArray.Add(item: step);
        }
        stepArray.Sort();
        return new StandardValuesCollection(values: stepArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

            tasks = wf.ChildItemsByType<WorkflowTask>(itemType: WorkflowTask.CategoryConst);
            foreach (WorkflowTask task in tasks)
            {
                if (task.Name == value.ToString())
                {
                    return task;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
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

        steps = wf.ChildItemsByType<IWorkflowStep>(itemType: AbstractWorkflowStep.CategoryConst);
        var stepArray = new List<IWorkflowStep>(capacity: steps.Count);

        foreach (IWorkflowStep step in steps)
        {
            if (step.Id != parentStep?.Id)
            {
                stepArray.Add(item: step);
            }
        }
        stepArray.Sort();
        return new StandardValuesCollection(values: stepArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

            tasks = wf.ChildItemsByType<WorkflowTask>(itemType: WorkflowTask.CategoryConst);
            foreach (WorkflowTask task in tasks)
            {
                if (task.Name == value.ToString())
                {
                    return task;
                }
            }
            return null;
        }

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class ServiceConverter : System.ComponentModel.TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

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
            _schema.GetProvider(type: typeof(ServiceSchemaItemProvider))
            as ServiceSchemaItemProvider;
        var columnArray = new List<IService>(capacity: services.ChildItems.Count);
        foreach (IService service in services.ChildItems)
        {
            columnArray.Add(item: service);
        }
        columnArray.Sort();
        return new StandardValuesCollection(values: columnArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
                _schema.GetProvider(type: typeof(ServiceSchemaItemProvider))
                as ServiceSchemaItemProvider;
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        var methodArray = new List<ISchemaItem>(capacity: methods.Count);

        foreach (ISchemaItem method in methods)
        {
            methodArray.Add(item: method);
        }
        methodArray.Sort();
        return new StandardValuesCollection(values: methodArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        fields.Add(item: null);
        fields.Sort();
        return new StandardValuesCollection(values: fields);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        ).ChildItemsByType<StateMachineState>(itemType: StateMachineState.CategoryConst);
        ;
        states.Sort();
        return new StandardValuesCollection(values: states);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
            ).ChildItemsByType<StateMachineState>(itemType: StateMachineState.CategoryConst);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        return new StandardValuesCollection(values: states);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
                mappedFields.Add(item: item);
            }
        }
        mappedFields.Sort();
        return new StandardValuesCollection(values: mappedFields);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
                    mappedFields.Add(item: col);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class ScheduleTimeConverter : System.ComponentModel.TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

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
            _schema.GetProvider(type: typeof(ScheduleTimeSchemaItemProvider))
            as ScheduleTimeSchemaItemProvider;
        var itemArray = new List<AbstractScheduleTime>(capacity: times.ChildItems.Count);
        foreach (AbstractScheduleTime time in times.ChildItems)
        {
            itemArray.Add(item: time);
        }
        itemArray.Sort();
        return new StandardValuesCollection(values: itemArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
                _schema.GetProvider(type: typeof(ScheduleTimeSchemaItemProvider))
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class WorkflowConverter : System.ComponentModel.TypeConverter
{
    static ISchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(ISchemaService)) as ISchemaService;

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
            _schema.GetProvider(type: typeof(WorkflowSchemaItemProvider))
            as WorkflowSchemaItemProvider;
        var itemArray = new List<IWorkflow>(capacity: workflows.ChildItems.Count);
        foreach (IWorkflow wf in workflows.ChildItems)
        {
            itemArray.Add(item: wf);
        }
        itemArray.Sort();
        return new StandardValuesCollection(values: itemArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
                _schema.GetProvider(type: typeof(WorkflowSchemaItemProvider))
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}

public class StateMachineStateLookupReaderConverter : System.ComponentModel.TypeConverter
{
    IDataLookupService _lookupManager =
        ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
        as IDataLookupService;
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
        _currentList = _lookupManager.GetList(lookupId: id, transactionId: null);
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
        InitLookup(lookup: lookup);
        if (lookup == null)
        {
            return null;
        }

        InitList(id: (Guid)lookup.PrimaryKey[key: "Id"]);
        _currentList.Sort = lookup.ListDisplayMember;
        var list = new List<object>(capacity: _currentList.Count);

        foreach (DataRowView rowview in _currentList)
        {
            list.Add(item: rowview.Row[columnName: lookup.ListValueMember]);
        }
        return new StandardValuesCollection(values: list);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
        InitLookup(lookup: lookup);
        _currentList.Sort = lookup.ListDisplayMember;
        int i = _currentList.Find(key: value);

        if (i == -1)
        {
            return null;
        }

        return _currentList[recordIndex: i].Row[columnName: lookup.ListValueMember];
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
            InitLookup(lookup: lookup);
            if (lookup == null)
            {
                return value.ToString();
            }

            if (_currentList == null)
            {
                InitList(id: (Guid)lookup.PrimaryKey[key: "Id"]);
            }

            _currentList.Sort = lookup.ListValueMember;
            int i = _currentList.Find(key: value);
            if (i == -1)
            {
                return value.ToString();
            }

            return _currentList[recordIndex: i]
                .Row[columnName: lookup.ListDisplayMember]
                .ToString();
        }

        return base.ConvertTo(
            context: context,
            culture: culture,
            value: value,
            destinationType: destinationType
        );
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
            return new StandardValuesCollection(values: new List<DataStructureRuleSet>());
        }

        List<DataStructureRuleSet> ruleSets = (currentItem.Structure as DataStructure).RuleSets;
        var array = new List<DataStructureRuleSet>(capacity: ruleSets.Count);
        foreach (DataStructureRuleSet item in ruleSets)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<DataStructureDefaultSet>());
        }

        List<DataStructureDefaultSet> defaultSets = (
            currentItem.Structure as DataStructure
        ).DefaultSets;
        var array = new List<DataStructureDefaultSet>(capacity: defaultSets.Count);
        foreach (DataStructureDefaultSet item in defaultSets)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<EntityFilter>());
        }

        List<EntityFilter> filters = currentItem.Entity.ChildItemsByType<EntityFilter>(
            itemType: EntityFilter.CategoryConst
        );
        var array = new List<EntityFilter>(capacity: filters.Count);
        foreach (EntityFilter item in filters)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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
                itemType: EntityFilter.CategoryConst
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<DataStructureMethod>());
        }

        List<DataStructureMethod> methods = currentItem.EntityStructure.Methods;
        var array = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod item in methods)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<DataStructureMethod>());
        }

        List<DataStructureMethod> methods = currentItem.NotificationStructure.Methods;
        var array = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod item in methods)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<DataStructureMethod>());
        }

        List<DataStructureMethod> methods = currentItem.WorkQueueStructure.Methods;
        var array = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod item in methods)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
            return new StandardValuesCollection(values: new List<DataStructureSortSet>());
        }

        List<DataStructureSortSet> sorts = currentItem.WorkQueueStructure.SortSets;
        var array = new List<DataStructureSortSet>(capacity: sorts.Count);
        foreach (DataStructureSortSet item in sorts)
        {
            array.Add(item: item);
        }
        array.Add(item: null);

        array.Sort();
        return new StandardValuesCollection(values: array);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        return new StandardValuesCollection(values: fields);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        var methodArray = new List<DataStructureMethod>(capacity: methods.Count);
        foreach (DataStructureMethod method in methods)
        {
            methodArray.Add(item: method);
        }
        methodArray.Add(item: null);
        methodArray.Sort();
        return new StandardValuesCollection(values: methodArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
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
        var sortSetArray = new List<DataStructureSortSet>(capacity: sortSets.Count);
        foreach (DataStructureSortSet sortSet in sortSets)
        {
            sortSetArray.Add(item: sortSet);
        }
        sortSetArray.Add(item: null);
        sortSetArray.Sort();
        return new StandardValuesCollection(values: sortSetArray);
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

        return base.CanConvertFrom(context: context, sourceType: sourceType);
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

        return base.ConvertFrom(context: context, culture: culture, value: value);
    }
}
