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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;


namespace Origam.Schema.GuiModel
{
	[XmlModelRoot(CategoryConst)]
    public abstract class AbstractReport : AbstractSchemaItem, ISchemaItemFactory
		{
			public const string CategoryConst = "Report";

			public AbstractReport() : base() {}
		
			public AbstractReport(Guid schemaExtensionId) : base(schemaExtensionId) {}

			public AbstractReport(Key primaryKey) : base(primaryKey) {}

			#region Properties
			private string _caption = "";
			[Category("User Interface")]
			[Localizable(true)]
			public string Caption
			{
				get
				{
					return _caption;
				}
				set
				{
					_caption = value;
				}
			}


			#endregion
           
			#region Overriden AbstractSchemaItem Members
			public override string ItemType
			{
				get
				{
					return AbstractReport.CategoryConst;
				}
			}

			#endregion			


			#region ISchemaItemFactory Members

			public override Type[] NewItemTypes
			{
				get
				{
					return new Type[] {typeof(DefaultValueParameter), typeof(SchemaItemParameter), typeof(XsltInitialValueParameter)};
				}
			}

			public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
			{
				AbstractSchemaItem item;

				if(type == typeof(DefaultValueParameter))
				{
					item =  new DefaultValueParameter(schemaExtensionId);
					item.Name = "NewParameter";					
				}
				else if(type == typeof(SchemaItemParameter))
				{
					item =  new SchemaItemParameter(schemaExtensionId);
					item.Name = "NewParameter";					
				}
                else if (type == typeof(XsltInitialValueParameter))
                {
                    item = new XsltInitialValueParameter(schemaExtensionId);
                    item.Name = "NewParameter";
                }
				else
				{
					throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorReportUnknownType"));
				}

				item.PersistenceProvider = this.PersistenceProvider;
				item.Group = group;
				this.ChildItems.Add(item);
				return item;
			}

			#endregion

	}
}
