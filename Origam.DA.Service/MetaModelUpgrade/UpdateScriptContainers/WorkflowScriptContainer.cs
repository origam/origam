using System;
using System.Collections.Generic;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers
{
    public class WorkflowScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } =
            typeof(Schema.WorkflowModel.Workflow).FullName;

        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }

        public WorkflowScriptContainer()
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"),
                new Version("6.0.1"),
                (node, doc) =>
                    RemoveAttribute(node, "traceLevel")));
        }
    }  
    
    public class AbstractWorkflowStepScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } =
            typeof(Schema.WorkflowModel.AbstractWorkflowStep).FullName;

        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }

        public AbstractWorkflowStepScriptContainer()
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"),
                new Version("6.0.1"),
                (node, doc) =>
                    RemoveAttribute(node, "traceLevel")));
        }
    }
}