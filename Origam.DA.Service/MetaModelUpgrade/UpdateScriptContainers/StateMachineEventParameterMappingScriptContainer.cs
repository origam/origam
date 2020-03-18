using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Origam.Schema.WorkflowModel;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers
{
    class StateMachineEventParameterMappingScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = typeof(StateMachineEventParameterMapping).FullName;
        public override List<string> OldFullTypeNames { get; }
        public override Dictionary<string, string[]> OldPropertyNames { get; } 
            = new Dictionary<string, string[]>
        {
            {nameof(StateMachineEventParameterMapping.Type), new []{"wfParameterTpe"}}
        };
        
        public StateMachineEventParameterMappingScriptContainer() 
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"), 
                new Version("6.0.1"),
                (node, doc) =>
                {
                    XNamespace nameSpace = node.Name.Namespace;
                    var typeAttribute = node.Attribute(nameSpace.GetName("wfParameterTpe"));
                    if (typeAttribute == null) return;

                    string value = typeAttribute.Value;
                    typeAttribute.Remove();
                    
                    node.SetAttributeValue(nameSpace.GetName("wfParameterType"), value);
                }));
        }
    }
}