using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origam.Workbench.Editors
{
    class UiActionEditor: PropertyGridEditor
    {
        public override List<ToolStrip> GetToolStrips(int maxWidth = -1)
        {
            if (!showMenusInAppToolStrip) return new List<ToolStrip>();
            var actions = ActionsBuilder.BuildSubmenu(Content);
            var actionToolStrip = MakeLabeledToolStrip(actions, "Actions", maxWidth / 2);
            return new List<ToolStrip> { actionToolStrip };
        }
    }
}
