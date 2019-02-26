using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Providers;
using Origam.DA.Service;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class OrigamFileManager: IDisposable
    {
        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FilePersistenceIndex index;
        private readonly OrigamPathFactory origamPathFactory;
        private readonly FileEventQueue fileEventQueue;
        internal event EventHandler<HashChangedEventArgs> HashChanged;

        public OrigamFileManager(FilePersistenceIndex index,
            OrigamPathFactory origamPathFactory,  FileEventQueue fileEventQueue)
        {
            this.index = index;
            this.origamPathFactory = origamPathFactory;
            this.fileEventQueue = fileEventQueue;
        }

        public void WriteReferenceFileToDisc(string fullPath, string contents, ParentFolders parentFolderIds)
        {
            if (File.Exists(fullPath)) return;
            OrigamPath path = origamPathFactory.Create(fullPath);
            
            fileEventQueue.Pause();
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            using (StreamWriter sw = File.CreateText(fullPath))
            {
                sw.Write(contents);
            }
            OrigamReferenceFile referenceFile = new OrigamReferenceFile(path, parentFolderIds);
            index.AddOrReplaceHash(referenceFile);
            fileEventQueue.Continue();
        }
        
        public void RenameDirectory(DirectoryInfo dirToRename, string newName)
        {
            string newDirPath =
                Path.Combine(dirToRename.Parent.FullName, newName);
            if (dirToRename.FullName.ToLower() == newDirPath.ToLower()) return;
            fileEventQueue.Pause();
            Directory.Move(dirToRename.FullName, newDirPath);
            index.RenameDirectory(dirToRename, newDirPath);
            fileEventQueue.Continue();
        }
        
        public void WriteToDisc(OrigamFile origamFile, XmlDocument xmlDocument)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true
            };
            
            string xmlToWrite = OrigamDocumentSorter
                .CopyAndSort(xmlDocument)
                .ToBeautifulString(xmlWriterSettings);
            fileEventQueue.Pause();
            Directory.CreateDirectory(origamFile.Path.Directory.FullName);
            File.WriteAllText(origamFile.Path.Absolute, xmlToWrite);
            origamFile.UpdateHash();
            index.AddOrReplaceHash(origamFile);
            fileEventQueue.Continue();
        }

        public void MoveFile(OrigamFile origamFile)
        {
            origamFile.NewPath.Directory.Create();
            string destination = origamFile.NewPath.Absolute;
            string source = origamFile.Path.Absolute;

            fileEventQueue.Pause();
            int numFilesBefore = index.OrigamFiles.Count();
            index.RemoveHash(origamFile);
            index.Remove(origamFile);
            origamFile.Path = origamFile.NewPath;
            origamFile.NewPath = null;
            index.AddOrReplace(origamFile);
            index.AddOrReplaceHash(origamFile);
            File.Move(source, destination); 
            System.Diagnostics.Debug.Assert(numFilesBefore == index.OrigamFiles.Count());
            fileEventQueue.Continue();
        }
            
        public void RemoveDirectoryIfEmpty(DirectoryInfo oldFullDirectory)
        {
            bool isEmpty = !oldFullDirectory
                .GetAllFilesInSubDirectories()
                .Any();
            if (isEmpty)
            {
                Directory.Delete(oldFullDirectory.FullName, true);
            }
        } 
        
        public OrigamPath MakeNewOrigamPath(IFilePersistent instance, bool resolveNamingConflicts)
        {
            string newRelativePath = instance.RelativeFilePath;
            if (index.HasFile(newRelativePath))
            {
                if (!resolveNamingConflicts)
                {
                    throw new InvalidOperationException($"Cannot create path at: {newRelativePath} because another file is already there");
                }
                string newFileName =
                    Path.GetFileNameWithoutExtension(newRelativePath) +
                    "_" +
                    Path.GetExtension(newRelativePath);
                newRelativePath =
                    Path.Combine(Path.GetDirectoryName(newRelativePath), newFileName);
            }
            return origamPathFactory.CreateFromRelative(newRelativePath);
        }

        public void DeleteFile(OrigamFile origamFile)
        {
            fileEventQueue.Pause();
            index.RemoveHash(origamFile);
            index.Remove(origamFile);
            DeleteFile(origamFile.Path.Absolute);
            fileEventQueue.Continue();
        }
        
        
        public void RemoveDirectoryWithContents(DirectoryInfo packageDir)
        {
            fileEventQueue.Pause();
            packageDir.Delete(true);
            fileEventQueue.Continue();
        }

        private void DeleteFile(String fileToDelete)
        {
            FileInfo fi = new FileInfo(fileToDelete);
            if (!fi.Exists) return;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    fi.Delete();
                    break;
                } 
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
            fi.Refresh();
            for (int i = 0; i < 10; i++)
            {
                if (!fi.Exists) return;
                Thread.Sleep(100);
                fi.Refresh();
            }
            throw new Exception($"Cannot remove file {fileToDelete}");
        }

        public void Dispose()
        {
            index?.Dispose();
            fileEventQueue?.Dispose();
            HashChanged = null;
        }
    }

    internal class HashChangedEventArgs : EventArgs
    {
        public string Hash { get;}

        public HashChangedEventArgs(string hash)
        {
            Hash = hash;
        }
    }
}