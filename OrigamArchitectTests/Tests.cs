using System;
using System.IO;
using NUnit.Framework;
using Origam;
using Origam.OrigamEngine;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace OrigamArchitectTests
{
    [TestFixture]
    public class ArchitectTests
    {
        [Test,  Order(1)]
        public void ShouldCreateNewEntity()
        {
            OrigamEngine.ConnectRuntime(
                customServiceFactory: new TestRuntimeServiceFactory(),
                runRestartTimer: false);

            SchemaItemGroup parentGroup = 
                GetItemById<SchemaItemGroup>(new Guid("d86679c6-3cee-419e-afc0-98011fad460e"));

            TableMappingItem newItem = 
                EntityHelper.CreateTable("Test1", parentGroup, true);

            TableMappingItem persistedItem = GetItemById<TableMappingItem>(newItem.Id);
            Assert.That(persistedItem, Is.Not.Null);
            Assert.That(persistedItem.Id, Is.EqualTo(newItem.Id));

            string xmlFilePath = Path.Combine(ProjectTopDirectory, newItem.RelativeFilePath);
            FileAssert.Exists(xmlFilePath);
        }

        [Test]
        public void Test()
        {
            Console.WriteLine(ProjectDir.FullName);
        }

        private DirectoryInfo ProjectDir =>
            new DirectoryInfo(TestContext.CurrentContext.TestDirectory)
                .Parent
                .Parent;

        private string ProjectTopDirectory
        {
            get
            {
                OrigamSettings settings =
                    ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
                return settings.ModelSourceControlLocation;
            }
        }


        private static T GetItemById<T>(Guid id) 
        {
            return (T)ServiceManager
                .Services
                .GetService<IPersistenceService>()
                .SchemaProvider
                .RetrieveInstance(typeof(T),  new Key {{"Id" , id}});
        }
    }
}