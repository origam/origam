using System;
using System.Collections.Generic;
using System.Text;
using Origam.Schema.GuiModel;

namespace Origam.DA.Service.MetaModelUpgrade.UpdateScriptContainers
{
    class WebReportScriptContainer : UpgradeScriptContainer
    {
        public override string FullTypeName { get; } = typeof(WebReport).FullName;
        public override List<string> OldFullTypeNames { get; }
        public override string[] OldPropertyXmlNames { get; }

        public WebReportScriptContainer()
        {
            upgradeScripts.Add(new UpgradeScript(
                new Version("6.0.0"),
                new Version("6.0.1"),
                (node, doc) => { }));
        }
    }
}
