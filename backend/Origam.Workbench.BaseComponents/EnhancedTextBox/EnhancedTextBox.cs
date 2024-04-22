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
using System.Windows.Forms;
using System.Globalization;

namespace Origam.Gui.UI;

public class EnhancedTextBox : TextBox
{
    private Type dataType = typeof(string);
    private IFormatter formatter;
    private string customFormat;
    private static readonly object ValueChangedEventKey = new object();
    private readonly Func<DateTime> timeNowFunc;
        
    public EnhancedTextBox(Func<DateTime> timeNowFunc = null)
    {
            KeyDown += OnKeyDown;
            KeyPress += OnKeyPress;
            Leave += OnLeave;
            LostFocus += OnLeave;
            formatter = new StringFormatter(this);
            this.timeNowFunc = timeNowFunc ?? (() => DateTime.Now);
        }

    public Type DataType
    {
        get => dataType;

        set
        {
                if (value == typeof(DateTime))
                {
                    formatter = new DatetimeFormatter(this,customFormat,timeNowFunc);
                }
                else if (value == typeof(string))
                {
                    formatter = new StringFormatter(this);
                }
                else if (value == typeof(long))
                {
                    formatter = new WholeNumberFormattrer(
                        textBox: this,
                        customFormat: customFormat,
                        textParseFunc: text => long.Parse(
                            text,
                            Formatter.WholeNumberStyle,
                            CurrentNumFormat)
                    );
                }
                else if (value == typeof(int))
                {
                    formatter = new WholeNumberFormattrer(
                        textBox: this,
                        customFormat: customFormat,
                        textParseFunc: text => int.Parse(
                            text, 
                            Formatter.WholeNumberStyle,
                            CurrentNumFormat)
                    );
                }
                else if (value == typeof(decimal))
                {
                    formatter = new RealNumberFormatter(
                        textBox: this,
                        format: customFormat,
                        textParseFunc: text => decimal.Parse(
                            text,  
                            Formatter.RealNumberStyle, 
                            CurrentNumFormat)
                    );
                }
                else if (value == typeof(float))
                {
                    formatter = new RealNumberFormatter(
                        textBox: this,
                        format: customFormat,
                        textParseFunc: text => float.Parse(
                            text,  
                            Formatter.RealNumberStyle, 
                            CurrentNumFormat)
                    );
                }
                else if (value == typeof(double))
                {
                    formatter = new RealNumberFormatter(
                        textBox: this,
                        format: customFormat,
                        textParseFunc: text => double.Parse(
                            text,  
                            Formatter.RealNumberStyle, 
                            CurrentNumFormat)
                    );
                }
                else
                {
                    formatter = new StringFormatter(this);
                }
                dataType = value;
            }
    }

    private  NumberFormatInfo CurrentNumFormat 
        => CultureInfo.CurrentCulture.NumberFormat;

    public object Value
    {
        get => formatter.GetValue();
        set
        {
                if (value == null)
                {
                    Text = "";
                    return;
                }
                Text = value.ToString();
                
                formatter.OnLeave(null, EventArgs.Empty);
                OnValueChanged(EventArgs.Empty);
            }
    }

    public string CustomFormat
    {
        get => customFormat;
        set
        {
                customFormat = value;
                DataType = dataType; // updates format in validator
            }
    }

    public event EventHandler ValueChanged
    {
        add => Events.AddHandler(ValueChangedEventKey, value);
        remove => Events.RemoveHandler(ValueChangedEventKey, value);
    }

    private void OnValueChanged(EventArgs e)
    {
            if (!(Events[ValueChangedEventKey] is EventHandler eventHandler))
            {
                return;
            }
            eventHandler(this, e);
        }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
            formatter.OnKeyDown(sender, e);
        }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
            formatter.OnKeyPress(sender, e);
        }

    private void OnLeave(object sender, EventArgs e)
    {
            formatter.OnLeave(sender, e);
            OnValueChanged(e);
        }
}