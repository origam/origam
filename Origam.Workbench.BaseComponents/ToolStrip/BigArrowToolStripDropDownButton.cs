using Origam.Workbench.BaseComponents;
using System.Windows.Forms;

namespace Origam.Gui
{
    public class BigArrowToolStripDropDownButton : ToolStripDropDownButton
    {
        public BigArrowToolStripDropDownButton()
        {
            ShowDropDownArrow = false;
            ImageScaling = ToolStripItemImageScaling.None;
            Image = ImageRes.Arrow;
        }
    }
}
