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
        public IActionResult GetMenu()
        {    
            return Ok(MenuXmlBuilder.GetMenu());
        }

        [HttpGet("[action]")]
        public IActionResult GetScreeSection([FromQuery] [Required] Guid id)
        {
            XmlOutput xmlOutput = FormXmlBuilder.GetXml(id);
            MenuLookupIndex.AddIfNotPresent(id, xmlOutput.ContainedLookups);
            return Ok(xmlOutput.Document.OuterXml);
        }
    }
}
