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