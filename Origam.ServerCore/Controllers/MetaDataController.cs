using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.OrigamEngine.ModelXmlBuilders;

namespace Origam.ServerCore.Controllers
{
    public class MetaDataController: AbstractController
    {
        public MetaDataController(ILogger<MetaDataController> log) : base(log)
        {
        }

        [HttpGet("[action]")]
        public ActionResult<string> GetMenu()
        {    
            return MenuXmlBuilder.GetMenu();
        }

        [HttpGet("[action]")]
        public ActionResult<string> GetScreeSection([FromQuery] [Required] Guid id)
        {
            return FormXmlBuilder.GetXml(id).OuterXml;
        }
    }
}
