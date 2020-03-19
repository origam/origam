using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers
{
    class EntityUIActionScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = typeof(EntityUIAction).FullName;
        public override List<string> OldFullTypeNames { get; }

        public override Dictionary<string, string[]> OldPropertyXmlNames { get; } 
            = new Dictionary<string, string[]>
            {
                {"", new []{"screen", "screenSection"}}
            };

        public EntityUIActionScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.1.0"),
                (node, doc) =>
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
                        XElement screenConditionNode = new XElement( srNamespace.GetName("ScreenCondition"));
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
                        XElement screenSectionConditionNode = new XElement( ssrNamespace.GetName("ScreenSectionCondition"));
                        screenSectionConditionNode.SetAttributeValue(persistenceNamespace.GetName("id"), Guid.NewGuid().ToString());
                        screenSectionConditionNode.SetAttributeValue(asiNamespace.GetName("name"), "ScreenSectionCondition1");
                        screenSectionConditionNode.SetAttributeValue(ssrNamespace.GetName("screenSection"), screenSectionAttributeValue);
                        node.Add(screenSectionConditionNode);
                    }
                }));
        }
    }
}