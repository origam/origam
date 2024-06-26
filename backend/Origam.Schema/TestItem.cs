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

namespace Origam.Schema;
/// <summary>
/// Summary description for TestItem.
/// </summary>
[ClassMetaVersion("6.0.0")]
public class TestItem : AbstractSchemaItem
{
	public TestItem() : base() {}
	public TestItem(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public TestItem(ModelElementKey primaryKey) : base(primaryKey)	{}
	public string TestField = "";
	#region Overriden AbstractSchemaItem Methods
	public override string ItemType
	{
		get
		{
			return "TEST";
		}
	}
	public override string Icon
	{
		get
		{
			return "0";
		}
	}
	#endregion
}
