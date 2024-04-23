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

using System.IO;
using Origam.DA.Service.MetaModelUpgrade;

namespace Origam.DA.Service;

public class TrackerLoaderFactory
{
    private readonly DirectoryInfo topDirectory;
    private readonly ObjectFileDataFactory objectFileDataFactory;
    private readonly OrigamFileFactory origamFileFactory;
    private readonly FileInfo pathToIndexFile;
    private readonly XmlFileDataFactory xmlFileDataFactory;
    private OrigamXmlLoader xmlLoader;
    private IBinFileLoader binLoader;
    private readonly bool useBinFile;
    private FilePersistenceIndex filePersistenceIndex;
    private readonly IMetaModelUpgradeService metaModelUpgradeService;

    public TrackerLoaderFactory(DirectoryInfo topDirectory,
        ObjectFileDataFactory objectFileDataFactory,
        OrigamFileFactory origamFileFactory,
        XmlFileDataFactory xmlFileDataFactory,
        FileInfo pathToIndexFile, bool useBinFile,
        FilePersistenceIndex filePersistence,
        IMetaModelUpgradeService metaModelUpgradeService)
    {
            this.topDirectory = topDirectory;
            this.objectFileDataFactory = objectFileDataFactory;
            this.origamFileFactory = origamFileFactory;
            this.pathToIndexFile = pathToIndexFile;
            this.xmlFileDataFactory = xmlFileDataFactory;
            this.useBinFile= useBinFile;
            this.filePersistenceIndex = filePersistence;
            this.metaModelUpgradeService = metaModelUpgradeService;
        }

        
    internal OrigamXmlLoader XmlLoader {
        get
        {
                return xmlLoader ?? (xmlLoader = new OrigamXmlLoader(
                           objectFileDataFactory, topDirectory, xmlFileDataFactory, metaModelUpgradeService));
            }
    }

    internal IBinFileLoader BinLoader {
        get
        {
                return binLoader ?? (binLoader = MakeBinLoader());
            }
    }

    private IBinFileLoader MakeBinLoader()
    {
            return useBinFile
                ? (IBinFileLoader) new BinFileLoader(origamFileFactory, topDirectory, pathToIndexFile, filePersistenceIndex)
                : new NullBinFileLoader();
        }
}