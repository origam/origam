using System;
using System.Xml;

namespace Origam.DA.Service.MetaModelUpgrade
{
    class TestPersistedClassScriptContainer : UpgradeScriptContainer<TestPersistedClass>
    {
        public TestPersistedClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("NewProperty1", "");
                    return node;
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.1"), 
                new Version("1.0.2"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("NewProperty2", "");
                    return node;
                }));
        }
    }
}