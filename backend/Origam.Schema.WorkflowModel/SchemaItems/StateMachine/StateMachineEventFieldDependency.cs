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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ContextStoreLink.
	/// </summary>
	[SchemaItemDescription("Field Dependency", "Field Dependencies", "field-dependency.png")]
    [HelpTopic("Data+Event+Field+Dependency")]
    [DefaultProperty("Field")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class StateMachineEventFieldDependency : AbstractSchemaItem
	{
		public const string CategoryConst = "StateMachineEventFieldDependency";

		public StateMachineEventFieldDependency() : base() {}

		public StateMachineEventFieldDependency(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public StateMachineEventFieldDependency(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Field);

			base.GetExtraDependencies (dependencies);
		}

		public override SchemaItemCollection ChildItems
		{
			get
			{
				return new SchemaItemCollection();
			}
		}
		#endregion

		#region Properties
		public Guid FieldId;

		[TypeConverter(typeof(StateMachineAllFieldConverter))]
		[NotNullModelElementRule()]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("field", "FieldId")]
		public IDataEntityColumn Field
		{
			get
			{
				ModelElementKey key = new ModelElementKey(this.FieldId);

				return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				this.FieldId = (Guid)value.PrimaryKey["Id"];

				if(this.FieldId == Guid.Empty)
				{
					this.Name = "";
				}
				else
				{
					this.Name = this.Field.Name;
				}
			}
		}
		#endregion
	}
}
