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

namespace Origam.Gui.UI;
public class NumberParser
{
    private readonly Type ValueType;
    private readonly Func<string, object> textParseFunc;
    private readonly IErrorReporter errorReporter;
    
    public NumberParser(Func<string,object> textParseFunc,
        IErrorReporter errorReporter) 
    {
        this.textParseFunc = textParseFunc;
        ValueType = GetValueType(textParseFunc); 
        this.errorReporter = errorReporter;
    }
    public object Parse(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        try
        {
            return textParseFunc.Invoke(text);
        } catch (OverflowException) 
        {
            // TODO: fix error message tooltip which does not show up above 
            // the textBox (commented lines below)
//                errorReporter.NotifyInputError($"The value {text} " +
//                                 $"is too big or too small for {ValueType.Name}");
            throw;
        } catch (FormatException)
        {
            errorReporter.NotifyInputError($"Cannot parse \"{text}\" to" +
                                           $" {ValueType.Name}");
            return "";
        }
    }
    private Type GetValueType(Func<string, object> textParseFunc){
        try
        {
           return  textParseFunc.Invoke("1").GetType();
        } 
        catch 
        {
            throw new ArgumentException(
                "textParseFunc cannot parse numeric values");
        }
    }
}
