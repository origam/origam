using System;
using System.IO;
using NUnit.Framework;

namespace Origam.Common_net2Tests;

[TestFixture]
[NonParallelizable]
public class ConfigurationManagerTests
{
    private string _settingsPath;
    private string _originalSettingsPath;
    private OrigamSettings _originalActiveConfiguration;

    [SetUp]
    public void SetUp()
    {
        _originalSettingsPath = ConfigurationManager.GetPathOrigamSettings();
        _originalActiveConfiguration = ConfigurationManager.GetActiveConfiguration();
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        _settingsPath = Path.Combine(directory, "OrigamSettings.config");
    }

    [TearDown]
    public void TearDown()
    {
        ConfigurationManager.SetPathOrigamSettings(_originalSettingsPath);
        ConfigurationManager.SetActiveConfiguration(_originalActiveConfiguration);
        Directory.Delete(Path.GetDirectoryName(_settingsPath), recursive: true);
    }

    [TestCase(null)]
    [TestCase("")]
    public void LoadsOnlyConfigurationWhenNoNameIsSpecified(string configurationName)
    {
        WriteConfigurations("Only");

        var configuration = ConfigurationManager.GetConfiguration(configurationName, _settingsPath);

        Assert.That(configuration.Name, Is.EqualTo("Only"));
    }

    [TestCase(0)]
    [TestCase(2)]
    public void RequiresExactlyOneConfigurationWhenNoNameIsSpecified(int count)
    {
        WriteConfigurations(count == 0 ? Array.Empty<string>() : new[] { "First", "Second" });

        Assert.Throws<Exception>(() =>
            ConfigurationManager.GetConfiguration(origamSettingsPath: _settingsPath)
        );
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SelectsNamedConfiguration(bool useUserHomeConfiguration)
    {
        WriteConfigurations("First", "Second");

        var configuration = useUserHomeConfiguration
            ? ConfigurationManager.GetUserHomeConfiguration("Second", _settingsPath)
            : ConfigurationManager.GetConfiguration("Second", _settingsPath);

        Assert.That(configuration.Name, Is.EqualTo("Second"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RejectsUnknownConfigurationName(bool useUserHomeConfiguration)
    {
        WriteConfigurations("First");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (useUserHomeConfiguration)
            {
                ConfigurationManager.GetUserHomeConfiguration("Missing", _settingsPath);
            }
            else
            {
                ConfigurationManager.GetConfiguration("Missing", _settingsPath);
            }
        });
    }

    [Test]
    public void UserHomeConfigurationDefaultsToFirstEntry()
    {
        WriteConfigurations("First", "Second");

        var configuration = ConfigurationManager.GetUserHomeConfiguration(
            origamSettingsPath: _settingsPath
        );

        Assert.That(configuration.Name, Is.EqualTo("First"));
    }

    [Test]
    public void UserHomeConfigurationRejectsEmptySettings()
    {
        WriteConfigurations();

        Assert.Throws<Exception>(() =>
            ConfigurationManager.GetUserHomeConfiguration(origamSettingsPath: _settingsPath)
        );
    }

    [Test]
    public void LoadingUsesConfiguredPathWithoutChangingActiveConfiguration()
    {
        WriteConfigurations("Loaded");
        ConfigurationManager.SetPathOrigamSettings(_settingsPath);
        var activeConfiguration = new OrigamSettings { Name = "Active" };
        ConfigurationManager.SetActiveConfiguration(activeConfiguration);

        var configuration = ConfigurationManager.GetConfiguration();

        Assert.That(configuration.Name, Is.EqualTo("Loaded"));
        Assert.That(ConfigurationManager.GetActiveConfiguration(), Is.SameAs(activeConfiguration));
    }

    private void WriteConfigurations(params string[] names)
    {
        var configurations = new OrigamSettingsCollection();
        foreach (string name in names)
        {
            configurations.Add(new OrigamSettings { Name = name });
        }
        new OrigamSettingsReader(_settingsPath).Write(configurations);
    }
}
