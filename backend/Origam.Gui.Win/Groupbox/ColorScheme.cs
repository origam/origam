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
            {
                //=========================================================
                //Setting color properties of button control for
                //Green color scheme
                //---------------------------------------------------------
                aCtrl.BackgroundBottomColor = Color.FromArgb(red: 193, green: 201, blue: 140);
                aCtrl.BackgroundTopColor = Color.FromArgb(red: 230, green: 233, blue: 208);
                aCtrl.BorderBottomColor = Color.FromArgb(red: 230, green: 233, blue: 208);
                aCtrl.BorderTopColor = Color.FromArgb(red: 193, green: 201, blue: 140);
                aCtrl.DefaultBorderColor = Color.FromArgb(red: 167, green: 168, blue: 127);
                aCtrl.DisabledFontColor = Color.FromArgb(red: 156, green: 147, blue: 113);
                aCtrl.DisbaledBottomColor = Color.FromArgb(red: 209, green: 215, blue: 170);
                aCtrl.DisabledTopColor = Color.FromArgb(red: 240, green: 242, blue: 227);
                aCtrl.FontColor = Color.FromArgb(red: 105, green: 110, blue: 26);
                aCtrl.PressedFontColor = Color.Black;
                break;
            }
            //---------------------------------------------------------
            case EnmColorScheme.Purple:
            {
                //=========================================================
                //Setting color properties of button control for
                //Purple color scheme
                //---------------------------------------------------------
                aCtrl.BackgroundBottomColor = Color.FromArgb(red: 183, green: 157, blue: 206);
                aCtrl.BackgroundTopColor = Color.FromArgb(red: 231, green: 222, blue: 239);
                aCtrl.BorderBottomColor = Color.FromArgb(red: 224, green: 215, blue: 233);
                aCtrl.BorderTopColor = Color.FromArgb(red: 193, green: 157, blue: 206);
                aCtrl.DefaultBorderColor = Color.FromArgb(red: 132, green: 100, blue: 161);
                aCtrl.DisabledFontColor = Color.FromArgb(red: 143, green: 116, blue: 156);
                aCtrl.DisbaledBottomColor = Color.FromArgb(red: 209, green: 192, blue: 210);
                aCtrl.DisabledTopColor = Color.FromArgb(red: 237, green: 231, blue: 230);
                aCtrl.FontColor = Color.FromArgb(red: 74, green: 30, blue: 115);
                aCtrl.PressedFontColor = Color.Black;
                break;
            }
            //---------------------------------------------------------
            case EnmColorScheme.Yellow:
            {
                //=========================================================
                //Setting color properties of button control for
                //Yellow color scheme
                //---------------------------------------------------------
                aCtrl.BackgroundBottomColor = Color.FromArgb(red: 194, green: 168, blue: 120);
                aCtrl.BackgroundTopColor = Color.FromArgb(red: 248, green: 245, blue: 224);
                aCtrl.BorderBottomColor = Color.FromArgb(red: 229, green: 219, blue: 196);
                aCtrl.BorderTopColor = Color.FromArgb(red: 194, green: 168, blue: 120);
                aCtrl.DefaultBorderColor = Color.FromArgb(red: 189, green: 153, blue: 74);
                aCtrl.DisabledFontColor = Color.FromArgb(red: 156, green: 147, blue: 113);
                aCtrl.DisbaledBottomColor = Color.FromArgb(red: 201, green: 177, blue: 135);
                aCtrl.DisabledTopColor = Color.FromArgb(red: 241, green: 236, blue: 212);
                aCtrl.FontColor = Color.FromArgb(red: 96, green: 83, blue: 43);
                aCtrl.PressedFontColor = Color.Black;
                break;
            }
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
            {
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
            }
            //---------------------------------------------------------
            case EnmColorScheme.Green:
            {
                //=========================================================
                // Setting color properties of container control for
                // Green color scheme
                //---------------------------------------------------------
                aCtrl.FontColor = Color.FromArgb(red: 57, green: 66, blue: 1);
                aCtrl.ShadowColor = Color.FromArgb(red: 142, green: 143, blue: 116);
                aCtrl.BorderTopColor = Color.FromArgb(red: 225, green: 225, blue: 183);
                aCtrl.BorderBottomColor = Color.FromArgb(red: 167, green: 168, blue: 127);
                aCtrl.BackgroundTopColor = Color.FromArgb(red: 245, green: 243, blue: 219);
                aCtrl.BackgroundBottomColor = Color.FromArgb(red: 214, green: 209, blue: 153);
                break;
            }
            //---------------------------------------------------------
            case EnmColorScheme.Purple:
            {
                //=========================================================
                // Setting color properties of container control for
                // Purple color scheme
                //---------------------------------------------------------
                aCtrl.FontColor = Color.FromArgb(red: 137, green: 101, blue: 163);
                aCtrl.ShadowColor = Color.FromArgb(red: 110, green: 92, blue: 121);
                aCtrl.BorderTopColor = Color.FromArgb(red: 234, green: 218, blue: 245);
                aCtrl.BorderBottomColor = Color.FromArgb(red: 191, green: 161, blue: 211);
                aCtrl.BackgroundTopColor = Color.FromArgb(red: 251, green: 246, blue: 255);
                aCtrl.BackgroundBottomColor = Color.FromArgb(red: 241, green: 229, blue: 249);
                break;
            }
            //---------------------------------------------------------
            default:
            {
                // For container control if other than Purple or Green
                // any other value is selected it throws an exception
                throw new InvalidColorSchemeException();
            }
        }
    }
}

///<summary>
/// This class define the exception which is thrown on invalid selection
///</summary>
public class InvalidColorSchemeException : Exception
{
    public InvalidColorSchemeException()
        : base(message: ResourceUtils.GetString(key: "ErrorColorScheme")) { }
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
