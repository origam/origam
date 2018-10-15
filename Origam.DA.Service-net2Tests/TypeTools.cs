using System;
using System.Collections.Generic;
using System.Linq;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Schema.RuleModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using Origam.Workbench.Services;

namespace Origam.DA.Service_net2Tests
{
    internal static class TypeTools
    {
        public static IEnumerable<Type> AllProviderTypes =>
            ((SchemaService) ServiceManager.Services
                .GetService(typeof(SchemaService)))
            .Providers
            .Select(provider => provider.GetType());

        public static SchemaItemCollection GetAllItems(Type providerType)
        {
            SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService))
                as SchemaService;
            ISchemaItemProvider provider = schema.GetProvider(providerType);
            return provider.ChildItems;
        }
    }
}