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
        private IBinFileLoader binLoader;
        private readonly bool useBinFile;

        public TrackerLoaderFactory(
            DirectoryInfo topDirectory,
            ObjectFileDataFactory objectFileDataFactory,
            OrigamFileFactory origamFileFactory,
            XmlFileDataFactory xmlFileDataFactory,
            FileInfo pathToIndexFile, bool useBinFile)
        {
            this.topDirectory = topDirectory;
            this.objectFileDataFactory = objectFileDataFactory;
            this.origamFileFactory = origamFileFactory;
            this.pathToIndexFile = pathToIndexFile;
            this.xmlFileDataFactory = xmlFileDataFactory;
            this.useBinFile= useBinFile;
        }

        
        internal OrigamXmlLoader XmlLoader {
            get
            {
                return xmlLoader ?? (xmlLoader = new OrigamXmlLoader(
                           objectFileDataFactory, topDirectory, xmlFileDataFactory));
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
                ? (IBinFileLoader) new BinFileLoader(origamFileFactory, topDirectory, pathToIndexFile)
                : new NullBinFileLoader();
        }
    }
}