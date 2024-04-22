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
using Origam.DA.ObjectPersistence;

namespace Origam.OrigamEngine;

public class  OrigamProviderBuilder
{
    private List<AbstractSchemaItemProvider> providers = new List<AbstractSchemaItemProvider>();

    public OrigamProviderBuilder()
    {
                    providers.Add(new StringSchemaItemProvider());
                    providers.Add(new FeatureSchemaItemProvider());
                    providers.Add(new EntityModelSchemaItemProvider());
                    providers.Add(new DataConstantSchemaItemProvider());
                    providers.Add(new DatabaseDataTypeSchemaItemProvider());
                    providers.Add(new UserControlSchemaItemProvider());
                    providers.Add(new PanelSchemaItemProvider());
                    providers.Add(new FormSchemaItemProvider());
                    providers.Add(new DataStructureSchemaItemProvider());
                    providers.Add(new FunctionSchemaItemProvider());
                    providers.Add(new ServiceSchemaItemProvider());
                    providers.Add(new WorkflowSchemaItemProvider());
                    providers.Add(new TransformationSchemaItemProvider());
                    providers.Add(new ReportSchemaItemProvider());
                    providers.Add(new RuleSchemaItemProvider());
                    providers.Add(new MenuSchemaItemProvider());
                    //			AbstractSchemaItemProviders.Add(new TestScenarioSchemaItemProvider());
                    //			AbstractSchemaItemProviders.Add(new TestChecklistRuleSchemaItemProvider());
                    providers.Add(new WorkflowScheduleSchemaItemProvider());
                    providers.Add(new ScheduleTimeSchemaItemProvider());
                    providers.Add(new DataLookupSchemaItemProvider());
                    providers.Add(new DeploymentSchemaItemProvider());
                    providers.Add(new GraphicsSchemaItemProvider());
                    providers.Add(new StateMachineSchemaItemProvider());
                    providers.Add(new WorkQueueClassSchemaItemProvider());
                    providers.Add(new ChartSchemaItemProvider());
                    providers.Add(new PagesSchemaItemProvider());
                    providers.Add(new DashboardWidgetsSchemaItemProvider());
                    providers.Add(new StylesSchemaItemProvider());
                    providers.Add(new NotificationBoxSchemaItemProvider());
                    providers.Add(new TreeStructureSchemaItemProvider());
                    providers.Add(new KeyboardShortcutsSchemaItemProvider());
                    providers.Add(new SearchSchemaItemProvider());
                    providers.Add(new DeepLinkCategorySchemaItemProvider());
                    providers.Add(new XsltFunctionSchemaItemProvider());
        }
    public List<AbstractSchemaItemProvider> GetAll()
    {
            return providers;
        }

    public OrigamProviderBuilder SetSchemaProvider(IPersistenceProvider schemaProvider)
    {
            foreach (var provider in providers)
            {
                provider.PersistenceProvider = schemaProvider;
            }

            return this;
        }
}