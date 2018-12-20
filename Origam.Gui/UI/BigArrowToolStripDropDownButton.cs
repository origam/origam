using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origam.Gui
{
    public class BigArrowToolStripDropDownButton : ToolStripDropDownButton
    {
        public BigArrowToolStripDropDownButton()
        {
            ShowDropDownArrow = false;
            ImageScaling = ToolStripItemImageScaling.None;
            Image = Images.Arrow;
        }
    }
}
