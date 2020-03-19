using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Origam.Extensions;
using Origam.Schema.WorkflowModel;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers
{
    class StateMachineEventParameterMappingScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = typeof(StateMachineEventParameterMapping).FullName;
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; } 
            = {"wfParameterTpe"};
        
        public StateMachineEventParameterMappingScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    node.RenameAttribute( "wfParameterTpe", "wfParameterType");
                })
            );
        }
    }
}