using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Origam.ServerCore.Controllers;

namespace Origam.ServerCoreTests
{
    [SetUpFixture]
    public class TestsSetupClass
    {

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Origam.OrigamEngine.OrigamEngine.ConnectRuntime();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }
}