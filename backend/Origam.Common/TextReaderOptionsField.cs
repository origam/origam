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
using System.Xml.Serialization;

namespace Origam.Workflow;

/// <summary>
/// Summary description for TextReaderAgentSeparator.
/// </summary>
[Serializable()]
public class TextReaderOptionsField
{
    private string _name;
    private string _culture;
    private string _format;
    private bool _isQuoted = false;
    private bool _isOptional = false;
    private string _quoteChar;
    private string _decimalSeparator;
    private int _length = 0;
    private string _nullValue = null;
    private bool _isIgnored = false;
    private string[] _alternativeFormats;

    public TextReaderOptionsField() { }

    [XmlAttribute()]
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    [XmlAttribute()]
    public string Format
    {
        get { return _format; }
        set { _format = value; }
    }

    [XmlArrayItem("Format", typeof(string))]
    public string[] AlternativeFormats
    {
        get { return _alternativeFormats; }
        set { _alternativeFormats = value; }
    }

    [XmlAttribute()]
    public string DecimalSeparator
    {
        get { return _decimalSeparator; }
        set { _decimalSeparator = value; }
    }

    [XmlAttribute()]
    public string Culture
    {
        get { return _culture; }
        set { _culture = value; }
    }

    [XmlAttribute()]
    public bool IsQuoted
    {
        get { return _isQuoted; }
        set { _isQuoted = value; }
    }

    [XmlAttribute()]
    public bool IsOptional
    {
        get { return _isOptional; }
        set { _isOptional = value; }
    }

    [XmlAttribute()]
    public bool IsIgnored
    {
        get { return _isIgnored; }
        set { _isIgnored = value; }
    }

    [XmlAttribute()]
    public string QuoteChar
    {
        get { return _quoteChar; }
        set { _quoteChar = value; }
    }

    [XmlAttribute()]
    public int Length
    {
        get { return _length; }
        set { _length = value; }
    }

    [XmlAttribute()]
    public string NullValue
    {
        get { return _nullValue; }
        set { _nullValue = value; }
    }

    public CultureInfo GetCulture()
    {
        if (_culture == null)
        {
            return CultureInfo.InvariantCulture;
        }

        return new CultureInfo(_culture);
    }

    public string[] Formats
    {
        get
        {
            if (this.AlternativeFormats == null || this.AlternativeFormats.Length == 0)
            {
                if (this.Format == null)
                {
                    return new string[0];
                }

                return new string[] { this.Format };
            }

            if (this.Format == null)
            {
                return this.AlternativeFormats;
            }
            string[] result = new string[this.AlternativeFormats.LongLength + 1];
            result[0] = this.Format;
            this.AlternativeFormats.CopyTo(result, 1);

            return result;
        }
    }
}
