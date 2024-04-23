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

namespace Origam.Schema.TestModel;

[SchemaItemDescription("Test Scenario", 16)]
[ClassMetaVersion("6.0.0")]
public class TestScenario : AbstractSchemaItem
{
	public const string CategoryConst = "TestScenario";

	public TestScenario() {}

	public TestScenario(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public TestScenario(Key primaryKey) : base(primaryKey) {}

	#region Overriden AbstractSchemaItem Members
		
	public override string ItemType => CategoryConst;

	public override string Icon => "16";

	public override bool UseFolders => false;

	#endregion

	#region ISchemaItemFactory Members

	public override Type[] NewItemTypes => new[]
		{
			typeof(TestCase)
		};

	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
			return base.NewItem<T>(schemaExtensionId, group,
				typeof(T) == typeof(TestCase)
					? "NewTestCase" : null);
		}

	#endregion

}