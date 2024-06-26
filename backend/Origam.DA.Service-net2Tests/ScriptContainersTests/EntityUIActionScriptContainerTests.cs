using NUnit.Framework;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.ServiceTests.MetaModelUpgraderTests;

namespace Origam.DA.ServiceTests.ScriptContainersTests;
[TestFixture]
public class EntityUIActionScriptContainerTests: ClassUpgradeTestBase
{
    protected override string DirName => "ScriptContainersTests";
    [Test]
    public void ShouldCreteChildren()
    {
        XFileData xFileData = LoadFile("TestEntityAction.origam");
        var modelUpgrader = new MetaModelAnalyzer(
            new NullFileWriter(),
            new MetaModelUpgrader());
        modelUpgrader.TryUpgrade(xFileData);
    }
}
