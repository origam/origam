using System;
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
        public string GetMenu()
        {    
            return MenuXmlBuilder.GetMenu();
        }

        [HttpGet("[action]")]
        public string GetScreeSection(Guid id)
        {
            return FormXmlBuilder.GetXml(id).OuterXml;
        }
    }
}
