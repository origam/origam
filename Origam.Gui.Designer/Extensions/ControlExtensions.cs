using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.Gui.Designer.Extensions
{
    public static class ControlExtensions
    {
        public static IEnumerable<Control> GetAllControls(this Control control)
        {
            foreach (Control child in control.Controls)
            {
                yield return child;
                foreach (Control innerChild in GetAllControls(child))
                {
                    yield return innerChild;
                }
            }
        }
    }
}