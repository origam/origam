using System.IO;
using System.Xml;
using NUnit.Framework;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.TestCommon;

namespace Origam.DA.ServiceTests.MetaModelUpgraderTests;
public class ClassUpgradeTestBase : AbstractFileTestClass
{
    protected XFileData LoadFile(string fileName)
    {
        var file = new FileInfo(Path.Combine(TestFilesDir.FullName, fileName));
        var document = new OrigamXDocument(file);
        return new XFileData(document, file);
    }
    
    protected override TestContext TestContext =>
        TestContext.CurrentContext;
}
