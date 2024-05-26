using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers;

public class EntitySecurityRuleScriptContainer : UpgradeScriptContainer
{
    public override string FullTypeName => "Origam.Schema.EntityModel.EntitySecurityRule";
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override List<string> OldFullTypeNames { get; }

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public override string[] OldPropertyXmlNames { get; }

    public EntitySecurityRuleScriptContainer()
    {
        upgradeScripts.Add(
            new UpgradeScript(
                fromVersion: new Version("6.0.0"),
                toVersion: new Version("6.1.0"),
                transformation: (node, doc) =>
                {
                    XNamespace newNameSpace= "http://schemas.origam.com/Origam.Schema.EntityModel.EntitySecurityRule/6.1.0";
                    node.Add(new XAttribute(
                            newNameSpace + "exportCredential", false));
                }));
    }
}