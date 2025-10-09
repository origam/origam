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

#region license
// Copyright 2004 Shouvik - https://www.codeproject.com/Articles/8103/Creating-some-cool-buttons-and-groupboxes
#endregion

using System;
using System.Drawing;
using Origam.UI;

namespace Origam.Gui.Win;

public enum EnmColorScheme
{
    Purple,
    Green,
    Yellow,
    Origam,
}

/// <summary>
///  This class works as a common point for all the controls to
///  implement the color scheme
/// </summary>
internal class ColorScheme
{
    private EnmColorScheme oClrScheme;

    public ColorScheme(EnmColorScheme aoColorScheme)
    {
        //
        // TODO: Add constructor logic here
        //
        oClrScheme = aoColorScheme;
    }

    ///<summary>
    /// This method sets the values of different color properties
    /// for controls of IGradientButtonColor Type
    /// </summary>
    internal void SetColorScheme(IGradientButtonColor aCtrl)
    {
        switch (oClrScheme)
        {
            case EnmColorScheme.Green:
                //=========================================================
                //Setting color properties of button control for
                //Green color scheme
                //---------------------------------------------------------
                aCtrl.BackgroundBottomColor = Color.FromArgb(193, 201, 140);
                aCtrl.BackgroundTopColor = Color.FromArgb(230, 233, 208);
                aCtrl.BorderBottomColor = Color.FromArgb(230, 233, 208);
                aCtrl.BorderTopColor = Color.FromArgb(193, 201, 140);
                aCtrl.DefaultBorderColor = Color.FromArgb(167, 168, 127);
                aCtrl.DisabledFontColor = Color.FromArgb(156, 147, 113);
                aCtrl.DisbaledBottomColor = Color.FromArgb(209, 215, 170);
                aCtrl.DisabledTopColor = Color.FromArgb(240, 242, 227);
                aCtrl.FontColor = Color.FromArgb(105, 110, 26);
                aCtrl.PressedFontColor = Color.Black;
                break;
            //---------------------------------------------------------
            case EnmColorScheme.Purple:
                //=========================================================
                //Setting color properties of button control for
                //Purple color scheme
                //---------------------------------------------------------
                aCtrl.BackgroundBottomColor = Color.FromArgb(183, 157, 206);
                aCtrl.BackgroundTopColor = Color.FromArgb(231, 222, 239);
                aCtrl.BorderBottomColor = Color.FromArgb(224, 215, 233);
                aCtrl.BorderTopColor = Color.FromArgb(193, 157, 206);
                aCtrl.DefaultBorderColor = Color.FromArgb(132, 100, 161);
                aCtrl.DisabledFontColor = Color.FromArgb(143, 116, 156);
                aCtrl.DisbaledBottomColor = Color.FromArgb(209, 192, 210);
                aCtrl.DisabledTopColor = Color.FromArgb(237, 231, 230);
                aCtrl.FontColor = Color.FromArgb(74, 30, 115);
                aCtrl.PressedFontColor = Color.Black;
                break;
            //---------------------------------------------------------
            case EnmColorScheme.Yellow:
                //=========================================================
                //Setting color properties of button control for
                //Yellow color scheme
                //---------------------------------------------------------
                aCtrl.BackgroundBottomColor = Color.FromArgb(194, 168, 120);
                aCtrl.BackgroundTopColor = Color.FromArgb(248, 245, 224);
                aCtrl.BorderBottomColor = Color.FromArgb(229, 219, 196);
                aCtrl.BorderTopColor = Color.FromArgb(194, 168, 120);
                aCtrl.DefaultBorderColor = Color.FromArgb(189, 153, 74);
                aCtrl.DisabledFontColor = Color.FromArgb(156, 147, 113);
                aCtrl.DisbaledBottomColor = Color.FromArgb(201, 177, 135);
                aCtrl.DisabledTopColor = Color.FromArgb(241, 236, 212);
                aCtrl.FontColor = Color.FromArgb(96, 83, 43);
                aCtrl.PressedFontColor = Color.Black;
                break;
            //---------------------------------------------------------
        }
    }

    ///<summary>
    /// This method sets the values of different color properties
    /// for controls of IGradientContainer Type
    ///</summary>
    internal void SetColorScheme(IGradientContainer aCtrl)
    {
        switch (oClrScheme)
        {
            case EnmColorScheme.Origam:
                //=========================================================
                // Setting color properties of container control for
                // Green color scheme
                //---------------------------------------------------------
                aCtrl.FontColor = OrigamColorScheme.GroupBoxFontColor;
                aCtrl.ShadowColor = OrigamColorScheme.GroupBoxShadowColor;
                aCtrl.BorderTopColor = OrigamColorScheme.GroupBoxBorderTopColor;
                aCtrl.BorderBottomColor = OrigamColorScheme.GroupBoxBorderBottomColor;
                aCtrl.BackgroundTopColor = OrigamColorScheme.GroupBoxBackgroundTopColor;
                aCtrl.BackgroundBottomColor = OrigamColorScheme.GroupBoxBackgroundBottomColor;
                break;
            //---------------------------------------------------------
            case EnmColorScheme.Green:
                //=========================================================
                // Setting color properties of container control for
                // Green color scheme
                //---------------------------------------------------------
                aCtrl.FontColor = Color.FromArgb(57, 66, 1);
                aCtrl.ShadowColor = Color.FromArgb(142, 143, 116);
                aCtrl.BorderTopColor = Color.FromArgb(225, 225, 183);
                aCtrl.BorderBottomColor = Color.FromArgb(167, 168, 127);
                aCtrl.BackgroundTopColor = Color.FromArgb(245, 243, 219);
                aCtrl.BackgroundBottomColor = Color.FromArgb(214, 209, 153);
                break;
            //---------------------------------------------------------
            case EnmColorScheme.Purple:
                //=========================================================
                // Setting color properties of container control for
                // Purple color scheme
                //---------------------------------------------------------
                aCtrl.FontColor = Color.FromArgb(137, 101, 163);
                aCtrl.ShadowColor = Color.FromArgb(110, 92, 121);
                aCtrl.BorderTopColor = Color.FromArgb(234, 218, 245);
                aCtrl.BorderBottomColor = Color.FromArgb(191, 161, 211);
                aCtrl.BackgroundTopColor = Color.FromArgb(251, 246, 255);
                aCtrl.BackgroundBottomColor = Color.FromArgb(241, 229, 249);
                break;
            //---------------------------------------------------------
            default:
                // For container control if other than Purple or Green
                // any other value is selected it throws an exception
                throw new InvalidColorSchemeException();
        }
    }
}

///<summary>
/// This class define the exception which is thrown on invalid selection
///</summary>
public class InvalidColorSchemeException : Exception
{
    public InvalidColorSchemeException()
        : base(ResourceUtils.GetString("ErrorColorScheme")) { }
}

///<summary>
/// This interface defines properties
/// for control that have diffrent colors
/// is disabled mode i.e. ElongatedButton
///</summary>
internal interface IGradientDisabledColor
{
    Color DisabledFontColor { get; set; }
    Color DisbaledBottomColor { get; set; }
    Color DisabledTopColor { get; set; }
}

///<summary>
/// This interface defines property
/// for the color of the text on
/// the control
///</summary>
internal interface IFontColor
{
    Color FontColor { get; set; }
}

///<summary>
/// This interface defines properties
/// to set the control background
/// Gradient's top color and bottom color
///</summary>
internal interface IGradientBackgroundColor
{
    Color BackgroundBottomColor { get; set; }
    Color BackgroundTopColor { get; set; }
}

///<summary>
/// This interface defines properties
/// to set control's Gradient Border's
/// Top color and Bottom Color
///</summary>
internal interface IGradientBorderColor
{
    Color BorderTopColor { get; set; }
    Color BorderBottomColor { get; set; }
}

///<summary>
/// This interface combines the interfaces
/// needed for button controls and add button
/// specific properties
///</summary>
internal interface IGradientButtonColor
    : IFontColor,
        IGradientDisabledColor,
        IGradientBackgroundColor,
        IGradientBorderColor
{
    Color PressedFontColor { get; set; }
    Color DefaultBorderColor { get; set; }
}

///<summary>
/// This interface combines the interfaces
/// needed for container controls and add
/// container specific property
///</summary>
internal interface IGradientContainer : IFontColor, IGradientBackgroundColor, IGradientBorderColor
{
    Color ShadowColor { get; set; }
}
