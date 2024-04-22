using NUnit.Framework;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.ServiceTests.MetaModelUpgraderTests;

namespace Origam.DA.ServiceTests.ScriptContainersTests;

[TestFixture]
public class StateMachineEventParameterMappingTests: ClassUpgradeTestBase
{
    protected override string DirName => "ScriptContainersTests";
    [Test]
    public void ShouldRenameTypeProperty()
    {
            XFileData xFileData = LoadFile("BusinessPartner.origam");
            var modelUpgrader = new MetaModelAnalyzer(new NullFileWriter(), new MetaModelUpgrader());
            modelUpgrader.TryUpgrade(xFileData);
        }
}