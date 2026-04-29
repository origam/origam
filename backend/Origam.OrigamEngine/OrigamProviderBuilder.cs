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
using System.Collections.Generic;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.RuleModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;

namespace Origam.OrigamEngine;

public class OrigamProviderBuilder
{
    private List<AbstractSchemaItemProvider> providers = new List<AbstractSchemaItemProvider>();

    public OrigamProviderBuilder()
    {
        providers.Add(item: new StringSchemaItemProvider());
        providers.Add(item: new FeatureSchemaItemProvider());
        providers.Add(item: new EntityModelSchemaItemProvider());
        providers.Add(item: new DataConstantSchemaItemProvider());
        providers.Add(item: new DatabaseDataTypeSchemaItemProvider());
        providers.Add(item: new UserControlSchemaItemProvider());
        providers.Add(item: new PanelSchemaItemProvider());
        providers.Add(item: new FormSchemaItemProvider());
        providers.Add(item: new DataStructureSchemaItemProvider());
        providers.Add(item: new FunctionSchemaItemProvider());
        providers.Add(item: new ServiceSchemaItemProvider());
        providers.Add(item: new WorkflowSchemaItemProvider());
        providers.Add(item: new TransformationSchemaItemProvider());
        providers.Add(item: new ReportSchemaItemProvider());
        providers.Add(item: new RuleSchemaItemProvider());
        providers.Add(item: new MenuSchemaItemProvider());
        //			AbstractSchemaItemProviders.Add(new TestScenarioSchemaItemProvider());
        //			AbstractSchemaItemProviders.Add(new TestChecklistRuleSchemaItemProvider());
        providers.Add(item: new WorkflowScheduleSchemaItemProvider());
        providers.Add(item: new ScheduleTimeSchemaItemProvider());
        providers.Add(item: new DataLookupSchemaItemProvider());
        providers.Add(item: new DeploymentSchemaItemProvider());
        providers.Add(item: new GraphicsSchemaItemProvider());
        providers.Add(item: new StateMachineSchemaItemProvider());
        providers.Add(item: new WorkQueueClassSchemaItemProvider());
        providers.Add(item: new ChartSchemaItemProvider());
        providers.Add(item: new PagesSchemaItemProvider());
        providers.Add(item: new DashboardWidgetsSchemaItemProvider());
        providers.Add(item: new StylesSchemaItemProvider());
        providers.Add(item: new NotificationBoxSchemaItemProvider());
        providers.Add(item: new TreeStructureSchemaItemProvider());
        providers.Add(item: new KeyboardShortcutsSchemaItemProvider());
        providers.Add(item: new SearchSchemaItemProvider());
        providers.Add(item: new DeepLinkCategorySchemaItemProvider());
        providers.Add(item: new XsltFunctionSchemaItemProvider());
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
