#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.TestModel
{
	/// <summary>
	/// Summary description for TestChecklistRule.
	/// </summary>
	[SchemaItemDescription("Checklist Rule", "Checklist Rules", 16)]
	public class TestChecklistRule : AbstractSchemaItem
	{
		public const string ItemTypeConst = "TestChecklistRule";

		public TestChecklistRule() : base() {}

		public TestChecklistRule(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public TestChecklistRule(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		public override string Icon
		{
			get
			{
				return "16";
			}
		}

		public override SchemaItemCollection ChildItems
		{
			get
			{
				return new SchemaItemCollection();
			}
		}
		#endregion
	}
}
