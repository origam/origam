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
    [ApiController]
    [Route("api/[controller]")]
    public abstract class AbstractController: ControllerBase
    {
        protected readonly ILogger<AbstractController> log;

        public AbstractController(ILogger<AbstractController> log)
        {
            this.log = log;
        }

        public MenuLookupIndex MenuLookupIndex {
            get
            {
                string allowedLookups = "AllowedLookups";
                if (!OrigamUserContext.Context.ContainsKey(allowedLookups))
                {
                    OrigamUserContext.Context.Add(allowedLookups, new MenuLookupIndex());
                }
                var lookupIndex = (MenuLookupIndex)OrigamUserContext.Context[allowedLookups];
                return lookupIndex;
            }
        }
    }
}
