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

using System;
using System.Globalization;
using System.Windows.Forms;

namespace Origam.Gui.UI;
internal abstract class Formatter : IFormatter
{
    public const NumberStyles RealNumberStyle = 
        NumberStyles.AllowLeadingSign 
      | NumberStyles.AllowThousands 
      | NumberStyles.AllowDecimalPoint;
    public const NumberStyles WholeNumberStyle =
        NumberStyles.AllowThousands 
      | NumberStyles.AllowLeadingSign;
    private const char Backspace = (char)8;
    private const char Delete = (char)31;
    protected const char Minus = '-';
    private const char CtrlC = (char) 3;
    private const char CtrlV = (char) 22;
    private const char CtrlA = (char) 1;
    protected readonly IErrorReporter errorReporter;
    protected readonly string customFormat;
    private readonly TextBox textBox;
    
    
    protected Formatter(TextBox textBox, string customFormat)
    {
        this.textBox = textBox;
        this.customFormat = customFormat;
        this.errorReporter = new ErrorReporter(textBox);
    }
    protected static CultureInfo Culture => CultureInfo.CurrentCulture;
    protected char ThousandsSeparator
    {
        get
        {
            var separator = Culture.NumberFormat.NumberGroupSeparator[0];
            return separator == (char)160 ? ' ' : separator;
        }
    }
    
    public void OnKeyPress(object sender, KeyPressEventArgs e)
    {
        if (!IsValidChar(e.KeyChar))
        {
            NotifyInputError($"\"{e.KeyChar}\" is not a valid character here.");
            e.Handled = true;
        }
    }
    
    public void OnKeyDown(object sender, KeyEventArgs e)
    {
    }
    protected void NotifyInputError(string message)
    {
        errorReporter.NotifyInputError(message);
    }
    public abstract void OnLeave(object sender, EventArgs e);
    public abstract object GetValue();
    protected virtual bool IsValidChar(char input)
    {
        return input == CtrlV ||
               input == CtrlC ||
               input == CtrlA ||
               input == Backspace ||
               input == Delete ;
    }
    protected string Text {
        get => textBox.Text;
        set => textBox.Text = value;
    }
}
