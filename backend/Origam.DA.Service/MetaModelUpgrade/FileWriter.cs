using System.IO;

namespace Origam.DA.Service.MetaModelUpgrade;

public interface IFileWriter
{
    void Write(FileInfo file, string text);
    void Delete(FileInfo file);
}

public class NullFileWriter : IFileWriter
{
    public void Write(FileInfo file, string text)
    {
        }

    public void Delete(FileInfo file)
    {
        }
}
    
class FileWriter : IFileWriter
{
    public void Write(FileInfo file, string text)
    {
            File.WriteAllText(file.FullName, text);
        }

    public void Delete(FileInfo file)
    {
            file.Delete();
        }
}