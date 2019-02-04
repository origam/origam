using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Origam.ServerCore.Controllers;

namespace Tests
{
    [SetUpFixture]
    public class TestsSetupClass
    {

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Origam.OrigamEngine.OrigamEngine.ConnectRuntime();
            var testConfig = new TestConfig();
        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }

    class TestConfig : IConfiguration
    {
        public IConfigurationSection GetSection(string key)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new System.NotImplementedException();
        }

        public string this[string key]
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}