using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Origam.TestCommon;

namespace Origam.Common_net2Tests.Properties
{
    [TestFixture]
    public class OrigamSettingsReaderTests: AbstractFileTestClass
    {
        [Test]
        public void ShouldReadAllSettings()
        {
            string pathToOrigamSettings = Path.Combine(TestFilesDir.FullName, "OrigamSettings.config");
            OrigamSettingsCollection settings = 
                new OrigamSettingsReader(pathToOrigamSettings).GetAll();
            
            Assert.That(settings, Has.Count.EqualTo(2));
            var firstSettings = settings[0];
            Assert.That(firstSettings.BaseFolder, Is.EqualTo(@"C:\Test\Test\"));
            Assert.That(firstSettings.SchemaConnectionString, Is.Empty);
            Assert.That(firstSettings.ModelSourceControlLocation, Is.EqualTo(@"C:\Test\Test1\"));
            Assert.That(firstSettings.DataConnectionString, Is.EqualTo(@"Data Source=.;Initial Catalog=test_data;Integrated Security=True;User ID=;Password=;Pooling=True"));
            Assert.That(firstSettings.SchemaDataService, Is.EqualTo("Origam.DA.Service.MsSqlDataService, Origam.DA.Service"));
            Assert.That(firstSettings.DataDataService, Is.EqualTo("Origam.DA.Service.MsSqlDataService, Origam.DA.Service"));
            Assert.That(firstSettings.SecurityDomain, Is.Empty);
            Assert.That(firstSettings.ReportConnectionString, Is.Empty);
            Assert.That(firstSettings.PrintItServiceUrl, Is.Empty);
            Assert.That(firstSettings.SQLReportServiceUrl, Is.Empty);
            Assert.That(firstSettings.SQLReportServiceAccount, Is.Empty);
            Assert.That(firstSettings.SQLReportServicePassword, Is.Empty);
            Assert.That(firstSettings.SQLReportServiceTimeout, Is.EqualTo(60000));
            Assert.That(firstSettings.GUIExcelExportFormat, Is.EqualTo("XLS"));
            Assert.That(firstSettings.DefaultSchemaExtensionId, Is.EqualTo(new Guid("3e37fa44-cdb7-4804-8176-8df118a918ae")));
            Assert.That(firstSettings.ExtraSchemaExtensionId, Is.EqualTo(Guid.Empty));
            Assert.That(firstSettings.TitleText, Is.EqualTo("test"));
            Assert.That(firstSettings.Slogan, Is.Empty);
            Assert.That(firstSettings.LocalizationFolder, Is.Empty);
            Assert.That(firstSettings.TranslationBuilderLanguages, Is.Empty);
            Assert.That(firstSettings.HelpUrl, Is.EqualTo("http://wiki.simplicor.com"));
            Assert.That(firstSettings.ModelProvider, Is.EqualTo("Origam.OrigamEngine.FilePersistenceBuilder, Origam.OrigamEngine"));
            Assert.That(firstSettings.AuthorizationProvider, Is.EqualTo("Origam.Security.OrigamDatabaseAuthorizationProvider, Origam.Security"));
            Assert.That(firstSettings.ProfileProvider, Is.EqualTo("Origam.Security.OrigamProfileProvider, Origam.Security"));
        }

        [Test]
        public void ShouldWriteSettings()
        {
            string pathToReadFrom = Path.Combine(TestFilesDir.FullName,"OrigamSettings.config");
            string pathToWriteTo = Path.Combine(TestFilesDir.FullName,"OrigamSettingsWriteTest.config");

            OrigamSettingsCollection settings =
                new OrigamSettingsReader(pathToReadFrom).GetAll();

            OrigamSettings clone = (OrigamSettings)settings[0].Clone();
            clone.Name = "New Settings";

            settings.Add(clone);
            new OrigamSettingsReader(pathToWriteTo).Write(settings);

            Assert.IsTrue(File.Exists(pathToWriteTo));

            OrigamSettingsCollection settingsRedFromTestFile =
                new OrigamSettingsReader(pathToWriteTo).GetAll();

            Assert.That(settingsRedFromTestFile, Has.Count.EqualTo(3));

        }
        
        [Test] public void ShouldFailWhenArrayOfOrigamSettingsIsMissing()
        {
            var exception = Assert.Throws<OrigamSettingsException>(() =>
            {
                string pathToOrigamSettings = Path.Combine(TestFilesDir.FullName, "OrigamSettingsWithArrayOfOrigamSettingsMissing.config");
                OrigamSettingsCollection settings =
                    new OrigamSettingsReader(pathToOrigamSettings).GetAll();
            });
            Assert.That(exception.Message, Is.EqualTo("Cannot read OrigamSettings.config... Cannot read OrigamSettings.config... Could not find path \"OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings\""));
        }

        protected override TestContext TestContext =>
            TestContext.CurrentContext;
    }
}