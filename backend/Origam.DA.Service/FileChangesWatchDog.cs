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
using System.IO;
using log4net;
using System.Reflection;

namespace Origam.DA.Service;
public interface IFileChangesWatchDog
{
    event EventHandler<FileSystemChangeEventArgs> FileChanged;
    void Start();
    void Stop();
}
public class NullWatchDog : IFileChangesWatchDog
{
    public event EventHandler<FileSystemChangeEventArgs> FileChanged
    {
        add { }
        remove { }
    }
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
    private readonly FileFilter ignoreFileFilter;
    private FileSystemWatcher watcher;
    public FileChangesWatchDog(
        DirectoryInfo topDir, FileFilter ignoreFileFilter)
    {
        this.topDir = topDir;
        this.ignoreFileFilter = ignoreFileFilter;
    }
    public event EventHandler<FileSystemChangeEventArgs> FileChanged;
    
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
            null, new FileSystemChangeEventArgs(e.FullPath, null, e.ChangeType));
    }
    private bool ShouldBeIgnored(string fullPath) =>
        !ignoreFileFilter.ShouldPass(fullPath);
    private void OnRenamed(object source, RenamedEventArgs e)
    {
        if (ShouldBeIgnored(e.FullPath)) return;
        
        FileChanged?.Invoke(
            null, 
            new FileSystemChangeEventArgs(
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
public class FileSystemChangeEventArgs : EventArgs
{
    private readonly string path;
    public bool IsDirectoryChange =>
        ChangeType == WatcherChangeTypes.Changed &&
        Directory.Exists(path);
    public DirectoryInfo Folder { get;}
    public FileInfo File { get; }
    public string OldFilename { get; }
    public WatcherChangeTypes ChangeType { get; }
    public DateTime Timestamp { get; }
    internal FileSystemChangeEventArgs(
        string path, string oldFilename, 
        WatcherChangeTypes changeType)
    {
        this.path = path;
        Folder = new DirectoryInfo(path);
        File = new FileInfo(path);
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
