using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Workbench.Services;

namespace Origam.ServerCore.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public abstract class AbstractController: ControllerBase
    {
        protected readonly ILogger<MetaDataController> log;

        public AbstractController(ILogger<MetaDataController> log)
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
    }
}
