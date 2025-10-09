#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Drawing;

namespace Origam.UI;

/// <summary>
/// Summary description for ColorScheme.
/// </summary>
public class OrigamColorScheme
{
    static OrigamColorScheme() { }

#if ORIGAM_CLIENT
    private static Color _windowBackgroundColor = Color.FloralWhite;
    public static Color MdiBackColor = System.Drawing.Color.FromArgb(237, 231, 217);
    public static Color MdiForeColor = Color.Black;
    public static Color PropertyGridHeaderColor = MdiBackColor;
    public static Color ToolbarBaseColor = _windowBackgroundColor;
    public static Color ToolbarHighlightColor = Color.FromArgb(255, 171, 63);
    public static Color DocumentTabInactiveBackBegin = Color.White;
    public static Color DocumentTabInactiveBackEnd = Color.White;
    public static Color DocumentTabInactiveEdge = Color.FromArgb(172, 168, 153);
    public static Color TitleActiveStartColor = Color.FromArgb(255, 217, 170);
    public static Color TitleActiveMiddleEndColor = Color.FromArgb(255, 187, 132);
    public static Color TitleActiveMiddleStartColor = Color.FromArgb(255, 171, 63);
    public static Color TitleActiveEndColor = Color.FromArgb(254, 225, 122);
    public static Color TitleInactiveStartColor = TitleActiveStartColor;
    public static Color TitleInactiveMiddleEndColor = TitleActiveStartColor;
    public static Color TitleInactiveMiddleStartColor = TitleActiveStartColor;
    public static Color TitleInactiveEndColor = Color.FloralWhite;
    public static Color TitleActiveForeColor = Color.FromArgb(120, 54, 0);
    public static Color TitleInactiveForeColor = TitleActiveForeColor;
    public static Color FormBackgroundColor = _windowBackgroundColor;
    public static Color FormLoadingStatusColor = System.Drawing.Color.FromArgb(227, 170, 121); //Color.DarkKhaki;
    public static Color LinkColor = Color.Blue;
    public static Color GridAlternatingBackColor = Color.LightGoldenrodYellow;
    public static Color GridForeColor = Color.Black;
    public static Color GridLineColor = Color.PaleGoldenrod;
    public static Color GridHeaderBackColor = Color.FromArgb(218, 136, 62);
    public static Color GridHeaderForeColor = Color.White;
    public static Color GridSelectionBackColor = Color.FromArgb(254, 225, 122);
    public static Color GridSelectionForeColor = Color.Black;
    public static Color GroupBoxFontColor = Color.Black;
    public static Color GroupBoxShadowColor = Color.FromArgb(193, 182, 137);
    public static Color GroupBoxBorderTopColor = Color.FromArgb(232, 219, 162);
    public static Color GroupBoxBorderBottomColor = Color.FromArgb(186, 174, 119);
    public static Color GroupBoxBackgroundTopColor = Color.FromArgb(247, 233, 202);
    public static Color GroupBoxBackgroundBottomColor = Color.FromArgb(229, 200, 128);
    public static Color ButtonBackColor = System.Drawing.Color.FromArgb(227, 170, 121); //System.Drawing.Color.FromArgb(214, 203, 111);
    public static Color ButtonForeColor = Color.Black;
    public static Color SplitterBackColor = ButtonBackColor; //Color.FromArgb(193, 201, 140);
    public static Color TabActiveStartColor = TitleActiveStartColor;
    public static Color TabActiveEndColor = TitleActiveStartColor;
    public static Color TabInactiveStartColor = SystemColors.ControlLightLight;
    public static Color TabInactiveEndColor = SystemColors.ControlLight;
    public static Color TabActiveForeColor = TitleActiveForeColor;
    public static Color TabInactiveForeColor = TitleInactiveForeColor;
    public static Color DateTimePickerBorderColor = GroupBoxShadowColor;
    public static Color DateTimePickerForeColor = Color.Black;
    public static Color DateTimePickerBackColor = GridAlternatingBackColor;
    public static Color DateTimePickerTitleBackColor = GridHeaderBackColor;
    public static Color DateTimePickerTitleForeColor = GridHeaderForeColor;
    public static Color DateTimePickerTrailingForeColor = Color.DimGray;
    public static Color FilterPanelActiveBackColor = TitleActiveStartColor;
    public static Color FilterPanelInactiveBackColor = _windowBackgroundColor;
    public static Color FilterOperatorActiveBackColor = GridHeaderBackColor;
    public static Color FilterOperatorActiveForeColor = System.Drawing.Color.White;
#else
    private static Color _windowBackgroundColor = System.Drawing.Color.FromArgb(229, 229, 229);
    public static Color MdiBackColor = _windowBackgroundColor;
    public static Color PropertyGridHeaderColor = _windowBackgroundColor;
    public static Color MdiForeColor = Color.Black;
    public static Color ToolbarBaseColor = _windowBackgroundColor;
    public static Color ToolbarHighlightColor = MdiBackColor;
    public static Color DocumentTabInactiveBackBegin = Color.White;
    public static Color DocumentTabInactiveBackEnd = Color.White;
    public static Color DocumentTabInactiveEdge = MdiBackColor;
    public static Color TitleActiveStartColor = Color.FromArgb(4, 139, 168);
    public static Color TitleActiveMiddleEndColor = TitleActiveStartColor;
    public static Color TitleActiveMiddleStartColor = TitleActiveStartColor;
    public static Color TitleActiveEndColor = TitleActiveStartColor;
    public static Color TitleInactiveStartColor = TitleActiveStartColor;
    public static Color TitleInactiveMiddleEndColor = TitleActiveStartColor;
    public static Color TitleInactiveMiddleStartColor = TitleActiveStartColor;
    public static Color TitleInactiveEndColor = TitleActiveStartColor;
    public static Color TitleActiveForeColor = Color.White;
    public static Color TitleInactiveForeColor = TitleActiveForeColor;
    public static Color FormBackgroundColor = _windowBackgroundColor;
    public static Color FormLoadingStatusColor = Color.FromArgb(76, 76, 76);
    public static Color LinkColor = Color.Blue;
    public static Color GridAlternatingBackColor = Color.FromArgb(247, 247, 247);
    public static Color GridForeColor = Color.Black;
    public static Color GridLineColor = _windowBackgroundColor;
    public static Color GridHeaderBackColor = _windowBackgroundColor;
    public static Color GridHeaderForeColor = Color.Black;
    public static Color GridSelectionBackColor = Color.FromArgb(204, 204, 204);
    public static Color GridSelectionForeColor = Color.Black;
    public static Color GroupBoxFontColor = Color.Black;
    public static Color GroupBoxShadowColor = Color.FromArgb(206, 206, 206);
    public static Color GroupBoxBorderTopColor = GroupBoxShadowColor;
    public static Color GroupBoxBorderBottomColor = GroupBoxShadowColor;
    public static Color GroupBoxBackgroundTopColor = GroupBoxShadowColor;
    public static Color GroupBoxBackgroundBottomColor = GroupBoxShadowColor;
    public static Color ButtonBackColor = Color.FromArgb(102, 102, 102);
    public static Color ButtonForeColor = Color.White;
    public static Color SplitterBackColor = TitleActiveStartColor; //Color.FromArgb(193, 201, 140);
    public static Color TabActiveStartColor = TitleActiveStartColor;
    public static Color TabActiveEndColor = TitleActiveStartColor;
    public static Color TabInactiveStartColor = Color.FromArgb(102, 102, 102);
    public static Color TabInactiveEndColor = TabInactiveStartColor;
    public static Color TabActiveForeColor = TitleActiveForeColor;
    public static Color TabInactiveForeColor = TitleInactiveForeColor;
    public static Color DateTimePickerBorderColor = GroupBoxShadowColor;
    public static Color DateTimePickerForeColor = Color.Black;
    public static Color DateTimePickerBackColor = GridAlternatingBackColor;
    public static Color DateTimePickerTitleBackColor = GridHeaderBackColor;
    public static Color DateTimePickerTitleForeColor = GridHeaderForeColor;
    public static Color DateTimePickerTrailingForeColor = Color.DimGray;
    public static Color FilterPanelActiveBackColor = TitleActiveStartColor;
    public static Color FilterPanelInactiveBackColor = _windowBackgroundColor;
    public static Color FilterOperatorActiveBackColor = GridHeaderBackColor;
    public static Color FilterOperatorActiveForeColor = System.Drawing.Color.White;
#endif
    public static Color DirtyColor = Color.FromArgb(241, 91, 71);
}
