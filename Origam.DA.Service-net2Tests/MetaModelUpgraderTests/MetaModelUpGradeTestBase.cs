using System.IO;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.TestCommon;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests
{
    public class MetaModelUpGradeTestBase : AbstractFileTestClass
    {
        protected XmlFileData LoadFile(string fileName)
        {
            var file = new FileInfo(Path.Combine(TestFilesDir.FullName, fileName));
            var document = new OrigamXmlDocument();
            document.Load(file.FullName);
            return new XmlFileData(document, file);
        }
        
        protected override TestContext TestContext =>
            TestContext.CurrentContext;
    }
}