using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Origam.Security.Identity;
using Origam.Server;
using System;
using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Controller
{
    [Authorize]
    [ApiController]
    [Route("internalApi/[controller]")]
    public class UIServiceController : ControllerBase
    {
        private readonly SessionObjects sessionObjects;
        private readonly IStringLocalizer<SharedResources> localizer;

        public UIServiceController(
            SessionObjects sessionObjects, 
            IServiceProvider serviceProvider,
            IStringLocalizer<SharedResources> localizer)
        {
            this.sessionObjects = sessionObjects;
            IdentityServiceAgent.ServiceProvider = serviceProvider;
            this.localizer = localizer;
        }

        [HttpGet("[action]")]
        public IActionResult InitPortal([FromQuery][Required]string locale)
        {
            Analytics.Instance.Log("UI_INIT");
            //TODO: find out how to setup locale cookies and incorporate
            // locale resolver
            /*// set locale
            locale = locale.Replace("_", "-");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
            // set locale to the cookie
            Response.Cookies.Append(
                ORIGAMLocaleResolver.ORIGAM_CURRENT_LOCALE, locale);*/
            try
            {
                //TODO: findout how to get request size limit
                return Ok(sessionObjects.UIService.InitPortal(4));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost("[action]")]
        public IActionResult InitUI([FromBody]UIRequest request)
        {
            // registerSession is important for sessionless handling
            try
            {
                return Ok(sessionObjects.UIManager.InitUI(
                    request: request,
                    addChildSession: false,
                    parentSession: null,
                    basicUIService: sessionObjects.UIService));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet("[action]")]
        public IActionResult DestroyUI(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            try
            {
                sessionObjects.UIService.DestroyUI(sessionFormIdentifier);
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet("[action]")]
        public IActionResult RefreshData(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            try
            {
                return Ok(sessionObjects.UIService.RefreshData(
                    sessionFormIdentifier, localizer));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet("[action]")]
        public IActionResult SaveDataQuery(
            [FromQuery][Required]Guid sessionFormIdentifier)
        {
            try
            {
                return Ok(sessionObjects.UIService.SaveDataQuery(
                    sessionFormIdentifier));
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

    }
}
