using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Origam.Extensions;
using ProtoBuf;

namespace Origam.DA.Service
{
    internal class BinFileLoader
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);
        private readonly OrigamFileFactory origamFileFactory;
        private readonly DirectoryInfo topDirectory;
        private readonly FileInfo indexFile;
        private readonly object Log = new object();

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
            lock (Log)
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
            var trackerFiles = itemTracker.OrigamFiles.ToDictionary(
                origamFile => origamFile.Path.Absolute, origamFile => origamFile);

            IEnumerable<FileInfo> fileInfos = topDirectory
                .GetAllFilesInSubDirectories()
                .AsParallel()
                .Where(OrigamFile.IsOrigamFile);

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

            foreach (string statName in newStats.Keys)
            {
                if (newStats[statName] != loadedStats[statName])
                {
                    log.Error(
                        $"Error when loading index file:{indexFile}, data is inconsistent." +
                        " ItemTracker stats now: " + newStats.Print(inLine: true) +
                        "ItemTracker stats in file: " +
                        loadedStats.Print(inLine: true));

                    throw new Exception(
                        "Error when loading index file, data is inconsistent.");
                    // This doesn't mean the serialized data is corrupted. Try running this in Persist method :
                    // var origFiles = itemTracker.OrigamFiles;
                    // itemTracker.Clear();
                    // itemTracker.ForEach(origFiles.AddOrReplace);
                    // and compare the stats to see if the problem is really in the serialization.
                }
            }
        }

        public void Persist(ItemTracker itemTracker)
        {      
            itemTracker.CleanUp();
            var serializationData = new TrackerSerializationData(
                itemTracker.OrigamFiles, itemTracker.GetStats());
            using (var file = indexFile.Create())
            {
                Serializer.Serialize(file, serializationData);
            }

           // SaveItemTrackerToTxtForDebugging(itemTracker);
        }
        
        private void SaveItemTrackerToTxtForDebugging(ItemTracker itemTracker)
        {
            FileInfo txtFileInfo = indexFile.MakeNew("debug");
            File.WriteAllText(txtFileInfo.FullName, itemTracker.Print());
        }
        
        private void SaveIndexToTxtForDebugging(
            TrackerSerializationData serializationData)
        {
            FileInfo txtFileInfo = indexFile.MakeNew("debug");
            File.WriteAllText(txtFileInfo.FullName, serializationData.ToString());
        }
    }
}