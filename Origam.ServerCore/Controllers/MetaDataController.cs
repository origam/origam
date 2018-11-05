using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<MetaDataController> log;

        public MetaDataController( ILogger<MetaDataController> log)
        {
            this.log = log;

            var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
            if (persistenceService == null)
            {
                Reflector.ClassCache = new NullReflectorCache();
                CoreRuntimeServiceFactory serviceFactory = new CoreRuntimeServiceFactory();
                OrigamEngine.OrigamEngine.ConnectRuntime(customServiceFactory: serviceFactory);
            }
        }

        [HttpGet("[action]")]
        public string GetMenu()
        {    
            return MenuXmlBuilder.GetMenu();
        }
    }
}
