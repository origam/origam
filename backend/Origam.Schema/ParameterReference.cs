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

namespace Origam.Schema;
/// <summary>
/// Summary description for ParameterReference.
/// </summary>
[SchemaItemDescription("Parameter Reference", "icon_parameter-reference.png")]
[HelpTopic("Parameter+Reference")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Parameter")]
[ClassMetaVersion("6.0.0")]
public class ParameterReference : AbstractSchemaItem
{
	public const string CategoryConst = "ParameterReference";
	public ParameterReference() : base() {}
	public ParameterReference(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ParameterReference(Key primaryKey) : base(primaryKey)	{}
	
	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Parameter);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.Parameter.PrimaryKey))
				{
					this.Parameter = item as SchemaItemParameter;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	#endregion
	#region Properties
	public Guid ParameterId;
	[Category("Reference")]
	[TypeConverter(typeof(ParameterReferenceConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("parameter", "ParameterId")]
	public SchemaItemParameter Parameter
	{
		get
		{
			return (SchemaItemParameter)this.PersistenceProvider.RetrieveInstance(typeof(SchemaItemParameter), new ModelElementKey(this.ParameterId)) as SchemaItemParameter;
		}
		set
		{
			this.ParameterId = (Guid)value.PrimaryKey["Id"];
			if(this.Name == null)
			{
				this.Name = this.Parameter.Name;
			}
		}
	}
	#endregion
}
