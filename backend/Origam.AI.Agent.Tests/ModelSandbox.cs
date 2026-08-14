#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using System.Xml.Linq;
using NUnit.Framework;
using Origam.AI.Agent.Tests.Infrastructure;

namespace Origam.AI.Agent.Tests;

[SetUpFixture]
public sealed class ModelSandbox
{
    private const string SettingsFileName = "OrigamSettings.config";
    private const string SettingsBackupFileName = "OrigamSettings.config.bak";
    private const string SandboxRootFolderName = "origam-ai-benchmark";
    private const string ModelLocationElement = "ModelSourceControlLocation";
    private const string ArchitectUrlVariable = "ORIGAM_ARCHITECT_URL";

    private static readonly TimeSpan StaleSandboxAge = TimeSpan.FromHours(6);
    private static readonly TimeSpan DeleteRetryDelay = TimeSpan.FromMilliseconds(300);

    private string settingsPath = string.Empty;
    private string settingsBackupPath = string.Empty;
    private DirectoryInfo? sandboxRunDirectory;
    private EventHandler? processExitHandler;

    [OneTimeSetUp]
    public void CopyModelIntoSandbox()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ArchitectUrlVariable)))
        {
            return;
        }

        settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        settingsBackupPath = Path.Combine(AppContext.BaseDirectory, SettingsBackupFileName);
        RestoreSettings();

        if (!File.Exists(settingsPath))
        {
            Assert.Ignore($"No {SettingsFileName} next to the test assembly, cannot sandbox it.");
        }

        var settings = XDocument.Load(settingsPath);
        var modelLocations = settings.Descendants(ModelLocationElement).ToList();
        var sourceModel = new DirectoryInfo(modelLocations.FirstOrDefault()?.Value ?? string.Empty);
        if (modelLocations.Count == 0 || !sourceModel.Exists)
        {
            Assert.Ignore(
                $"{ModelLocationElement} in {SettingsFileName} does not point at an existing "
                    + "model, cannot sandbox it."
            );
        }

        DeleteStaleSandboxes();

        sandboxRunDirectory = new DirectoryInfo(
            Path.Combine(
                Path.GetTempPath(),
                SandboxRootFolderName,
                DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..8]
            )
        );
        var sandboxModel = new DirectoryInfo(
            Path.Combine(sandboxRunDirectory.FullName, path2: "model")
        );
        CopyDirectory(sourceModel, sandboxModel);

        File.Copy(settingsPath, settingsBackupPath, overwrite: true);
        processExitHandler = (_, _) => RestoreSettings();
        AppDomain.CurrentDomain.ProcessExit += processExitHandler;

        foreach (var modelLocation in modelLocations)
        {
            modelLocation.Value = sandboxModel.FullName;
        }
        settings.Save(settingsPath);

        TestContext.Progress.WriteLine(
            $"model sandbox: {sourceModel.FullName} -> {sandboxModel.FullName}"
        );
    }

    [OneTimeTearDown]
    public void RemoveSandbox()
    {
        InProcessArchitect.Shutdown();

        if (processExitHandler is not null)
        {
            AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
            processExitHandler = null;
        }
        RestoreSettings();

        if (sandboxRunDirectory is null)
        {
            return;
        }

        if (!TryDeleteDirectory(sandboxRunDirectory))
        {
            TestContext.Progress.WriteLine(
                $"model sandbox left behind, delete it by hand: {sandboxRunDirectory.FullName}"
            );
        }
        sandboxRunDirectory = null;
    }

    private void RestoreSettings()
    {
        if (string.IsNullOrEmpty(settingsBackupPath) || !File.Exists(settingsBackupPath))
        {
            return;
        }

        File.Copy(settingsBackupPath, settingsPath, overwrite: true);
        File.Delete(settingsBackupPath);
    }

    private static void DeleteStaleSandboxes()
    {
        var sandboxRoot = new DirectoryInfo(
            Path.Combine(Path.GetTempPath(), SandboxRootFolderName)
        );
        if (!sandboxRoot.Exists)
        {
            return;
        }

        var deleteOlderThan = DateTime.UtcNow - StaleSandboxAge;
        foreach (var abandonedRun in sandboxRoot.GetDirectories())
        {
            if (abandonedRun.LastWriteTimeUtc < deleteOlderThan)
            {
                TryDeleteDirectory(abandonedRun);
            }
        }
    }

    private static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
    {
        target.Create();
        foreach (var file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(target.FullName, file.Name), overwrite: true);
        }
        foreach (var directory in source.GetDirectories())
        {
            CopyDirectory(
                directory,
                new DirectoryInfo(Path.Combine(target.FullName, directory.Name))
            );
        }
    }

    private static bool TryDeleteDirectory(DirectoryInfo directory)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                directory.Delete(recursive: true);
                return true;
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(DeleteRetryDelay);
            }
        }

        return !directory.Exists;
    }
}
