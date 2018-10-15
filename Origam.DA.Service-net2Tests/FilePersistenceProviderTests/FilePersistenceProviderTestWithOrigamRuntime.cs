using System;
using System.IO;
using NUnit.Framework;
using Origam.TestCommon;

namespace Origam.DA.Service_net2Tests
{
    /// <summary>
    /// This test reads data from database, saves them to xml, reads them back
    /// and compares the result to the original data. Edit OrigamSettings.config
    /// to change data to work with.
    /// </summary>
    [TestFixture]
    public class FilePersistenceProviderTestWithOrigamRuntime: AbstractFileTestClass
    {
        protected override TestContext TestContext =>
            TestContext.CurrentContext;
//        [Test]
        public void ShouldReadDataFromDataBaseAndOverwriteExistingXmls()
        {           
            OrigamEngine.OrigamEngine.ConnectRuntime(
                "Data for Xml serialization Test");
            Console.WriteLine("OrigamEngine connected");
            var settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
            
            ClearTestDir(settings);

            var persistor = new PersitHelper(settings.ModelSourceControlLocation);
            persistor.PersistAll();
            Console.WriteLine("Test data serialized to XML");
            Console.WriteLine("DONE"); 
        }

        private static void ClearTestDir(OrigamSettings settings)
        {
            foreach (string dir in Directory.EnumerateDirectories(settings
                .ModelSourceControlLocation))
            {
                Directory.Delete(dir, true);
            }
            foreach (string file in Directory.EnumerateFiles(settings
                .ModelSourceControlLocation))
            {
                File.Delete(file);
            }
        }

//        [Test]
//        public void ShouldReadXmlData()
//        {
//            OrigamEngine.OrigamEngine.ConnectRuntime("Xml Serialized test");
//            var settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
//            var persistor = new PersitHelper(settings.ModelSourceControlLocation);
//            //List<AbstractSchemaItem> abstractSchemaItems = persistor.RetrieveAll();
//            
////            int itemsWithNopackageCount = abstractSchemaItems   
////                .Count(x => x.SchemaExtensionId == Guid.Empty);
////            Assert.That(itemsWithNopackageCount,Is.EqualTo(0));
////            
//            Console.WriteLine("Xml data red");
//            Console.WriteLine("DONE");
//        }

        protected override string DirName => "FilePersistenceProviderTests";
    }
}