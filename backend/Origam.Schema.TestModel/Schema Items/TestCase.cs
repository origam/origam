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
using Origam.DA.Common;

namespace Origam.Schema.TestModel;

[SchemaItemDescription("Test Case", 26)]
[ClassMetaVersion("6.0.0")]
public class TestCase : AbstractSchemaItem
{
    public const string CategoryConst = "TestCase";

    public TestCase() { }

    public TestCase(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public TestCase(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;
    public override string Icon => "26";

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        // can move test cases between scenarios
        return newNode is TestScenario;
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[] { typeof(TestCaseAlternative), typeof(TestCaseStep) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(TestCaseAlternative))
        {
            itemName = "NewTestCaseAlternative";
        }
        else if (typeof(T) == typeof(TestCaseStep))
        {
            itemName = "NewTestCaseCheck";
        }
        return base.NewItem<T>(schemaExtensionId, group, itemName);
    }
    #endregion
    #region Properties
    private string _role;

    public string Role
    {
        get => _role;
        set => _role = value;
    }
    #endregion
}
