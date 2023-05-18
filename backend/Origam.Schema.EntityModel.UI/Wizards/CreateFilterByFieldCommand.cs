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

namespace Origam.Schema.EntityModel.UI.Wizards
{
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
                throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
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
			EntityHelper.CreateFilter(field, "Equal", "GetBy", false, GeneratedModelElements);
		}
	}

	public class CreateFilterWithParameterByFieldCommand : AbstractFilterMenuCommand
    {
		public override void Run()
		{
			IDataEntityColumn field = Owner as IDataEntityColumn;
			EntityHelper.CreateFilter(field, "Equal", "GetBy", true, GeneratedModelElements);
		}
	}

	public class CreateFilterLikeWithParameterByFieldCommand : AbstractFilterMenuCommand
    {
		public override void Run()
		{
			IDataEntityColumn field = Owner as IDataEntityColumn;
			EntityHelper.CreateFilter(field, "Like", "GetLike", true, GeneratedModelElements);
		}
	}

	public class CreateFilterByListWithParameterByFieldCommand : AbstractFilterMenuCommand
    {
		public override void Run()
		{
			IDataEntityColumn field = Owner as IDataEntityColumn;
			EntityHelper.CreateFilter(field, "In", "GetBy", true, GeneratedModelElements);
		}
	}

	public class CreateFilterLikeByFieldCommand : AbstractFilterMenuCommand
    {
		public override void Run()
		{
			IDataEntityColumn field = Owner as IDataEntityColumn;
			EntityHelper.CreateFilter(field, "Like", "GetLike", false, GeneratedModelElements);
		}
	}

	public class CreateFilterBetweenWithParameterByFieldCommand : AbstractFilterMenuCommand
    {
		WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;

		public override void Run()
		{
		    IDataEntityColumn field = Owner as IDataEntityColumn;
            if (field.Name == null) throw new ArgumentException("Filed Name is not set.");
			IDataEntity entity = field.ParentItem as IDataEntity;
            // first paramater
			DatabaseParameter param1 = entity.NewItem<DatabaseParameter>( 
                _schema.ActiveSchemaExtensionId, null);
			param1.DataType = field.DataType;
			param1.DataLength = field.DataLength;
			param1.Name = "par" + (field.Name.StartsWith("ref") ? field.Name.Substring(3) : field.Name) + "From";
			param1.Persist();
            GeneratedModelElements.Add(param1);
            // second parameter
            DatabaseParameter param2 = entity.NewItem<DatabaseParameter>( 
                _schema.ActiveSchemaExtensionId, null);
			param2.DataType = field.DataType;
			param2.DataLength = field.DataLength;
			param2.Name = "par" + (field.Name.StartsWith("ref") ? field.Name.Substring(3) : field.Name) + "To";
			param2.Persist();
            GeneratedModelElements.Add(param2);
            // filter
            EntityFilter filter = entity.NewItem<EntityFilter>(
	            _schema.ActiveSchemaExtensionId, null);
			filter.Name = "GetBetween" + (field.Name.StartsWith("ref") ? field.Name.Substring(3) : field.Name);
			filter.Persist();
            GeneratedModelElements.Add(filter);
            // function call
			FunctionCall call = filter.NewItem<FunctionCall>(
				_schema.ActiveSchemaExtensionId, null);
			FunctionSchemaItemProvider functionProvider = _schema.GetProvider(typeof(FunctionSchemaItemProvider)) as FunctionSchemaItemProvider;
			Function equalFunction = (Function)functionProvider.GetChildByName("Between", Function.CategoryConst);
			if(equalFunction == null) throw new Exception(ResourceUtils.GetString("ErrorBetweenFunctionNotFound"));
			call.Function = equalFunction;
			call.Name = "Between";
			call.Persist();
            // function parameters
            EntityColumnReference reference1 = call.GetChildByName("Expression")
	            .NewItem<EntityColumnReference>(
		            _schema.ActiveSchemaExtensionId, null);
			reference1.Field = field;
			reference1.Persist();
			ParameterReference reference2 = call.GetChildByName("Left")
				.NewItem<ParameterReference>(
					_schema.ActiveSchemaExtensionId, null);
			reference2.Parameter = param1;
			reference2.Persist();
			ParameterReference reference3 = call.GetChildByName("Right")
				.NewItem<ParameterReference>(
					_schema.ActiveSchemaExtensionId, null);
			reference3.Parameter = param2;
			reference3.Persist();
		}
	}
}
