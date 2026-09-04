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

namespace Origam.AI.Agent.Tests.Integration;

public sealed class ModelSandbox
{
    private const string SettingsFileName = "OrigamSettings.config";
    private const string SettingsBackupFileName = "OrigamSettings.config.bak";
    private const string SandboxRootFolderName = "origam-ai-benchmark";
    private const string ModelLocationElement = "ModelSourceControlLocation";
    private const string ArchitectUrlVariable = "ORIGAM_ARCHITECT_URL";
    private const string SettingsPathVariable = "ORIGAM_SETTINGS_PATH";
    private const string ArchitectProjectFolderName = "Origam.Architect.Server";

    private static readonly TimeSpan StaleSandboxAge = TimeSpan.FromHours(6);
    private static readonly TimeSpan DeleteRetryDelay = TimeSpan.FromMilliseconds(300);

    private string settingsPath = string.Empty;
    private string settingsBackupPath = string.Empty;
    private bool settingsWereBorrowed;
    private DirectoryInfo? sandboxRunDirectory;
    private EventHandler? processExitHandler;

    public void Create()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ArchitectUrlVariable)))
        {
            return;
        }

        settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        settingsBackupPath = Path.Combine(AppContext.BaseDirectory, SettingsBackupFileName);
        RestoreSettings();

        var source = FindUsableSettings();
        if (source is null)
        {
            Assert.Ignore(
                $"Found no {SettingsFileName} pointing at an existing model. Looked next to the "
                    + $"test assembly and in {ArchitectProjectFolderName}\\bin; point "
                    + $"{SettingsPathVariable} at one to override."
            );
            return;
        }

        var (sourceSettingsPath, settings) = source.Value;
        var modelLocations = settings.Descendants(ModelLocationElement).ToList();
        var sourceModel = new DirectoryInfo(modelLocations[0].Value);

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

        if (string.Equals(sourceSettingsPath, settingsPath, StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(settingsPath, settingsBackupPath, overwrite: true);
        }
        else
        {
            settingsWereBorrowed = true;
            TestContext.Progress.WriteLine($"settings borrowed from: {sourceSettingsPath}");
        }
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

    public void Remove()
    {
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
        if (!string.IsNullOrEmpty(settingsBackupPath) && File.Exists(settingsBackupPath))
        {
            File.Copy(settingsBackupPath, settingsPath, overwrite: true);
            File.Delete(settingsBackupPath);
            return;
        }

        if (settingsWereBorrowed && File.Exists(settingsPath))
        {
            File.Delete(settingsPath);
            settingsWereBorrowed = false;
        }
    }

    private static (string Path, XDocument Settings)? FindUsableSettings()
    {
        foreach (var candidate in SettingsCandidates())
        {
            var settings = LoadSettingsWithAnExistingModel(candidate);
            if (settings is not null)
            {
                return (candidate, settings);
            }
        }

        return null;
    }

    private static IEnumerable<string> SettingsCandidates()
    {
        var fromVariable = Environment.GetEnvironmentVariable(SettingsPathVariable);
        if (!string.IsNullOrWhiteSpace(fromVariable))
        {
            yield return Directory.Exists(fromVariable)
                ? Path.Combine(fromVariable, SettingsFileName)
                : fromVariable;
            yield break;
        }

        yield return Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        foreach (var fromArchitectOutput in ArchitectOutputSettings())
        {
            yield return fromArchitectOutput;
        }
    }

    private static IEnumerable<string> ArchitectOutputSettings()
    {
        for (
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            folder is not null;
            folder = folder.Parent
        )
        {
            var architectOutput = new DirectoryInfo(
                Path.Combine(folder.FullName, ArchitectProjectFolderName, path3: "bin")
            );
            if (!architectOutput.Exists)
            {
                continue;
            }

            return architectOutput
                .GetFiles(SettingsFileName, SearchOption.AllDirectories)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Select(file => file.FullName);
        }

        return [];
    }

    private static XDocument? LoadSettingsWithAnExistingModel(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        XDocument settings;
        try
        {
            settings = XDocument.Load(path);
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }

        var modelLocation = settings.Descendants(ModelLocationElement).FirstOrDefault()?.Value;
        return !string.IsNullOrWhiteSpace(modelLocation) && Directory.Exists(modelLocation)
            ? settings
            : null;
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
