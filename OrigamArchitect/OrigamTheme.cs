using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace OrigamArchitect
{
    public class OrigamTheme : VS2015ThemeBase
    {
        public OrigamTheme()
            : base(Decompress(Resources.origam_vstheme))
        {
        }
    }
}
