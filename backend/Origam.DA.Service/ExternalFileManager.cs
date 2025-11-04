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
using CSharpFunctionalExtensions;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;

namespace Origam.DA.Service;

public interface IExternalFileManager
{
    string AddAndReturnLink(
        string fieldName,
        Guid objectId,
        object data,
        ExternalFileExtension fileExtension
    );
}

public class NullExternalFileManager : IExternalFileManager
{
    public string AddAndReturnLink(
        string fieldName,
        Guid objectId,
        object data,
        ExternalFileExtension fileExtension
    )
    {
        return "DummyLink";
    }
}

public class ExternalFileManager : IExternalFileManager
{
    private readonly OrigamFile origamFile;
    private readonly OrigamPathFactory pathFactory;
    private readonly Dictionary<string, ExternalFile> pathFileDict =
        new Dictionary<string, ExternalFile>();
    private readonly FileEventQueue fileEventQueue;
    private readonly Queue<IDeferredTask> deferredTasks = new Queue<IDeferredTask>();

    public ExternalFileManager(
        OrigamFile origamFile,
        OrigamPathFactory pathFactory,
        FileEventQueue fileEventQueue
    )
    {
        this.pathFactory = pathFactory;
        this.origamFile = origamFile;
        this.fileEventQueue = fileEventQueue;
    }

    public IEnumerable<string> Files =>
        pathFileDict.Values.Select(extFile => extFile.ExtFilePath.Absolute);

    public string AddAndReturnLink(
        string fieldName,
        Guid objectId,
        object data,
        ExternalFileExtension fileExtension
    )
    {
        if (data == null)
        {
            return "";
        }

        ExternalFilePath filePath = pathFactory.Create(
            origamFile.Path,
            fieldName,
            objectId,
            fileExtension
        );
        var task = new DeferredWritingTask(filePath, data, AddOrRefreshHash);
        deferredTasks.Enqueue(task);
        return filePath.LinkWithPrefix;
    }

    public void UpdateFilesOnDisc()
    {
        fileEventQueue.Pause();
        while (deferredTasks.Count > 0)
        {
            deferredTasks.Dequeue().Run();
        }
        fileEventQueue.Continue();
    }

    private void AddOrRefreshHash(ExternalFilePath filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath.Absolute);
        string hash = fileInfo.Exists ? fileInfo.GetFileBase64Hash() : "";
        ExternalFile externalFile = new ExternalFile(filePath, hash);
        pathFileDict.AddOrReplace(filePath.Absolute, externalFile);
    }

    public object GetValue(Guid instanceId, string fieldName)
    {
        ExternalFilePath filePath = pathFileDict
            .Values.Select(file => file.ExtFilePath)
            .FirstOrDefault(path => path.OwnerOjectId == instanceId && path.FieldName == fieldName);
        if (filePath == null)
        {
            return null;
        }
        // first try to see if the value is not waiting in deferred queue
        // otherwise read it from the external file
        return deferredTasks
                .OfType<DeferredWritingTask>()
                .FirstOrDefault(x => filePath.Equals(x.ExtFilePath))
                ?.Data
            ?? ExternalFileWriter.GetNew(filePath).Read();
    }

    public void AddFileLink(string externalLinkWithPrefix)
    {
        if (string.IsNullOrEmpty(externalLinkWithPrefix))
        {
            return;
        }

        ExternalFilePath externalFilePath = pathFactory.Create(
            origamFile.Path,
            externalLinkWithPrefix
        );
        AddOrRefreshHash(externalFilePath);
    }

    public void RemoveExternalFile(string fileName)
    {
        ExternalFilePath path = pathFactory.Create(origamFile.Path, fileName);
        deferredTasks.Enqueue(new DeferredDeletingTask(path, pathFileDict));
    }

    public Maybe<ExternalFile> GetExternalFile(FileInfo externalFile)
    {
        pathFileDict.TryGetValue(externalFile.FullName, out var extFile);
        return extFile;
    }
}

class DeferredDeletingTask : IDeferredTask
{
    private readonly ExternalFilePath extFilePath;
    private readonly Dictionary<string, ExternalFile> pathFileDict;

    public DeferredDeletingTask(
        ExternalFilePath extFilePath,
        Dictionary<string, ExternalFile> pathFileDict
    )
    {
        this.extFilePath = extFilePath;
        this.pathFileDict = pathFileDict;
    }

    public void Run()
    {
        ExternalFile file =
            pathFileDict.Values.FirstOrDefault(x => x.ExtFilePath.Equals(extFilePath))
            ?? new ExternalFile(extFilePath, "");
        pathFileDict.Remove(file.ExtFilePath.Absolute);
        Delete();
    }

    private void Delete()
    {
        if (extFilePath.Exists)
        {
            File.Delete(extFilePath.Absolute);
        }
    }
}

class DeferredWritingTask : IDeferredTask
{
    public ExternalFilePath ExtFilePath { get; }
    public object Data { get; }
    private readonly Action<ExternalFilePath> addOrRefreshHash;

    public DeferredWritingTask(
        ExternalFilePath extFilePath,
        object data,
        Action<ExternalFilePath> addOrRefreshHash
    )
    {
        this.ExtFilePath = extFilePath;
        this.Data = data;
        this.addOrRefreshHash = addOrRefreshHash;
    }

    public void Run()
    {
        ExternalFileWriter.GetNew(ExtFilePath).Write(Data);
        addOrRefreshHash(ExtFilePath);
    }
}

public class ExternalFile
{
    public string FileHash { get; }
    public ExternalFilePath ExtFilePath { get; }

    public ExternalFile(ExternalFilePath path, string hash)
    {
        ExtFilePath = path;
        FileHash = hash;
    }
}
