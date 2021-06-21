using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers
{
    public class DataStructureColumnScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } =
            typeof(Origam.Schema.EntityModel.DataStructureColumn).FullName;

        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }

        public DataStructureColumnScriptContainer()
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"),
                new Version("6.0.1"),
                (node, doc) =>
                {
                    XNamespace oldNameSpace= "http://schemas.origam.com/Origam.Schema.EntityModel.DataStructureColumn/6.0.0";
                    XNamespace newNameSpace= "http://schemas.origam.com/Origam.Schema.EntityModel.DataStructureColumn/6.0.1";
                    var mappingAttribute = node.Attribute(oldNameSpace+"xmlMappingType");
                    if (mappingAttribute != null && mappingAttribute.Value == "Hidden")
                    {
                        mappingAttribute.Value = "Default";
                        node.Add(new XAttribute(newNameSpace+"hideInOutput", "true"));
                    }
                }
            ));
        }
    }
}