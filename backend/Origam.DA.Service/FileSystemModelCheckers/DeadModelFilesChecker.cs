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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.FileSystemModeCheckers;

namespace Origam.DA.Service.FileSystemModelCheckers;
public class DeadModelFilesChecker: IFileSystemModelChecker
{
    private readonly FilePersistenceProvider filePersistenceProvider;
    private readonly FileFilter ignoredFileFilter;
    private readonly List<FileInfo> modelDirectoryFiles;
    public DeadModelFilesChecker(
        FilePersistenceProvider filePersistenceProvider,
        FileFilter ignoredFileFilter,
        List<FileInfo> modelDirectoryFiles)
    {
        this.filePersistenceProvider = filePersistenceProvider;
        this.ignoredFileFilter = ignoredFileFilter;
        this.modelDirectoryFiles = modelDirectoryFiles;
    }
    public IEnumerable<ModelErrorSection> GetErrors()
    {
        var fileNamesToIgnore = new []
        {
            ".origamGroupReference",
            ".gitignore",
            ".origamDoc",
            "index.originalTracker",
            "index.testTracker"
        };
        string topDirectoryPath = filePersistenceProvider.TopDirectory.FullName;
        IFilePersistent[] allPersistedObjects = filePersistenceProvider
            .RetrieveList<IFilePersistent>()
            .ToArray();
        var modelFilePaths = allPersistedObjects
            .Select(persistent => Path.Combine(topDirectoryPath, persistent.RelativeFilePath));
        var externalFilePaths = allPersistedObjects
            .Select(persistent => filePersistenceProvider.FindPersistedObjectInfo(persistent.Id))
            .Select(objectInfo => objectInfo.OrigamFile)
            .SelectMany(origamFile => origamFile.ExternalFiles)
            .Select(file => file.FullName);
        
        HashSet<string> knownFilePaths = new HashSet<string>(modelFilePaths
            .Concat(externalFilePaths));
      
        var unexpectedFiles = modelDirectoryFiles
            .Where(file => !knownFilePaths.Contains(file.FullName))
            .Where(file => !fileNamesToIgnore.Contains(file.Name))
            .Where(file => ignoredFileFilter.ShouldPass(file.FullName))
            .ToList();
        return  new []{ 
            new ModelErrorSection
            (
                caption : "These files are not referenced by any model element. Please remove them.",
                errorMessages : unexpectedFiles
                    .Where(file => file.Extension != ".origam")
                    .Select(file => 
                        new ErrorMessage(
                            text: file.FullName, 
                            link:file.Directory?.FullName)
                    )
                    .ToList()
            ),                
            new ModelErrorSection
            (
                caption : "These files are empty or contain incomplete data so they add nothing to the model. Please remove them.",
                errorMessages :  unexpectedFiles
                    .Where(file => file.Extension == ".origam")
                    .Select(file => 
                        new ErrorMessage(
                            text: file.FullName, 
                            link:file.Directory?.FullName)
                    )
                    .ToList()
            )
        };
    }
}
