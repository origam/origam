#region license

/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using System.Linq;
using System.Xml.Linq;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers;
class EntityUIActionScriptContainer : UpgradeScriptContainer
{
    public override string FullTypeName { get; } = typeof(EntityUIAction).FullName;
    public override List<string> OldFullTypeNames { get; }
    public override string[] OldPropertyXmlNames { get; } =
        {"screen", "screenSection"};
        
    public EntityUIActionScriptContainer()
    {
        upgradeScripts.Add(new UpgradeScript(
            new Version("6.0.0"),
            new Version("6.1.0"),
            UpgradeTo610()));
        AddEmptyUpgrade("6.1.0", "6.2.0");
    }
    private Action<XElement, OrigamXDocument> UpgradeTo610()
    {
        return (node, doc) =>
        {
            XNamespace persistenceNamespace = GetPersistenceNamespace(node);
            XNamespace asiNamespace = GetAbstractSchemaItemNamespace(node);
            var screenAttribute = node
                .Attributes()
                .FirstOrDefault(attr => attr.Name.LocalName == "screen");
            if (screenAttribute != null)
            {
                string screenValue = screenAttribute.Value;
                screenAttribute.Remove();
                XNamespace srNamespace = "http://schemas.origam.com/Origam.Schema.GuiModel.ScreenCondition/6.0.0";
                doc.AddNamespace("sc",
                    "http://schemas.origam.com/Origam.Schema.GuiModel.ScreenCondition/6.0.0");
                XElement screenConditionNode = new XElement(srNamespace.GetName("ScreenCondition"));
                screenConditionNode.SetAttributeValue(persistenceNamespace.GetName("id"), Guid.NewGuid().ToString());
                screenConditionNode.SetAttributeValue(asiNamespace.GetName("name"), "ScreenSectionCondition1");
                screenConditionNode.SetAttributeValue(srNamespace.GetName("screen"), screenValue);
                node.Add(screenConditionNode);
            }
            var screenSectionAttribute = node
                .Attributes()
                .FirstOrDefault(attr => attr.Name.LocalName == "screenSection");
            if (screenSectionAttribute != null)
            {
                string screenSectionAttributeValue = screenSectionAttribute.Value;
                screenSectionAttribute.Remove();
                XNamespace ssrNamespace = "http://schemas.origam.com/Origam.Schema.GuiModel.ScreenSectionCondition/6.0.0";
                doc.AddNamespace("ssc",
                    "http://schemas.origam.com/Origam.Schema.GuiModel.ScreenSectionCondition/6.0.0");
                XElement screenSectionConditionNode = new XElement(ssrNamespace.GetName("ScreenSectionCondition"));
                screenSectionConditionNode.SetAttributeValue(persistenceNamespace.GetName("id"), Guid.NewGuid().ToString());
                screenSectionConditionNode.SetAttributeValue(asiNamespace.GetName("name"), "ScreenSectionCondition1");
                screenSectionConditionNode.SetAttributeValue(ssrNamespace.GetName("screenSection"), screenSectionAttributeValue);
                node.Add(screenSectionConditionNode);
            }
        };
    }
}
