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
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel.UI.Wizards;

public abstract class AbstractFilterMenuCommand : AbstractMenuCommand
{
    public override bool IsEnabled
    {
        get
        {
            IDataEntityColumn column = Owner as IDataEntityColumn;
            return column != null && column.ParentItem is IDataEntity;
        }
        set
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorSetProperty"),
                paramName: "IsEnabled"
            );
        }
    }
}

/// <summary>
/// Summary description for CreateFilterByFieldCommand.
/// </summary>
public class CreateFilterByFieldCommand : AbstractFilterMenuCommand
{
    public override void Run()
    {
        IDataEntityColumn field = Owner as IDataEntityColumn;
        EntityHelper.CreateFilter(
            field: field,
            functionName: "Equal",
            filterPrefix: "GetBy",
            createParameter: false,
            generatedElements: GeneratedModelElements
        );
    }
}

public class CreateFilterWithParameterByFieldCommand : AbstractFilterMenuCommand
{
    public override void Run()
    {
        IDataEntityColumn field = Owner as IDataEntityColumn;
        EntityHelper.CreateFilter(
            field: field,
            functionName: "Equal",
            filterPrefix: "GetBy",
            createParameter: true,
            generatedElements: GeneratedModelElements
        );
    }
}

public class CreateFilterLikeWithParameterByFieldCommand : AbstractFilterMenuCommand
{
    public override void Run()
    {
        IDataEntityColumn field = Owner as IDataEntityColumn;
        EntityHelper.CreateFilter(
            field: field,
            functionName: "Like",
            filterPrefix: "GetLike",
            createParameter: true,
            generatedElements: GeneratedModelElements
        );
    }
}

public class CreateFilterByListWithParameterByFieldCommand : AbstractFilterMenuCommand
{
    public override void Run()
    {
        IDataEntityColumn field = Owner as IDataEntityColumn;
        EntityHelper.CreateFilter(
            field: field,
            functionName: "In",
            filterPrefix: "GetBy",
            createParameter: true,
            generatedElements: GeneratedModelElements
        );
    }
}

public class CreateFilterLikeByFieldCommand : AbstractFilterMenuCommand
{
    public override void Run()
    {
        IDataEntityColumn field = Owner as IDataEntityColumn;
        EntityHelper.CreateFilter(
            field: field,
            functionName: "Like",
            filterPrefix: "GetLike",
            createParameter: false,
            generatedElements: GeneratedModelElements
        );
    }
}

public class CreateFilterBetweenWithParameterByFieldCommand : AbstractFilterMenuCommand
{
    WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;

    public override void Run()
    {
        IDataEntityColumn field = Owner as IDataEntityColumn;
        if (field.Name == null)
        {
            throw new ArgumentException(message: "Filed Name is not set.");
        }

        IDataEntity entity = field.ParentItem as IDataEntity;
        // first paramater
        DatabaseParameter param1 = entity.NewItem<DatabaseParameter>(
            schemaExtensionId: _schema.ActiveSchemaExtensionId,
            group: null
        );
        param1.DataType = field.DataType;
        param1.DataLength = field.DataLength;
        param1.Name =
            "par"
            + (
                field.Name.StartsWith(value: "ref")
                    ? field.Name.Substring(startIndex: 3)
                    : field.Name
            )
            + "From";
        param1.Persist();
        GeneratedModelElements.Add(item: param1);
        // second parameter
        DatabaseParameter param2 = entity.NewItem<DatabaseParameter>(
            schemaExtensionId: _schema.ActiveSchemaExtensionId,
            group: null
        );
        param2.DataType = field.DataType;
        param2.DataLength = field.DataLength;
        param2.Name =
            "par"
            + (
                field.Name.StartsWith(value: "ref")
                    ? field.Name.Substring(startIndex: 3)
                    : field.Name
            )
            + "To";
        param2.Persist();
        GeneratedModelElements.Add(item: param2);
        // filter
        EntityFilter filter = entity.NewItem<EntityFilter>(
            schemaExtensionId: _schema.ActiveSchemaExtensionId,
            group: null
        );
        filter.Name =
            "GetBetween"
            + (
                field.Name.StartsWith(value: "ref")
                    ? field.Name.Substring(startIndex: 3)
                    : field.Name
            );
        filter.Persist();
        GeneratedModelElements.Add(item: filter);
        // function call
        FunctionCall call = filter.NewItem<FunctionCall>(
            schemaExtensionId: _schema.ActiveSchemaExtensionId,
            group: null
        );
        FunctionSchemaItemProvider functionProvider =
            _schema.GetProvider(type: typeof(FunctionSchemaItemProvider))
            as FunctionSchemaItemProvider;
        Function equalFunction = (Function)
            functionProvider.GetChildByName(name: "Between", itemType: Function.CategoryConst);
        if (equalFunction == null)
        {
            throw new Exception(
                message: ResourceUtils.GetString(key: "ErrorBetweenFunctionNotFound")
            );
        }

        call.Function = equalFunction;
        call.Name = "Between";
        call.Persist();
        // function parameters
        EntityColumnReference reference1 = call.GetChildByName(name: "Expression")
            .NewItem<EntityColumnReference>(
                schemaExtensionId: _schema.ActiveSchemaExtensionId,
                group: null
            );
        reference1.Field = field;
        reference1.Persist();
        ParameterReference reference2 = call.GetChildByName(name: "Left")
            .NewItem<ParameterReference>(
                schemaExtensionId: _schema.ActiveSchemaExtensionId,
                group: null
            );
        reference2.Parameter = param1;
        reference2.Persist();
        ParameterReference reference3 = call.GetChildByName(name: "Right")
            .NewItem<ParameterReference>(
                schemaExtensionId: _schema.ActiveSchemaExtensionId,
                group: null
            );
        reference3.Parameter = param2;
        reference3.Persist();
    }
}
