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
using CSharpFunctionalExtensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public class UpgradeManager
    {
        private readonly XmlLoader xmlLoader;
        private List<XmlFileData> xmlFileData = new List<XmlFileData>();

        public UpgradeManager(DirectoryInfo topDirectory)
        {
            var xmlFileDataFactory = new XmlFileDataFactory(new List<MetaVersionFixer>());
            xmlLoader = new XmlLoader(topDirectory ,xmlFileDataFactory);
        }
        
        public Maybe<XmlLoadError> LoadFiles()
        {
            Result<List<XmlFileData>,XmlLoadError> result = xmlLoader.LoadOrigamFiles();
            if (result.IsSuccess)
            {
                xmlFileData = result.Value;
                return Maybe<XmlLoadError>.None;
            }
            return result.Error;
        }

        public void Upgrade()
        {
            new MetaModelUpGrader().TryUpgrade(xmlFileData);
        }
    }
}