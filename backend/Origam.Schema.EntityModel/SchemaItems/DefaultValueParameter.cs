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
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DeaultValueParameter.
	/// </summary>
	[SchemaItemDescription("Default Value Parameter", "Parameters", "icon_default-value-parameter.png")]
    [HelpTopic("Default+Value+Parameter")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DefaultValueParameter : SchemaItemParameter
	{
		public DefaultValueParameter() : base() {}

		public DefaultValueParameter(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DefaultValueParameter(Key primaryKey) : base(primaryKey)	{}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.DefaultValue);
		}

		#region Properties
		public Guid DefaultValueId;

		[Category("Reference")]
		[TypeConverter(typeof(DataConstantConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("defaultValue", "DefaultValueId")]
		public DataConstant DefaultValue
		{
			get
			{
				try
				{
					return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DefaultValueId)) as DataConstant;
				}
				catch
				{
					throw new Exception(ResourceUtils.GetString("ErrorDataConstantNotFound", this.Name));
				}
			}
			set
			{
				this.DefaultValueId = (Guid)value.PrimaryKey["Id"];
			}
		}
		#endregion
	}
}
