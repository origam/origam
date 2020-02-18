#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using CSharpFunctionalExtensions;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class XmlLoader
    {
        private readonly DirectoryInfo topDirectory;
        private readonly XmlFileDataFactory xmlFileDataFactory;

        public XmlLoader(DirectoryInfo topDirectory, XmlFileDataFactory xmlFileDataFactory)
        {
            this.topDirectory = topDirectory;
            this.xmlFileDataFactory = xmlFileDataFactory;
        }

        public Result<List<XmlFileData>, XmlLoadError> LoadOrigamFiles()
        {
            return FindMissingFiles(itemTracker: null,  tryUpdate: false);
        }

        public Result<List<XmlFileData>, XmlLoadError> FindMissingFiles(
            ItemTracker itemTracker, bool tryUpdate)
        {
            List<Result<XmlFileData, XmlLoadError>> results = topDirectory
                .GetAllFilesInSubDirectories()
                .AsParallel()
                .Where(OrigamFile.IsPersistenceFile)
                .Where(file => itemTracker == null || !itemTracker.ContainsFile(file))
                .Select(fileInfo =>
                    xmlFileDataFactory.Create(fileInfo, tryUpdate))
                .ToList();

            List<Result<XmlFileData, XmlLoadError>> errors = results
                .Where(result => result.IsFailure)
                .ToList();

            IEnumerable<XmlFileData> data = results
                .Select(res => res.Value);
                
            return errors.Count == 0
                ? Result.Ok<List<XmlFileData>, XmlLoadError>(data.ToList())
                : Result.Fail<List<XmlFileData>, XmlLoadError>(errors[0].Error);
        }
    }
}