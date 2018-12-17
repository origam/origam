using System.IO;

namespace Origam.DA.Service
{
    public class TrackerLoaderFactory
    {
        private readonly DirectoryInfo topDirectory;
        private readonly ObjectFileDataFactory objectFileDataFactory;
        private readonly OrigamFileFactory origamFileFactory;
        private readonly FileInfo pathToIndexFile;
        private readonly XmlFileDataFactory xmlFileDataFactory;
        private OrigamXmlLoader xmlLoader;
        private BinFileLoader binLoader;
        
        public TrackerLoaderFactory(
            DirectoryInfo topDirectory,
            ObjectFileDataFactory objectFileDataFactory,
            OrigamFileFactory origamFileFactory,
            XmlFileDataFactory xmlFileDataFactory,
            FileInfo pathToIndexFile)
        {
            this.topDirectory = topDirectory;
            this.objectFileDataFactory = objectFileDataFactory;
            this.origamFileFactory = origamFileFactory;
            this.pathToIndexFile = pathToIndexFile;
            this.xmlFileDataFactory = xmlFileDataFactory;
        }

        
        internal OrigamXmlLoader XmlLoader {
            get
            {
                return xmlLoader ?? (xmlLoader = new OrigamXmlLoader(
                           objectFileDataFactory, topDirectory, xmlFileDataFactory));
            }
        }

        internal BinFileLoader BinLoader {
            get
            {
                return binLoader ?? (binLoader = new BinFileLoader(
                           origamFileFactory, topDirectory, pathToIndexFile));
            }
        }
    }
}