using System;
using System.Xml;
using Origam.DA.ServiceTests;

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
                    ((XmlElement) node).SetAttribute("newProperty1", "");
                    return node;
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.1"), 
                new Version("1.0.2"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("newProperty2", "");
                    return node;
                }));
        }
    }    
    class TestBaseClassScriptContainer : UpgradeScriptContainer<TestBaseClass>
    {
        public TestBaseClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("TestBaseClassProperty", "");
                    return node;
                }));
        }
    }
    
    class TestPersistedClassScriptContainer2 : UpgradeScriptContainer<TestPersistedClass2>
    {
        public TestPersistedClassScriptContainer2() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("newProperty1", "");
                    return node;
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.1"), 
                new Version("1.0.2"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("newProperty2", "");
                    return node;
                }));           
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.3"), 
                new Version("1.0.4"),
                (node, doc) =>
                {
                    ((XmlElement) node).SetAttribute("newProperty4", "");
                    return node;
                }));
        }
    }
}