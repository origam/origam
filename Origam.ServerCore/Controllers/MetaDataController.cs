using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Origam.OrigamEngine.ModelXmlBuilders;

namespace Origam.ServerCore.Controllers
{
    [Route("api/[controller]")]
   // [ApiController]
    public class MetaDataController: ControllerBase
    {
        [HttpGet("[action]")]
        public string GetMenu()
        {
            return MenuXmlBuilder.GetMenu();
        }
    }
}
