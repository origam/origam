using System.IO;
using NUnit.Framework;

namespace Origam.TestCommon
{
    public abstract class AbstractFileTestClass
    {
        protected DirectoryInfo ProjectDir =>
            new DirectoryInfo(TestContext.TestDirectory);

        protected virtual string DirName => "";

        protected abstract TestContext TestContext { get; }

        protected DirectoryInfo TestFilesDir {
            get
            {
                string path = Path.Combine(ProjectDir.FullName,DirName,"TestFiles");
                Directory.CreateDirectory(path);
                return new DirectoryInfo(path);
            }
        }
        protected DirectoryInfo TestProjectDir {
            get
            {
                string relativeToFilesDir = DirName + @"\TestProject";
                
                string path = Path.Combine(ProjectDir.FullName, relativeToFilesDir);
                Directory.CreateDirectory(path);
                return  new DirectoryInfo(path);
            }
        }

        protected void ClearTestDir()
        {
            Directory.Delete(TestFilesDir.FullName, true);
        }
    }
}