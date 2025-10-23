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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using Origam.DA.Common;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service;

internal interface IBinFileLoader
{
    void LoadInto(ItemTracker itemTracker);
    void Persist(ItemTracker itemTracker);
    void MarkToSave(ItemTracker itemTracker);
    void StopTask();
}

internal class BinFileLoader : IBinFileLoader
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType
    );
    private readonly OrigamFileFactory origamFileFactory;
    private readonly DirectoryInfo topDirectory;
    private readonly FileInfo indexFile;
    private readonly object Lock = new object();
    private readonly Queue<ItemTracker> fileSaveQueue = new Queue<ItemTracker>();
    private readonly FilePersistenceIndex filePersistenceIndex;
    private readonly Task task;
    private readonly CancellationTokenSource IndexTaskCancellationTokenSource =
        new CancellationTokenSource();

    public BinFileLoader(
        OrigamFileFactory origamFileFactory,
        DirectoryInfo topDirectory,
        FileInfo indexFile,
        FilePersistenceIndex filePersistenceIdx
    )
    {
        this.origamFileFactory = origamFileFactory;
        this.topDirectory = topDirectory;
        this.indexFile = indexFile;
        this.filePersistenceIndex = filePersistenceIdx;
        var cancellationToken = IndexTaskCancellationTokenSource.Token;
        task = Task.Factory.StartNew(() => SaveIndex(cancellationToken), cancellationToken);
    }

    private void SaveIndex(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (fileSaveQueue.Count > 0)
            {
                QueueOperation(Operation.clear, null);
                filePersistenceIndex.PersistActualIndex(this);
            }
            for (int i = 0; i < 10; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(200);
            }
        }
    }

    private enum Operation
    {
        clear,
        add,
    }

    private void QueueOperation(Operation operation, ItemTracker itemTracker)
    {
        lock (fileSaveQueue)
        {
            switch (operation)
            {
                case Operation.clear:
                {
                    fileSaveQueue.Clear();
                    break;
                }

                case Operation.add:
                {
                    fileSaveQueue.Enqueue(itemTracker);
                    break;
                }
            }
        }
    }

    public void Persist(ItemTracker itemTracker)
    {
#if DEBUG
        // The CheckDataConsistency method was originally a debugging tool.
        // Running this method on a medium size project takes about 300 ms
        // which seemed like a lot for something that is not necessary anymore.
        // That is why was this call removed from the production builds.
        CheckDataConsistency(itemTracker);
#endif
        itemTracker.CleanUp();
        var serializationData = new TrackerSerializationData(
            itemTracker.AllFiles,
            itemTracker.GetStats()
        );
        using (var file = indexFile.Create())
        {
            Serializer.Serialize(file, serializationData);
        }
    }

    public void MarkToSave(ItemTracker itemTracker)
    {
        QueueOperation(Operation.add, itemTracker);
    }

    public void LoadInto(ItemTracker itemTracker)
    {
        if (!indexFile.ExistsNow())
        {
            return;
        }

        lock (Lock)
        {
            bool indexFileCompatible = true;
            try
            {
                LoadInternal(itemTracker);
            }
            catch (Exception exception)
            {
                indexFileCompatible = false;
                log.Warn($"Could not read index file: {indexFile}. Removing it...", exception);
            }
            if (!indexFileCompatible)
            {
                try
                {
                    itemTracker.Clear();
                    indexFile.Delete();
                    log.Warn($"The index file was removed, no data were read from it.");
                }
                catch (IOException ex)
                {
                    log.LogOrigamError($"Could not remove {indexFile}", ex);
                }
            }
        }
    }

    private void LoadInternal(ItemTracker itemTracker)
    {
        var serializationData = new TrackerSerializationData(
            new List<ITrackeableFile>(),
            new Dictionary<string, int>()
        );
        using (var file = indexFile.OpenRead())
        {
            serializationData = Serializer.Deserialize<TrackerSerializationData>(file);
        }

        bool containsIncompatibleClasses = serializationData.PersistedTypeInfos.Any(info =>
            Versions.TryGetCurrentClassVersion(info.FullTypeName) != info.Version
        );
        if (containsIncompatibleClasses)
        {
            return;
        }
        serializationData
            .GetOrigamFiles(origamFileFactory)
            .ForEach(x =>
            {
                itemTracker.AddOrReplace(x);
                itemTracker.AddOrReplaceHash(x);
            });
        if (log.IsDebugEnabled)
        {
            log.RunHandled(() =>
            {
                log.Debug(
                    $"Loaded index file: {indexFile}, last modified: {indexFile.LastWriteTime}, "
                        + "tracker stats:\n"
                        + itemTracker.GetStats()
                );
            });
        }
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (settings.CheckFileHashesAfterModelLoad && !itemTracker.IsEmpty)
        {
            CheckItemTrackerDataIsConsistentOrThrow(itemTracker, serializationData);
            if (!HashesAreUpToDate(itemTracker))
            {
                itemTracker.Clear();
            }
        }
    }

    private bool HashesAreUpToDate(ItemTracker itemTracker)
    {
        var trackerFiles = itemTracker.AllFiles.ToDictionary(
            origamFile => origamFile.Path.Absolute,
            origamFile => origamFile
        );
        IEnumerable<FileInfo> fileInfos = topDirectory
            .GetAllFilesInSubDirectories()
            .AsParallel()
            .Where(OrigamFile.IsPersistenceFile);
        foreach (var file in fileInfos)
        {
            if (!trackerFiles.ContainsKey(file.FullName))
            {
                return false;
            }

            string hash = file.GetFileBase64Hash();
            if (trackerFiles[file.FullName].FileHash != hash)
            {
                return false;
            }

            trackerFiles.Remove(file.FullName);
        }
        return trackerFiles.Count == 0;
    }

    private void CheckItemTrackerDataIsConsistentOrThrow(
        ItemTracker itemTracker,
        TrackerSerializationData serializationData
    )
    {
        Dictionary<string, int> newStats = itemTracker.GetStats();
        Dictionary<string, int> loadedStats = serializationData.ItemTrackerStats;
        if (!AreTrackerStatsEqual(newStats, loadedStats))
        {
            HandleTrackerStatsAreNotEqual(newStats, loadedStats);
        }
    }

    private bool AreTrackerStatsEqual(
        Dictionary<string, int> newStats,
        Dictionary<string, int> loadedStats
    )
    {
        foreach (string statName in newStats.Keys)
        {
            if (newStats[statName] != loadedStats[statName])
            {
                return false;
            }
        }
        return true;
    }

    private void HandleTrackerStatsAreNotEqual(
        Dictionary<string, int> newStats,
        Dictionary<string, int> loadedStats
    )
    {
        log.Error(
            $"Error when loading index file:{indexFile}, data is inconsistent."
                + " ItemTracker stats now: "
                + newStats.Print(inLine: true)
                + "ItemTracker stats in file: "
                + loadedStats.Print(inLine: true)
        );
        throw new Exception("Error when loading index file, data is inconsistent.");
    }

    private void CheckDataConsistency(ItemTracker originalTracker)
    {
        ItemTracker testTracker = new ItemTracker(null);
        originalTracker.AllFiles.ForEach(x =>
        {
            testTracker.AddOrReplace(x);
            testTracker.AddOrReplaceHash(x);
        });
        if (!AreTrackerStatsEqual(testTracker.GetStats(), originalTracker.GetStats()))
        {
            SaveItemTrackersForDebugging(originalTracker, testTracker);
            HandleTrackerStatsAreNotEqual(testTracker.GetStats(), originalTracker.GetStats());
        }
    }

    private void SaveItemTrackersForDebugging(ItemTracker originalTracker, ItemTracker testTracker)
    {
        FileInfo originalTrackerTxt = indexFile.MakeNew("originalTracker");
        File.WriteAllText(originalTrackerTxt.FullName, originalTracker.Print());

        FileInfo testTrackerTxt = indexFile.MakeNew("testTracker");
        File.WriteAllText(testTrackerTxt.FullName, testTracker.Print());
    }

    private void SaveIndexToTxtForDebugging(TrackerSerializationData serializationData)
    {
        FileInfo txtFileInfo = indexFile.MakeNew("debug");
        File.WriteAllText(txtFileInfo.FullName, serializationData.ToString());
    }

    public void StopTask()
    {
        IndexTaskCancellationTokenSource.Cancel();
        try
        {
            task.Wait();
        }
        catch (AggregateException ex)
        {
            var exceptions = ex.Flatten().InnerExceptions.ToList();
            if (exceptions.Count != 1 || exceptions[0] is not TaskCanceledException)
            {
                throw;
            }
        }
    }
}

class NullBinFileLoader : IBinFileLoader
{
    public void MarkToSave(ItemTracker itemTracker) { }

    public void LoadInto(ItemTracker itemTracker) { }

    public void Persist(ItemTracker itemTracker) { }

    public void StopTask() { }
}
