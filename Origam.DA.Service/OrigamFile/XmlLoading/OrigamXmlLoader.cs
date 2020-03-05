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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;
using MoreLinq;
using Origam.DA.Common;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;

namespace Origam.DA.Service
{
    public class OrigamXmlLoader
    {
        private readonly ObjectFileDataFactory objectFileDataFactory;
        private readonly DirectoryInfo topDirectory;
        private readonly XmlFileDataFactory xmlFileDataFactory;
        private readonly XmlLoader xmlLoader;

        public OrigamXmlLoader(ObjectFileDataFactory objectFileDataFactory, 
            DirectoryInfo topDirectory, XmlFileDataFactory xmlFileDataFactory)
        {
            this.objectFileDataFactory = objectFileDataFactory;
            this.topDirectory = topDirectory;
            this.xmlFileDataFactory = xmlFileDataFactory;
            xmlLoader = new XmlLoader(topDirectory, xmlFileDataFactory);
        }

        public Maybe<XmlLoadError> LoadInto(ItemTracker itemTracker,  bool tryUpdate)
        {
            Result<List<XmlFileData>, XmlLoadError> result =
                xmlLoader.FindMissingFiles(itemTracker, tryUpdate);

            if (result.IsSuccess)
            {
                AddOrigamFiles(itemTracker, result.Value);
                RemoveOrigamFilesThatNoLongerExist(itemTracker);
                return Maybe<XmlLoadError>.None;
            } 
            else
            {
                return result.Error;
            }
        }

        private void AddOrigamFiles(ItemTracker itemTracker,
            List<XmlFileData> filesToLoad)
        {
            GetNamespaceFinder(filesToLoad, itemTracker)
                .FileDataWithNamespacesAssigned
                .AsParallel()
                .Select(objFileData => objFileData.Read())
                .ForEach( x=>
                {
                    itemTracker.AddOrReplace(x);
                    itemTracker.AddOrReplaceHash(x);
                });
        }

        private void RemoveOrigamFilesThatNoLongerExist(ItemTracker itemTracker)
        {
            IEnumerable<FileInfo> allFilesInSubDirectories
                = topDirectory.GetAllFilesInSubDirectories();
            itemTracker.KeepOnly(allFilesInSubDirectories);
        }

        private INamespaceFinder GetNamespaceFinder(List<XmlFileData> filesToLoad,
            ItemTracker itemTracker)
        {
            if (filesToLoad.Count == 0)
            {
                return new NullNamespaceFinder();
            } 

            // PreLoadedNamespaceFinder needs to realod all files so we have to
            // clear tracker and run FindMissingFiles method again to get all
            // origam files. This will not be necessary once loading of
            // individual files is supported.  
            
            itemTracker.Clear();
            List<XmlFileData> allOrigamFiles = 
                xmlLoader.FindMissingFiles(itemTracker: itemTracker, tryUpdate: false)
                .Value;
            return new PreLoadedNamespaceFinder(
                allOrigamFiles,
                objectFileDataFactory);
        }
    }
    
    public class MetaVersionFixer
    {
        private readonly string xmlNameSpaceName;
        private readonly bool failIfNamespaceNotFound;
        private readonly Version currentVersion;

        public MetaVersionFixer(string xmlNameSpaceName, Version currentVersion,
            bool failIfNamespaceNotFound)
        {
            this.xmlNameSpaceName = xmlNameSpaceName;
            this.failIfNamespaceNotFound = failIfNamespaceNotFound;
            this.currentVersion = currentVersion;
        }

        public  Result<int,XmlLoadError> UpdateVersion(OrigamXmlDocument xmlDoc, bool tryUpdate)
        {
            if (xmlDoc.IsEmpty) Result.Ok<int, XmlLoadError>(0);
            string nameSpace = xmlDoc.GetNameSpaceByName(xmlNameSpaceName);
            if (nameSpace != null)
            {
                return UpdateVersion(xmlDoc, nameSpace, tryUpdate);
            }
            return failIfNamespaceNotFound 
                ? Result.Fail<int,XmlLoadError>( new XmlLoadError(ErrType.XmlGeneralError, xmlNameSpaceName+" namespace not found in: "+xmlDoc.BaseURI)) 
                : Result.Ok<int, XmlLoadError>(0);
        }

        private Result<int,XmlLoadError> UpdateVersion(OrigamXmlDocument xmlDoc,string nameSpace, bool tryUpdate)
        {
            Version version = OrigamNameSpace.Create(nameSpace).Version;
            if ( version > currentVersion)
            {
                return Result.Fail<int,XmlLoadError>( new XmlLoadError(ErrType.XmlGeneralError, $"Cannot work with file: {xmlDoc.BaseURI} because it's version of namespace \"{nameSpace}\" is newer than the current version: {currentVersion}"));
            }
            if( version < currentVersion &&
               !version.DiffersOnlyInBuildFrom(currentVersion))
            {
                if(tryUpdate)
                {
                    throw new NotImplementedException();
                } else
                {
                    return Result.Fail<int,XmlLoadError>( 
                        new XmlLoadError( ErrType.XmlVersionIsOlderThanCurrent,
                        $"{xmlDoc.BaseURI} has old version of: {nameSpace}, current version: {currentVersion}"));
                }
            }
            return Result.Ok<int, XmlLoadError>(0);
        } 
    }
}
