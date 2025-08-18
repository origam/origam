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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;


namespace Origam.Schema.GuiModel;
[XmlModelRoot(CategoryConst)]
public abstract class AbstractReport : AbstractSchemaItem
	{
		public const string CategoryConst = "Report";
		public AbstractReport() {}
	
		public AbstractReport(Guid schemaExtensionId) 
			: base(schemaExtensionId) {}
		public AbstractReport(Key primaryKey) : base(primaryKey) {}
		#region Properties
		private string _caption = "";
		[Category("User Interface")]
		[Localizable(true)]
		public string Caption
		{
			get => _caption;
			set => _caption = value;
		}
		#endregion
       
		#region Overriden ISchemaItem Members
		public override string ItemType => CategoryConst;
		#endregion			
		#region ISchemaItemFactory Members
		public override Type[] NewItemTypes => new[]
		{
			typeof(DefaultValueParameter), 
			typeof(SchemaItemParameter), 
			typeof(XsltInitialValueParameter)
		};
		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			return base.NewItem<T>(schemaExtensionId, group, 
				typeof(T) == typeof(FunctionCallParameter) ?
					"NewParameter" : null);
		}
		#endregion
}
