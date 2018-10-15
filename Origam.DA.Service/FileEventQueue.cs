using System;
using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence.Providers;
using Origam.Extensions;
using Origam.Workbench.Services;
using System.Threading;
using Origam.Services;

namespace Origam.DA.Service
{
    public class FileEventQueue
    {
        private readonly FilePersistenceIndex index;
        private IFileChangesWatchDog watchDog;
        private readonly Queue<ChangedFileEventArgs> fileChangeQueue 
            = new Queue<ChangedFileEventArgs>();
        private ChangedFileEventArgs lastChange = null;
        private bool processEvents = true;
        private readonly object lockObj = new object();
        private System.Timers.Timer timer = new System.Timers.Timer();
        public event EventHandler<ChangedFileEventArgs> ReloadNeeded;

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
            timer.Elapsed += (sender, e) => TimerElapsedHandler();
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
                ChangedFileEventArgs eventArgs = fileChangeQueue.Dequeue();
                if (NeedsUpdate(eventArgs.File))
                {
                    ReloadNeeded?.Invoke(this, eventArgs);
                    fileChangeQueue.Clear();
                }
            }
            lastChange = null;
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
            if (OrigamFile.IsOrigamFile(file))
            {
                return index.GetFileHash(file);
            }
            Maybe<ExternalFile> mayBeExtFile = index.GetExternalFile(file);
            if (mayBeExtFile.HasValue) return mayBeExtFile.Value?.FileHash;

            var documentationService = 
                (IFileStorageDocumentationService)ServiceManager.Services
                    .GetService<IDocumentationService>();
            return documentationService.GetDocumentationFileHash(file);
        }
        
    }
}