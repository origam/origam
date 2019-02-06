using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Origam.ServerCore;

namespace Origam.ServerCoreTests
{
    abstract class ControllerTests
    {
        protected SessionObjects sessionObjects;
        protected ControllerContext context;

        protected ControllerTests()
        {
            sessionObjects = new SessionObjects();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "JohnDoe"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("name", "JohnDoe"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
            Thread.CurrentPrincipal = principal;
        }
    }
}