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
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

namespace Origam.Workflow;

/// <summary>
/// Summary description for TextReaderAgentSettings.
/// </summary>
[Serializable()]
public class TextReaderOptions
{
    public TextReaderOptions() { }

    private int _ignoreFirst = 0;
    private int _ignoreLast = 0;
    private string _separator = ",";
    private Hashtable _cache = null;
    private TextReaderOptionsField[] _fieldOptions;

    public static TextReaderOptions Deserialize(XmlDocument doc)
    {
        if (doc == null)
        {
            return new TextReaderOptions();
        }

        XmlSerializer ser = new XmlSerializer(typeof(TextReaderOptions));
        XmlNodeReader reader = new XmlNodeReader(doc);
        return (TextReaderOptions)ser.Deserialize(reader);
    }

    [XmlAttribute()]
    public int IgnoreFirst
    {
        get { return _ignoreFirst; }
        set { _ignoreFirst = value; }
    }

    [XmlAttribute()]
    public int IgnoreLast
    {
        get { return _ignoreLast; }
        set { _ignoreLast = value; }
    }

    [XmlAttribute()]
    public string Separator
    {
        get { return _separator; }
        set { _separator = value; }
    }

    [XmlArrayItem("Option", typeof(TextReaderOptionsField))]
    public TextReaderOptionsField[] FieldOptions
    {
        get { return _fieldOptions; }
        set { _fieldOptions = value; }
    }

    public TextReaderOptionsField GetFieldOption(string fieldName)
    {
        if (_cache == null)
        {
            _cache = new Hashtable();
            if (this.FieldOptions != null)
            {
                foreach (TextReaderOptionsField fld in this.FieldOptions)
                {
                    _cache[fld.Name] = fld;
                }
            }
        }
        return _cache[fieldName] as TextReaderOptionsField;
    }
}
