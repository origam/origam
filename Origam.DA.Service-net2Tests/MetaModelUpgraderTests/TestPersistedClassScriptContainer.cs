using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Origam.DA.Common;
using Origam.DA.ServiceTests;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    class TestPersistedClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestPersistedClass";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }

        public TestPersistedClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.1"), 
                new Version("6.0.2"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty2", "");
                }));
        }
    }     
    
    class TestTestRenamedClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestRenamedClass";
        public override List<string> OldFullTypeNames { get; } = new List<string>{"Origam.DA.ServiceTests.TestOldNameClass"};
        public override string[] OldPropertyXmlNames { get; }
        
        public TestTestRenamedClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    XNamespace trcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1";
                    XNamespace toncNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestOldNameClass/6.0.0";
                    node.Name = trcNamespace.GetName("TestRenamedClass");
                    XAttribute attribute = node.Attribute(toncNamespace.GetName("name"));
                    attribute.Remove();
                    node.Add(new XAttribute(trcNamespace.GetName("name"), attribute.Value));
                    
                    doc.FileElement.Attribute(XNamespace.Xmlns.GetName("tonc")).Remove();
                    doc.FileElement.Add(new XAttribute(trcNamespace + "trc", trcNamespace));
                }));
        }
    }    
    
    class TestDeadClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestDeadClass";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }
        
        public TestDeadClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.1"), 
                Versions.Last,
                (node, doc) =>
                {
                    XNamespace tdcNamespace = doc.FindClassNamespace("Origam.DA.ServiceTests.TestDeadClass");
                    XNamespace tpcNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestPersistedClass/6.0.2";
                    
                    node.Remove();
                    string namePropertyValue = node.Attribute(tdcNamespace.GetName("name")).Value;
                    XElement newElement = new XElement(tpcNamespace.GetName("TestPersistedClass"));
                    newElement.SetAttributeValue(tpcNamespace.GetName("name"), namePropertyValue);
                    newElement.SetAttributeValue(tpcNamespace.GetName("newProperty1"), "");
                    newElement.SetAttributeValue(tpcNamespace.GetName("newProperty2"), "");
                    doc.FileElement.SetAttributeValue(XNamespace.Xmlns.GetName("tpc"), tpcNamespace);
                    doc.FileElement.Add(newElement);
                    
                }));
        }
    }        
    
    class TestDeadClass2ScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestDeadClass2";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }
        
        public TestDeadClass2ScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.1"), 
                Versions.Last,
                (node, doc) =>
                {
                    node.Remove();
                }));
        }
    }       
    class TestTestDeadBaseClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestDeadBaseClass";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }
        
        public TestTestDeadBaseClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "deadClassProperty", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.1"), 
                Versions.Last,
                (node, doc) =>
                {
                    XNamespace deadNamespace = "http://schemas.origam.com/Origam.DA.ServiceTests.TestDeadBaseClass/6.0.1";
                    node.Attribute(deadNamespace.GetName("deadClassProperty")).Remove();
                }));
        }
    }   
    
    class TestBaseClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestBaseClass";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }
        
        public TestBaseClassScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "TestBaseClassProperty", "");
                }));
        }
    }
    
    class TestPersistedClassScriptContainer2 : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestPersistedClass2";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }
        
        public TestPersistedClassScriptContainer2() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));            
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.1"), 
                new Version("6.0.2"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty2", "");
                }));           
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.3"), 
                new Version("6.0.4"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty4", "");
                }));
        }
    } 
    
    class TestPersistedClassScriptContainer4 : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestPersistedClass4";
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }
        
        public TestPersistedClassScriptContainer4() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    AddAttribute(node, "newProperty1", "");
                }));
        }
    }
}