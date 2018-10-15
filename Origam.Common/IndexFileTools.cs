namespace Origam
{
    public interface IndexFileTools
    {
        string GetFirstUnprocessedFile(
            string path, string mask, string indexFile);
    }
}
