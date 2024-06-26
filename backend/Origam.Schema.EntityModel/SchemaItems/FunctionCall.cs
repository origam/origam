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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Linq;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;
[SchemaItemDescription("Function Call", "Fields", "icon_function-call.png")]
[HelpTopic("Function+Call+Field")]
[DefaultProperty("Function")]
[ClassMetaVersion("6.0.0")]
public class FunctionCall : AbstractDataEntityColumn
{
	public FunctionCall() {}
	public FunctionCall(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public FunctionCall(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractDataEntityColumn Members
	
	[Browsable(false)]
	public override bool UseFolders => false;
	public override string FieldType => "FunctionCall";
	public override bool ReadOnly
	{
		get
		{
			if(Function == null)
			{
				return true;
			}
			return Function.FunctionType == OrigamFunctionType.Standard;
		}
	}
	public override string Icon 
		=> Function == null ? "icon_function-call.png" : Function.Icon;
	public override bool CanMove(UI.IBrowserNode2 newNode)
	{
		// can move inside the same entity 
		return RootItem == ((ISchemaItem)newNode).RootItem;
	}
	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
		dependencies.Add(Function);
		base.GetExtraDependencies (dependencies);
	}
	#endregion
	#region Properties
	public Guid FunctionId;
	[Category("Function")]
	[TypeConverter(typeof(FunctionConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
    [XmlReference("function", "FunctionId")]
    public Function Function
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = this.FunctionId
			};
			try
			{
				return (Function)PersistenceProvider.RetrieveInstance(
					typeof(Function), key);
			}
			catch
			{
				return null;
			}
		}
		set
		{
			// We have to delete all child items
			foreach(ISchemaItem item in ChildItems)
			{
				item.IsDeleted = true;
			}
			if(value == null)
			{
				FunctionId = Guid.Empty;
				Name = "";
			}
			else
			{
				FunctionId = (Guid)value.PrimaryKey["Id"];
				if(Name == null)
				{
					Name = Function.Name;
				}
				// We generate all parameters to the function
				foreach(var abstractSchemaItem in Function.ChildItems)
				{
					var parameter = (FunctionParameter)abstractSchemaItem;
					var functionCallParameter 
						= NewItem<FunctionCallParameter>(
							SchemaExtensionId, null);
					functionCallParameter.FunctionParameter = parameter;
					functionCallParameter.Name = parameter.Name;
				}
			}
		}
	}
	private bool _forceDatabaseCalculation = false;
	[Category("Function"), DefaultValue(false)]
	[XmlAttribute("forceDatabaseCalculation")]
    public bool ForceDatabaseCalculation
	{
		get => _forceDatabaseCalculation;
		set => _forceDatabaseCalculation = value;
	}
	#endregion
	#region ISchemaItemFactory Members
	[Browsable(false)]
	public override Type[] NewItemTypes
	{
		get
		{
			var functionCallParameterType 
				= new[] {typeof(FunctionCallParameter)};
			return ParentItem is IDataEntity 
				? functionCallParameterType.Concat(base.NewItemTypes)
					.ToArray() 
				: functionCallParameterType;
		}
	}
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		return base.NewItem<T>(schemaExtensionId, group, 
			typeof(T) == typeof(FunctionCallParameter) ?
				"NewFunctionCallParameter" : null);
	}
	#endregion
	#region Convert
	public override bool CanConvertTo(Type type)
	{
		return
			(
				(
					type == typeof(FieldMappingItem)
					|| type == typeof(DetachedField)
				)
			&&
				(
					ParentItem is IDataEntity
				)
			);
	}
	protected override ISchemaItem ConvertTo<T>()
	{
		var converted = ParentItem.NewItem<T>(SchemaExtensionId, Group);
		if(converted is AbstractDataEntityColumn column)
		{
			CopyFieldMembers(this, column);
		}
		if(converted is FieldMappingItem fieldMappingItem)
		{
			fieldMappingItem.MappedColumnName = Name;
		}
		else if(typeof(T) == typeof(DetachedField))
		{
		}
		else
		{
			return base.ConvertTo<T>();
		}
		// does the common conversion tasks and persists both this and converted objects
		FinishConversion(this, converted);
		return converted;
	}
	#endregion
}
