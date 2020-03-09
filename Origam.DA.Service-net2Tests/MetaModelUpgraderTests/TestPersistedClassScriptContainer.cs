using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Origam.DA.Common;
using Origam.DA.ServiceTests;
using Origam.Extensions;

namespace Origam.DA.Service.MetaModelUpgrade
{
    class TestPersistedClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestPersistedClass";
        public override List<string> OldFullTypeNames { get; }

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
    
    // class TestTestRenamedClassScriptContainer : UpgradeScriptContainer
    // {
    //     public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestRenamedClass";
    //     public override List<string> OldFullTypeNames { get; } = new List<string>{"Origam.DA.ServiceTests.TestOldNameClass"};
    //
    //     public TestTestRenamedClassScriptContainer() 
    //     {
    //         upgradeScripts.Add(new UpgradeScript(
    //             new Version("6.0.0"), 
    //             new Version("6.0.1"),
    //             (node, doc) =>
    //             {
    //                 doc.FileElement.RemoveAttribute("xmlns:tonc");
    //                 doc.FileElement.SetAttribute("xmlns:trc","http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1");
    //                 var classElement = (XmlElement)doc.FileElement.ChildNodes[0];
    //                 var newElement = doc.CreateElement("trc","TestRenamedClass", "http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1");
    //                 var name = classElement.Attributes["tonc:name"].Value;
    //                 newElement.SetAttribute(
    //                     "name",
    //                     "http://schemas.origam.com/Origam.DA.ServiceTests.TestRenamedClass/6.0.1",
    //                     name);
    //                 XmlNode parent = node.ParentNode;
    //                 parent.RemoveChild(node);
    //                 parent.AppendChild(newElement);
    //             }));
    //     }
    // }    
    
    class TestDeadClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestDeadClass";
        public override List<string> OldFullTypeNames { get; }
    
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
                    node.Remove();
                }));
        }
    }       
    // class TestTestDeadBaseClassScriptContainer : UpgradeScriptContainer
    // {
    //     public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestDeadBaseClass";
    //     public override List<string> OldFullTypeNames { get; }
    //
    //     public TestTestDeadBaseClassScriptContainer() 
    //     {
    //         upgradeScripts.Add(new UpgradeScript(
    //             new Version("6.0.0"), 
    //             new Version("6.0.1"),
    //             (node, doc) =>
    //             {
    //                 AddAttribute(node, "deadClassProperty", "");
    //             }));            
    //         upgradeScripts.Add(new UpgradeScript(
    //             new Version("6.0.1"), 
    //             Versions.Last,
    //             (node, doc) =>
    //             {
    //                 ((XmlElement)node).RemoveAttribute("deadClassProperty", "http://schemas.origam.com/Origam.DA.ServiceTests.TestDeadBaseClass/6.0.1");
    //             }));
    //     }
    // }   
    
    class TestBaseClassScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = "Origam.DA.ServiceTests.TestBaseClass";
        public override List<string> OldFullTypeNames { get; }

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
}