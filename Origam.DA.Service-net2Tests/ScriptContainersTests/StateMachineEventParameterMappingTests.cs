using NUnit.Framework;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.ServiceTests.MetaModelUpgraderTests;

namespace Origam.DA.ServiceTests.ScriptContainersTests
{
    [TestFixture]
    public class StateMachineEventParameterMappingTests: ClassUpgradeTestBase
    {
        [Test]
        public void ShouldRenameTypeProperty()
        {
            XFileData xFileData = LoadFile("BusinessPartner.origam");
            var modelUpGrader = new MetaModelUpgrader(new NullFileWriter());
            modelUpGrader.TryUpgrade(xFileData);
        }
    }
}