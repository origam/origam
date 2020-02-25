using System;
using System.Xml;
using Origam.DA.Common;
using Origam.DA.ServiceTests;

namespace Origam.DA.Service.MetaModelUpgrade
{
    class TestPersistedClassScriptContainer : UpgradeScriptContainer
    {
        public override string ClassName { get; } = "Origam.DA.ServiceTests.TestPersistedClass";
        
        public TestPersistedClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.1"), 
                new Version("1.0.2"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty2", "");
                }));
        }
    }    
    
    class TestDeadClassScriptContainer : UpgradeScriptContainer
    {
        public override string ClassName { get; } = "Origam.DA.ServiceTests.TestDeadClass";
        public TestDeadClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.1"), 
                Versions.EndOfLife,
                (node, doc) =>
                {
                    doc.DocumentElement.RemoveChild(node);
                }));
        }
    }   
    
    class TestBaseClassScriptContainer : UpgradeScriptContainer
    {
        public override string ClassName { get; } = "Origam.DA.ServiceTests.TestBaseClass";
        public TestBaseClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "TestBaseClassProperty", "");
                }));
        }
    }
    
    class TestPersistedClassScriptContainer2 : UpgradeScriptContainer
    {
        public override string ClassName { get; } = "Origam.DA.ServiceTests.TestPersistedClass2";
        public TestPersistedClassScriptContainer2() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.0"), 
                new Version("1.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.1"), 
                new Version("1.0.2"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty2", "");
                }));           
            upgradeScripts.Add(new UpgradeScript(
                new Version("1.0.3"), 
                new Version("1.0.4"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty4", "");
                }));
        }
    }
}