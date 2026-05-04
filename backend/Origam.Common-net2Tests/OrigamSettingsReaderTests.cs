#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.IO;
using NUnit.Framework;
using Origam.TestCommon;

namespace Origam.Common_net2Tests.Properties;

[TestFixture]
public class OrigamSettingsReaderTests : AbstractFileTestClass
{
    [Test]
    public void ShouldReadAllSettings()
    {
        string pathToOrigamSettings = Path.Combine(
            path1: TestFilesDir.FullName,
            path2: "OrigamSettings.config"
        );
        OrigamSettingsCollection settings = new OrigamSettingsReader(
            pathToOrigamSettings: pathToOrigamSettings
        ).GetAll();

        Assert.That(actual: settings, expression: Has.Count.EqualTo(expected: 2));
        var firstSettings = settings[index: 0];
        //Assert.That(firstSettings.BaseFolder, Is.EqualTo(@"C:\Test\Test\"));
        Assert.That(actual: firstSettings.SchemaConnectionString, expression: Is.Empty);
        Assert.That(
            actual: firstSettings.ModelSourceControlLocation,
            expression: Is.EqualTo(expected: @"C:\Test\Test1\")
        );
        Assert.That(
            actual: firstSettings.DataConnectionString,
            expression: Is.EqualTo(
                expected: @"Data Source=.;Initial Catalog=test_data;Integrated Security=True;User ID=;Password=;Pooling=True"
            )
        );
        Assert.That(
            actual: firstSettings.SchemaDataService,
            expression: Is.EqualTo(
                expected: "Origam.DA.Service.MsSqlDataService, Origam.DA.Service"
            )
        );
        Assert.That(
            actual: firstSettings.DataDataService,
            expression: Is.EqualTo(
                expected: "Origam.DA.Service.MsSqlDataService, Origam.DA.Service"
            )
        );
        Assert.That(actual: firstSettings.SecurityDomain, expression: Is.Empty);
        Assert.That(actual: firstSettings.ReportConnectionString, expression: Is.Empty);
        Assert.That(actual: firstSettings.PrintItServiceUrl, expression: Is.Empty);
        Assert.That(actual: firstSettings.SQLReportServiceUrl, expression: Is.Empty);
        Assert.That(actual: firstSettings.SQLReportServiceAccount, expression: Is.Empty);
        Assert.That(actual: firstSettings.SQLReportServicePassword, expression: Is.Empty);
        Assert.That(
            actual: firstSettings.SQLReportServiceTimeout,
            expression: Is.EqualTo(expected: 60000)
        );
        Assert.That(
            actual: firstSettings.GUIExcelExportFormat,
            expression: Is.EqualTo(expected: "XLS")
        );
        Assert.That(
            actual: firstSettings.DefaultSchemaExtensionId,
            expression: Is.EqualTo(expected: new Guid(g: "3e37fa44-cdb7-4804-8176-8df118a918ae"))
        );
        Assert.That(
            actual: firstSettings.ExtraSchemaExtensionId,
            expression: Is.EqualTo(expected: Guid.Empty)
        );
        Assert.That(actual: firstSettings.TitleText, expression: Is.EqualTo(expected: "test"));
        Assert.That(actual: firstSettings.Slogan, expression: Is.Empty);
        Assert.That(actual: firstSettings.LocalizationFolder, expression: Is.Empty);
        Assert.That(actual: firstSettings.TranslationBuilderLanguages, expression: Is.Empty);
        Assert.That(
            actual: firstSettings.HelpUrl,
            expression: Is.EqualTo(expected: "http://origam.com/doc")
        );
        Assert.That(
            actual: firstSettings.ModelProvider,
            expression: Is.EqualTo(
                expected: "Origam.OrigamEngine.FilePersistenceBuilder, Origam.OrigamEngine"
            )
        );
        Assert.That(
            actual: firstSettings.AuthorizationProvider,
            expression: Is.EqualTo(
                expected: "Origam.Security.OrigamDatabaseAuthorizationProvider, Origam.Security"
            )
        );
        Assert.That(
            actual: firstSettings.ProfileProvider,
            expression: Is.EqualTo(
                expected: "Origam.Security.OrigamProfileProvider, Origam.Security"
            )
        );
    }

    [Test]
    public void ShouldWriteSettings()
    {
        string pathToReadFrom = Path.Combine(
            path1: TestFilesDir.FullName,
            path2: "OrigamSettings.config"
        );
        string pathToWriteTo = Path.Combine(
            path1: TestFilesDir.FullName,
            path2: "OrigamSettingsWriteTest.config"
        );
        OrigamSettingsCollection settings = new OrigamSettingsReader(
            pathToOrigamSettings: pathToReadFrom
        ).GetAll();
        OrigamSettings clone = (OrigamSettings)settings[index: 0].Clone();
        clone.Name = "New Settings";
        settings.Add(value: clone);
        new OrigamSettingsReader(pathToOrigamSettings: pathToWriteTo).Write(
            configuration: settings
        );
        Assert.IsTrue(condition: File.Exists(path: pathToWriteTo));
        OrigamSettingsCollection settingsRedFromTestFile = new OrigamSettingsReader(
            pathToOrigamSettings: pathToWriteTo
        ).GetAll();
        Assert.That(actual: settingsRedFromTestFile, expression: Has.Count.EqualTo(expected: 3));
    }

    [Test]
    public void ShouldFailWhenArrayOfOrigamSettingsIsMissing()
    {
        var exception = Assert.Throws<OrigamSettingsException>(code: () =>
        {
            string pathToOrigamSettings = Path.Combine(
                path1: TestFilesDir.FullName,
                path2: "OrigamSettingsWithArrayOfOrigamSettingsMissing.config"
            );
            OrigamSettingsCollection settings = new OrigamSettingsReader(
                pathToOrigamSettings: pathToOrigamSettings
            ).GetAll();
        });
        Assert.That(
            actual: exception.Message,
            expression: Is.EqualTo(
                expected: "Cannot read OrigamSettings.config... Cannot read OrigamSettings.config... Could not find path \"OrigamSettings/xmlSerializerSection/ArrayOfOrigamSettings\""
            )
        );
    }

    protected override TestContext TestContext => TestContext.CurrentContext;
}
