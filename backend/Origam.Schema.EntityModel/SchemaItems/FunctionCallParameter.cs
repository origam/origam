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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;
[SchemaItemDescription("Parameter", 15)]
[HelpTopic("Function+Call+Field")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class FunctionCallParameter : AbstractSchemaItem
{
	public const string CategoryConst = "FunctionCallParameter";
	public FunctionCallParameter() {}
	public FunctionCallParameter(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public FunctionCallParameter(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType => CategoryConst;
	public override string Icon => "15";
	[Browsable(false)]
	public override bool UseFolders => false;
	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
		dependencies.Add(FunctionParameter);
		base.GetExtraDependencies (dependencies);
	}
	#endregion
	#region Properties
	public Guid FunctionParameterId;
	[NotNullModelElementRule()]
    [XmlReference("parameter", "FunctionParameterId")]
    public FunctionParameter FunctionParameter
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = FunctionParameterId
			};
			return (FunctionParameter)PersistenceProvider.RetrieveInstance(
				typeof(FunctionParameter), key);
		}
		set => FunctionParameterId = (Guid)value.PrimaryKey["Id"];
	}
	#endregion
	#region ISchemaItemFactory Members
	[Browsable(false)]
	public override Type[] NewItemTypes =>
		new[] {
			typeof(EntityColumnReference),
			typeof(FunctionCall),
			typeof(ParameterReference),
			typeof(DataConstantReference),
			typeof(EntityFilterReference),
			typeof(EntityFilterLookupReference)
		};
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		if(obj is FunctionCallParameter functionCallParameter)
		{
			return FunctionParameter.OrdinalPosition.CompareTo(
				functionCallParameter.FunctionParameter.OrdinalPosition);
		}
		return base.CompareTo(obj);
	}
	#endregion
}
