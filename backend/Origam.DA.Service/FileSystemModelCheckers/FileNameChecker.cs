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
using Origam.DA.ObjectPersistence;
using Origam.DA.Service.FileSystemModeCheckers;
using Origam.Schema;

namespace Origam.DA.Service;

public class FileNameChecker : IFileSystemModelChecker
{
    private readonly FilePersistenceProvider filePersistenceProvider;
    private readonly FilePersistenceIndex index;
    private readonly string topDirectory;

    public FileNameChecker(
        FilePersistenceProvider filePersistenceProvider,
        FilePersistenceIndex index
    )
    {
        this.filePersistenceProvider = filePersistenceProvider;
        this.index = index;
        topDirectory = this.filePersistenceProvider.TopDirectory.FullName;
    }

    public IEnumerable<ModelErrorSection> GetErrors()
    {
        IFilePersistent[] allPersistedObjects = filePersistenceProvider
            .RetrieveList<IFilePersistent>()
            .ToArray();
        HashSet<Guid> allChildrenIds = new HashSet<Guid>(
            allPersistedObjects.OfType<ISchemaItem>().SelectMany(GetChildrenIds)
        );
        List<ErrorMessage> errors = allPersistedObjects
            .Where(instance => !(instance is SchemaItemAncestor))
            .Where(instance => !allChildrenIds.Contains(instance.Id))
            .Where(IsPersistedInWrongFile)
            .SelectMany(ToErrorMessages)
            .ToList();
        yield return new ModelErrorSection(
            "Objects persisted in wrong files (object name is different from file name)",
            errors
        );
    }

    private IEnumerable<ErrorMessage> ToErrorMessages(IFilePersistent instance)
    {
        string actualFilePath = index.GetById(instance.Id).OrigamFile.Path.Absolute;
        string expectedFilePath = Path.Combine(topDirectory, instance.RelativeFilePath);
        yield return new ErrorMessage($"Object with id: \"{instance.Id}\"");
        yield return new ErrorMessage(
            text: $"should be in: {expectedFilePath}",
            link: expectedFilePath
        );
        yield return new ErrorMessage(
            text: $"but is in:    {actualFilePath}",
            link: actualFilePath
        );
    }

    private IEnumerable<Guid> GetChildrenIds(ISchemaItem item)
    {
        return item.ChildItems.Select(x => x.Id);
    }

    private bool IsPersistedInWrongFile(IFilePersistent instance)
    {
        var objectInfo = index.GetById(instance.Id);
        return objectInfo.OrigamFile.Path.Relative != instance.RelativeFilePath;
    }
}
