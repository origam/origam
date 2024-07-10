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
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.TestModel;
[SchemaItemDescription("Alternative", "Alternatives", 27)]
[ClassMetaVersion("6.0.0")]
public class TestCaseAlternative : AbstractSchemaItem
{
	public const string CategoryConst = "TestCaseAlternative";
	public TestCaseAlternative() {}
	public TestCaseAlternative(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public TestCaseAlternative(Key primaryKey) : base(primaryKey) {}
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override string Icon => "27";
	public override bool UseFolders => false;
	#endregion
	#region ISchemaItemFactory Members
	public override Type[] NewItemTypes => new[]
	{
		typeof(TestCaseStep)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
		return base.NewItem<T>(schemaExtensionId, group,
			typeof(T) == typeof(TestCaseStep)
				? "NewTestCaseStep" : null);
	}
	#endregion
}
