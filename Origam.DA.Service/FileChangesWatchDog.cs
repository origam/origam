using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using System.Reflection;

namespace Origam.DA.Service
{
    public interface IFileChangesWatchDog
    {
        event EventHandler<ChangedFileEventArgs> FileChanged;
        void Start();
        void Stop();
    }

    public class NullWatchDog : IFileChangesWatchDog
    {
        public event EventHandler<ChangedFileEventArgs> FileChanged;
        public void Start()
        {
        }

        public void Stop()
        { 
        }
    }

    public class FileChangesWatchDog : IFileChangesWatchDog
    {
        private static readonly ILog log 
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DirectoryInfo topDir;
        private readonly IEnumerable<string> fileExtensionsToIgnore;
        private readonly IEnumerable<FileInfo> filesToIgnore;
        private readonly IEnumerable<string> directoryNamesToIgnore;
        private FileSystemWatcher watcher;
        private HashSet<string> IgnorePaths { get;} 
            = new HashSet<string>();

        public FileChangesWatchDog(
            DirectoryInfo topDir,
            IEnumerable<string> fileExtensionsToIgnore, 
            IEnumerable<FileInfo> filesToIgnore,
            IEnumerable<string> directoryNamesToIgnore)
        {
            this.fileExtensionsToIgnore = fileExtensionsToIgnore;
            this.topDir = topDir;
            this.filesToIgnore = filesToIgnore;
            this.directoryNamesToIgnore = directoryNamesToIgnore;
        }

        public event EventHandler<ChangedFileEventArgs> FileChanged;
        
        public void Start()
        {
            watcher = new FileSystemWatcher
            {
                Path = topDir.FullName,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName 
            };
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (ShouldBeIgnored(e.FullPath)) return;
            
            FileChanged?.Invoke(
                null, new ChangedFileEventArgs(e.FullPath, null, e.ChangeType));
        }

        private bool ShouldBeIgnored(string fullPath) =>
            HasIgnoredExtension(fullPath) ||
            IsIgnoredFile(fullPath) ||
            IsInIgnoredDirectory(fullPath);

        private bool HasIgnoredExtension(string fullPath)
        {
            string extension = Path.GetExtension(fullPath);
            if (extension.StartsWith("."))
            {
                extension = extension.Substring(1);
            }
            return fileExtensionsToIgnore.Any(ext => ext == extension);
        }

        private bool IsIgnoredFile(string fullPath)
        {
            return filesToIgnore.Any(f => f.FullName == fullPath);
        }

        private bool IsInIgnoredDirectory(string fullPath)
        {
            return fullPath
                .Split(Path.DirectorySeparatorChar)
                .Any(dirName => directoryNamesToIgnore.Contains(dirName));
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (ShouldBeIgnored(e.FullPath)) return;
            
            FileChanged?.Invoke(
                null, 
                new ChangedFileEventArgs(
                    e.OldFullPath, e.FullPath, e.ChangeType));
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnChanged;
            watcher.Created -= OnChanged;
            watcher.Deleted -= OnChanged;
            watcher.Renamed -= OnRenamed;
        }
    }
    public class ChangedFileEventArgs : EventArgs
    {
        public FileInfo File { get; }
        public string OldFilename { get; }
        public WatcherChangeTypes ChangeType { get; }
        public DateTime Timestamp { get; }
        internal ChangedFileEventArgs(
            string filename, string oldFilename, 
            WatcherChangeTypes changeType)
        {
            File = new FileInfo(filename);
            OldFilename = oldFilename;
            ChangeType = changeType;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return "File: " + File + Environment.NewLine 
                + "Change Type: " + ChangeType;
        }
    }
}