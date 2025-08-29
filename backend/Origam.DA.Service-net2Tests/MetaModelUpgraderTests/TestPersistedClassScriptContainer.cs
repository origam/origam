#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Origam.DA.Common;

namespace Origam.DA.Service.MetaModelUpgrade;
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
