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

namespace Origam.Gui.UI
{
    class RealNumberFormatter: Formatter
    {

        private readonly NumberParser numberParser;
        public RealNumberFormatter(TextBox textBox, string format,
            Func<string,object> textParseFunc)
            : base(textBox,format)
        {
            numberParser = new NumberParser(textParseFunc,errorReporter);
        }

        private string Format => 
            string.IsNullOrEmpty(customFormat) ? "###,###,###.######" : customFormat;

        private char DecimalSeparator 
            => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];


        public override void OnLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Text)) return;

            var value = numberParser.Parse(Text);
            Text = string.Format(Culture, "{0:"+Format+"}", value);
        }

        public override object GetValue()
        {
            return numberParser.Parse(Text);
        }

        protected override bool IsValidChar(char input)
        {
            if (base.IsValidChar(input)) return true;
            
            return char.IsDigit(input) ||
                   input == Minus ||
                   input == ThousandsSeparator ||
                   input == DecimalSeparator;
        }
    }
}