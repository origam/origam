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
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.Common;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.TestModel;

public enum TestCaseStepType
{
    InitialCheck,
    Step,
    FinalCheck,
}

/// <summary>
/// Summary description for TestCaseStep.
/// </summary>
[SchemaItemDescription("Step", "Steps", 24)]
[ClassMetaVersion("6.0.0")]
public class TestCaseStep : AbstractSchemaItem
{
    public const string CategoryConst = "TestCaseStep";

    public TestCaseStep()
        : base() { }

    public TestCaseStep(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public TestCaseStep(Key primaryKey)
        : base(primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override string Icon
    {
        get
        {
            switch (this.StepType)
            {
                case TestCaseStepType.InitialCheck:
                {
                    return "23";
                }
                case TestCaseStepType.Step:
                {
                    return "24";
                }

                case TestCaseStepType.FinalCheck:
                {
                    return "25";
                }
            }
            return "0";
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(this.ChecklistRule);
        base.GetExtraDependencies(dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
    #region Properties
    private TestCaseStepType _stepType = TestCaseStepType.Step;

    [Category("Test Step")]
    public TestCaseStepType StepType
    {
        get { return _stepType; }
        set { _stepType = value; }
    }

    public Guid ChecklistRuleId;

    [Category("Test Step")]
    [TypeConverter(typeof(TestChecklistRuleConverter))]
    public TestChecklistRule ChecklistRule
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ChecklistRuleId;
            return (ISchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key)
                as TestChecklistRule;
        }
        set { this.ChecklistRuleId = (Guid)value.PrimaryKey["Id"]; }
    }
    #endregion
}
