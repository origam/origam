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
#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.Xml;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.Service.NamespaceMapping;
using Origam.Extensions;

namespace Origam.DA.Service;

public class OrigamXmlLoader
{
    private readonly ObjectFileDataFactory objectFileDataFactory;
    private readonly DirectoryInfo topDirectory;
    private readonly XmlFileDataFactory xmlFileDataFactory;
    private readonly IMetaModelUpgradeService metaModelUpgradeService;

    public OrigamXmlLoader(
        ObjectFileDataFactory objectFileDataFactory,
        DirectoryInfo topDirectory,
        XmlFileDataFactory xmlFileDataFactory,
        IMetaModelUpgradeService metaModelUpgradeService
    )
    {
        this.objectFileDataFactory = objectFileDataFactory;
        this.topDirectory = topDirectory;
        this.xmlFileDataFactory = xmlFileDataFactory;
        this.metaModelUpgradeService = metaModelUpgradeService;
    }

    public Maybe<XmlLoadError> LoadInto(ItemTracker itemTracker, MetaModelUpgradeMode mode)
    {
        PropertyToNamespaceMapping.Init();
        Result<List<XmlFileData>, XmlLoadError> result = FindMissingFiles(itemTracker);
        if (result.IsFailure)
        {
            return result.Error;
        }
        List<XmlFileData> dataToAdd = metaModelUpgradeService.Upgrade(result.Value, mode);
        AddOrigamFiles(itemTracker, dataToAdd);
        RemoveOrigamFilesThatNoLongerExist(itemTracker);
        return Maybe<XmlLoadError>.None;
    }

    private void AddOrigamFiles(ItemTracker itemTracker, List<XmlFileData> filesToLoad)
    {
        GetNamespaceFinder(filesToLoad)
            .FileDataWithNamespacesAssigned.AsParallel()
            .Select(objFileData => objFileData.Read())
            .ForEach(x =>
            {
                itemTracker.AddOrReplace(x);
                itemTracker.AddOrReplaceHash(x);
            });
    }

    private Result<List<XmlFileData>, XmlLoadError> FindMissingFiles(ItemTracker itemTracker)
    {
        List<Result<XmlFileData, XmlLoadError>> results = topDirectory
            .GetAllFilesInSubDirectories()
            .AsParallel()
            .Where(OrigamFile.IsPersistenceFile)
            .Where(file => itemTracker == null || !itemTracker.ContainsFile(file))
            .Select(xmlFileDataFactory.Create)
            .ToList();
        List<Result<XmlFileData, XmlLoadError>> errors = results
            .Where(result => result.IsFailure)
            .ToList();
        IEnumerable<XmlFileData> data = results.Select(res => res.Value);

        return errors.Count == 0
            ? Result.Success<List<XmlFileData>, XmlLoadError>(data.ToList())
            : Result.Failure<List<XmlFileData>, XmlLoadError>(errors[0].Error);
    }

    private void RemoveOrigamFilesThatNoLongerExist(ItemTracker itemTracker)
    {
        IEnumerable<FileInfo> allFilesInSubDirectories = topDirectory.GetAllFilesInSubDirectories();
        itemTracker.KeepOnly(allFilesInSubDirectories);
    }

    private INamespaceFinder GetNamespaceFinder(List<XmlFileData> filesToLoad)
    {
        if (filesToLoad.Count == 0)
        {
            return new NullNamespaceFinder();
        }
        return new PreLoadedNamespaceFinder(filesToLoad, objectFileDataFactory);
    }
}
