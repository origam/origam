using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;

namespace Origam.DA.Service
{
    public class XmlFileDataFactory
    {
        private readonly List<MetaVersionFixer> versionFixers;

        public XmlFileDataFactory(List<MetaVersionFixer> versionFixers)
        {
            this.versionFixers = versionFixers;
        }

        public Result<XmlFileData, XmlLoadError> Create(FileInfo fileInfo,
            bool tryUpdate = false)
        {
            Result<OrigamXmlDocument> documentResult = LoadXmlDoc(fileInfo);
            if (documentResult.IsFailure)
            {
                return Result.Fail<XmlFileData, XmlLoadError>(
                    new XmlLoadError(documentResult.Error));
            }

            Result<int, XmlLoadError> result = versionFixers
                                                   .Select(fixer =>fixer.UpdateVersion(documentResult.Value, tryUpdate))
                                                   .Cast<Result<int, XmlLoadError>?>()
                                                   .FirstOrDefault(res => res.Value.IsFailure)
                                               ?? Result.Ok<int, XmlLoadError>(0);
            return result.OnSuccess(res =>
                new XmlFileData(documentResult.Value, fileInfo));
        }

        private Result<OrigamXmlDocument> LoadXmlDoc(FileInfo fileInfo)
        {
            OrigamXmlDocument xmlDocument = new OrigamXmlDocument();
            try
            {
                xmlDocument.Load(fileInfo.FullName);
            } catch (XmlException ex)
            {
                return Result.Fail<OrigamXmlDocument>(
                    $"Could not read file: {fileInfo.FullName}{Environment.NewLine}{ex.Message}");
            }
            return Result.Ok(xmlDocument);
        }
    }
    
    public class OrigamXmlDocument : XmlDocument
    {
        public string GetNameSpaceByName(string xmlNameSpaceName)
        {
            if (IsEmpty) return null;
            return ChildNodes[1]?.Attributes?[xmlNameSpaceName]?.InnerText;
        }

        public bool IsEmpty => ChildNodes.Count < 2;
    }
}