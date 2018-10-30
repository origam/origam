using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Origam.DA;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.ServerCore.Controllers
{
    [Route("api/[controller]")]
   // [ApiController]
    public class MetaDataController: ControllerBase
    {
        [HttpGet("[action]")]
        public string GetMenu()
        {
            Reflector.ClassCache = new NullReflectorCache();
            var DefaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };
            ServiceManager sManager = ServiceManager.Services;
            SchemaService schemaService = new SchemaService();
            IParameterService parameterService = new NullParameterService();

            sManager.AddService(schemaService);
            sManager.AddService(parameterService);

            var settings = new OrigamSettings();
            ConfigurationManager.SetActiveConfiguration(settings);
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("origam_server"), null);
            StateMachineSchemaItemProvider StateMachineSchema = new StateMachineSchemaItemProvider();
            var persistenceService = new FilePersistenceService(DefaultFolders);

            sManager.AddService(persistenceService);
            schemaService.AddProvider(StateMachineSchema);
            schemaService.AddProvider(new MenuSchemaItemProvider());
            return MenuXmlBuilder.GetMenu();
        }
    }
}
