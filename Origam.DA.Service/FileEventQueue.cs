using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Extensions;
using Origam.Workbench.Services;
using System.Threading;
using System.Timers;
using log4net;
using Origam.Services;

namespace Origam.DA.Service
{
    public class FileEventQueue: IDisposable
    {
        private static readonly ILog log
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FilePersistenceIndex index;
        private IFileChangesWatchDog watchDog;
        private readonly Queue<FileSystemChangeEventArgs> fileChangeQueue 
            = new Queue<FileSystemChangeEventArgs>();
        private FileSystemChangeEventArgs lastChange = null;
        private bool processEvents = true;
        private readonly object lockObj = new object();
        private System.Timers.Timer timer = new System.Timers.Timer();

        public event EventHandler<FileSystemChangeEventArgs> ReloadNeeded;

        public FileEventQueue(FilePersistenceIndex index, IFileChangesWatchDog watchDog)
        {
            this.index = index;
            this.watchDog = watchDog;
            this.watchDog.FileChanged += (sender, args) =>
            {
                lock (lockObj)
                {
                    fileChangeQueue.Enqueue(args);
                    lastChange = args;
                }
            };
            timer.Interval = 1000;
            timer.Elapsed += OnTimerOnElapsed;
        }

        private void OnTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            TimerElapsedHandler();
        }

        public void Start()
        {
            watchDog.Start();
            timer.Start();
        }

        public void Stop()
        {
            watchDog.Stop();
            timer.Stop();
        }

        /// Underlying timer will not process elapsed events.
        public void Pause()
        {
            lock (lockObj)
            {
                processEvents = false;
            }
        }

        /// Underlying timer will process elapsed events.
        public void Continue()
        {
            lock (lockObj)
            {
                processEvents = true;
            }
        }

        private void TimerElapsedHandler()
        {
            try
            {
                lock (lockObj)
                {
                    DateTime now = DateTime.Now;
                    if (!processEvents 
                    || (lastChange == null) 
                    || ((now - lastChange.Timestamp).TotalSeconds < 1))
                    {
                        return;
                    }
                    ProcessAccumulatedEvents();
                }

            }
            catch (Exception ex)
            {
                ThreadPool.QueueUserWorkItem(_ => {
                    throw new Exception("Exception on timer", ex); });
            }
        }

        private void ProcessAccumulatedEvents()
        {
            while (fileChangeQueue.Count > 0)
            {
                FileSystemChangeEventArgs eventArgs = fileChangeQueue.Dequeue();
                if (ShouldBeUpdated(eventArgs.File) || IsDirectoryChangeAndNeedsUpdate(eventArgs))
                {
                    ReloadNeeded?.Invoke(this, eventArgs);
                    fileChangeQueue.Clear();
                }
            }
            lastChange = null;
        }

        private bool IsDirectoryChangeAndNeedsUpdate(FileSystemChangeEventArgs eventArgs)
        {
            if(!eventArgs.IsDirectoryChange) return false;
            if (FilesInIndexNeedUpdate(eventArgs)) return true;
            if (ExistingFilesNeedUpdate(eventArgs)) return true;
            return false;
        }

        private bool ExistingFilesNeedUpdate(FileSystemChangeEventArgs eventArgs)
        {
            return eventArgs.Folder
                .GetAllFilesInSubDirectories()
                .Any(ShouldBeUpdated);
        }

        private bool FilesInIndexNeedUpdate(FileSystemChangeEventArgs eventArgs)
        {
           return index
                .GetByDirectory(eventArgs.Folder)
                .Any(ShouldBeUpdated);
        }

        private bool ShouldBeUpdated(FileInfo file)
        {
            bool needsUpdate = NeedsUpdate(file);
            if (needsUpdate && log.IsInfoEnabled)
            {
                log.Info("Reload caused by: " + file.FullName );
            }
            return needsUpdate;
        }

        private bool NeedsUpdate(FileInfo file)
        {
            Maybe<string> maybeHash = FindPersistenceFileHash(file);

            file.Refresh();
            if (file.Exists)
            {
                if (maybeHash.HasNoValue) return true;
                if (maybeHash.Value != file.GetFileBase64Hash())
                {
                    return true;
                }
            }

            if (!file.Exists && maybeHash.HasValue) return true;
            return false;
        }

        private Maybe<string> FindPersistenceFileHash(FileInfo file)
        {
            if (OrigamFile.IsPersistenceFile(file))
            {
                return index.GetFileHash(file);
            }
            Maybe<ExternalFile> mayBeExtFile = index.GetExternalFile(file);
            if (mayBeExtFile.HasValue) return mayBeExtFile.Value?.FileHash;

            IFileStorageDocumentationService documentationService =
                (IFileStorageDocumentationService)ServiceManager.Services
                    .GetService<IDocumentationService>();
            return documentationService.GetDocumentationFileHash(file);
        }

        public void Dispose()
        {
            timer.Elapsed -= OnTimerOnElapsed;
            index?.Dispose();
            timer?.Dispose();
        }
    }
}