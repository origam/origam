using NUnit.Framework;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.DA.ServiceTests.MetaModelUpgraderTests;

namespace Origam.DA.ServiceTests.ScriptContainersTests
{
    [TestFixture]
    public class EntityUIActionScriptContainerTests: ClassUpgradeTestBase
    {
        [Test]
        public void ShouldCreteChildren()
        {
            XFileData xFileData = LoadFile("TestEntityAction.origam");
            var modelUpGrader = new MetaModelUpgrader(new NullFileWriter());
            modelUpGrader.TryUpgrade(xFileData);
        }
    }
}