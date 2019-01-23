using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Origam.Server;

namespace Origam.ServerCore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        [HttpPost("[action]")]
        public IActionResult New()
        {
            UIService uiService = new UIService();
            Guid newSessionId = Guid.NewGuid();
            UIRequest uiRequest = new UIRequest{FormSessionId = newSessionId.ToString()};
            uiService.InitUI(uiRequest);
            return Ok(newSessionId);
        }

        // PUT: api/Sessions/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
