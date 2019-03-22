#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using MoreLinq;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service
{
    internal interface IBinFileLoader
    {
        void LoadInto(ItemTracker itemTracker);
        void Persist(ItemTracker itemTracker);
    }

    internal class BinFileLoader : IBinFileLoader
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);
        private readonly OrigamFileFactory origamFileFactory;
        private readonly DirectoryInfo topDirectory;
        private readonly FileInfo indexFile;
        private readonly object Lock = new object();

        public BinFileLoader(OrigamFileFactory origamFileFactory,
            DirectoryInfo topDirectory, FileInfo indexFile)
        {
            this.origamFileFactory = origamFileFactory;
            this.topDirectory = topDirectory;
            this.indexFile = indexFile;
        }

        public void LoadInto(ItemTracker itemTracker)
        {
            if (!indexFile.ExistsNow()) return;
            lock (Lock)
            {
                TrackerSerializationData serializationData;
                using (var file = indexFile.OpenRead())
                {
                    try
                    {
                        serializationData = Serializer
                            .Deserialize<TrackerSerializationData>(file);
                    } 
                    catch (ProtoException ex)
                    {
                        throw new Exception(
                            $"Could not read index file: {indexFile}. Maybe it is damaged, try removing it.",ex);
                    }
                }
                serializationData.GetOrigamFiles(origamFileFactory)
                    .ForEach(x =>
                    {
                        itemTracker.AddOrReplace(x);
                        itemTracker.AddOrReplaceHash(x);
                    });

                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        $"Loaded index file: {indexFile}, last modified: {indexFile.LastWriteTime}, " +
                        ", tracker stats:\n" +
                        itemTracker.GetStats());
                }
                CheckItemTrackerDataIsConsistentOrThrow(itemTracker, serializationData);
                if (!HashesAreUpToDate(itemTracker))
                {
                    itemTracker.Clear();
                }
            }
        }

        private bool HashesAreUpToDate(ItemTracker itemTracker)
        {
            var trackerFiles = itemTracker
                .AllFiles
                .ToDictionary(
                    origamFile => origamFile.Path.Absolute, 
                    origamFile => origamFile);

            IEnumerable<FileInfo> fileInfos = topDirectory
                .GetAllFilesInSubDirectories()
                .AsParallel()
                .Where(OrigamFile.IsPersistenceFile)
                .Where(file => file.Name != OrigamFile.ReferenceFileName);

            foreach (var file in fileInfos)
            {
                if (!trackerFiles.ContainsKey(file.FullName)) return false;
                string hash = file.GetFileBase64Hash();
                if (trackerFiles[file.FullName].FileHash != hash) return false;
                trackerFiles.Remove(file.FullName);
            }

            return trackerFiles.Count == 0;
        }

        private void CheckItemTrackerDataIsConsistentOrThrow(ItemTracker itemTracker,
            TrackerSerializationData serializationData)
        {
            Dictionary<string, int> newStats = itemTracker.GetStats();
            Dictionary<string, int> loadedStats = serializationData.ItemTrackerStats;
            if (!AreTrackerStatsAreEqual(newStats, loadedStats))
            {
                HandleTrackerStatsAreNotEqual(newStats, loadedStats);
            }
        }

        private bool AreTrackerStatsAreEqual(Dictionary<string, int> newStats, Dictionary<string, int> loadedStats)
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


        private void HandleTrackerStatsAreNotEqual(Dictionary<string, int> newStats, Dictionary<string, int> loadedStats) {
            log.Error(
                $"Error when loading index file:{indexFile}, data is inconsistent." +
                " ItemTracker stats now: " + newStats.Print(inLine: true) +
                "ItemTracker stats in file: " +
                loadedStats.Print(inLine: true));

            throw new Exception(
                "Error when loading index file, data is inconsistent.");
        }

        public void Persist(ItemTracker itemTracker)
        {
            CheckDataConsistency(itemTracker);
            itemTracker.CleanUp();
            var serializationData = new TrackerSerializationData(
                itemTracker.AllFiles, itemTracker.GetStats());
            using (var file = indexFile.Create())
            {
                Serializer.Serialize(file, serializationData);
            }
        }

        private void CheckDataConsistency(ItemTracker originalTracker)
        {
            ItemTracker testTracker = new ItemTracker(null);
            originalTracker.AllFiles
                   .ForEach(x =>
                   {
                       testTracker.AddOrReplace(x);
                       testTracker.AddOrReplaceHash(x);
                   });
            if (!AreTrackerStatsAreEqual(testTracker.GetStats(), originalTracker.GetStats()))
            {
                SaveItemTrackersForDebugging(originalTracker, testTracker);
                HandleTrackerStatsAreNotEqual(testTracker.GetStats(), originalTracker.GetStats());
            }
        }

        private void SaveItemTrackersForDebugging(ItemTracker originalTracker, ItemTracker testTracker)
        {
            FileInfo originalTrackerTxt = indexFile.MakeNew("originalTracker");
            File.WriteAllText(originalTrackerTxt.FullName,  originalTracker.Print());
            
            FileInfo testTrackerTxt = indexFile.MakeNew("testTracker");
            File.WriteAllText(testTrackerTxt.FullName, testTracker.Print());
        }
        
        private void SaveIndexToTxtForDebugging(
            TrackerSerializationData serializationData)
        {
            FileInfo txtFileInfo = indexFile.MakeNew("debug");
            File.WriteAllText(txtFileInfo.FullName, serializationData.ToString());
        }
    }

    class NullBinFileLoader : IBinFileLoader
    {
        public void LoadInto(ItemTracker itemTracker)
        {   
        }

        public void Persist(ItemTracker itemTracker)
        {
        }
    }
}