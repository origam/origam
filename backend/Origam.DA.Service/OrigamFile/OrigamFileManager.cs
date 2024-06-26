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
using System.Linq;
using System.Reflection;
using System.Threading;
using Origam.Extensions;

namespace Origam.DA.Service;
public class OrigamFileManager: IDisposable
{
    private static readonly log4net.ILog log
        = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
    private readonly FilePersistenceIndex index;
    private readonly OrigamPathFactory origamPathFactory;
    private readonly FileEventQueue fileEventQueue;
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
        if (Directory.Exists(dirToRename.FullName))
        {
            Directory.Move(dirToRename.FullName, newDirPath);
        }
        index.RenameDirectory(dirToRename, newDirPath);
        fileEventQueue.Continue();
    }
    
    public void WriteToDisc(OrigamFile origamFile, OrigamXmlDocument xmlDocument)
    {
        string xmlToWrite = OrigamDocumentSorter
            .CopyAndSort(xmlDocument)
            .ToBeautifulString();
        fileEventQueue.Pause();
        Directory.CreateDirectory(origamFile.Path.Directory.FullName);
        File.WriteAllText(origamFile.Path.Absolute, xmlToWrite);
        origamFile.UpdateHash();
        index.AddOrReplaceHash(origamFile);
        fileEventQueue.Continue();
    }
    public void RemoveDirectoryIfEmpty(DirectoryInfo oldFullDirectory)
    {
        if (!Directory.Exists(oldFullDirectory.FullName))
        {
            return;
        }
        bool isEmpty = !oldFullDirectory
            .GetAllFilesInSubDirectories()
            .Any();
        if (isEmpty)
        {
            Directory.Delete(oldFullDirectory.FullName, true);
        }
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
    }
}
