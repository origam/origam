using System.IO;

namespace Origam.DA.Service.MetaModelUpgrade
{
    public interface IFileWriter
    {
        void Write(FileInfo file, string text);
    }

    public class NullFileWriter : IFileWriter
    {
        public void Write(FileInfo file, string text)
        {
        }
    }
    
    class FileWriter : IFileWriter
    {
        public void Write(FileInfo file, string text)
        {
            File.WriteAllText(file.FullName, text);
        }
    }
}