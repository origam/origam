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
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.RuleModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using System.Collections.Generic;

namespace Origam.OrigamEngine
{
    public class  OrigamProviders
    {
        private List<AbstractSchemaItemProvider> AbstractSchemaItemProviders = new List<AbstractSchemaItemProvider>();

        public OrigamProviders()
        {
                    AbstractSchemaItemProviders.Add(new StringSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new FeatureSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new EntityModelSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new DataConstantSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new DatabaseDataTypeSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new UserControlSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new PanelSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new FormSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new DataStructureSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new FunctionSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new ServiceSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new WorkflowSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new TransformationSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new ReportSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new RuleSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new MenuSchemaItemProvider());
                    //			AbstractSchemaItemProviders.Add(new TestScenarioSchemaItemProvider());
                    //			AbstractSchemaItemProviders.Add(new TestChecklistRuleSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new WorkflowScheduleSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new ScheduleTimeSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new DataLookupSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new DeploymentSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new GraphicsSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new StateMachineSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new WorkQueueClassSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new ChartSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new PagesSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new DashboardWidgetsSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new StylesSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new NotificationBoxSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new TreeStructureSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new KeyboardShortcutsSchemaItemProvider());
                    AbstractSchemaItemProviders.Add(new SearchSchemaItemProvider());
        }
        public List<AbstractSchemaItemProvider> GetAllProviders()
        {
            return AbstractSchemaItemProviders;
        }
    }
}
