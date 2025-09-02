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
using System.Data;
using System.Xml;
using System.Xml.XPath;

namespace Origam.Rule.Xslt;

public class DataReaderXPathNavigator : XPathNavigator
{
    private XmlNameTable nameTable = new NameTable();
    private IDataReader dataReader = null;
    private string entityName = null;
    private int position = -1; // -1 root, 0 - entity element, 1... attribute
    public int Position => position;
    private bool wasMoveToFirstChildInvoked = false;
    public bool WasMoveToFirstChildInvoked => wasMoveToFirstChildInvoked;

    public DataReaderXPathNavigator(
        IDataReader dataReader,
        string entityName,
        int position,
        bool wasMoveToFirstChildInvoked
    )
    {
        this.dataReader = dataReader;
        this.entityName = entityName;
        this.position = position;
        this.wasMoveToFirstChildInvoked = wasMoveToFirstChildInvoked;
    }

    public override XmlNameTable NameTable => nameTable;
    public override XPathNodeType NodeType
    {
        get
        {
            if (position < 1)
            {
                return XPathNodeType.Element;
            }
            return XPathNodeType.Attribute;
        }
    }
    public override string LocalName
    {
        get
        {
            if (position == -1)
            {
                return "ROOT";
            }
            if (position == 0)
            {
                return entityName;
            }
            return dataReader.GetName(position - 1);
        }
    }
    public override string Name
    {
        get
        {
            if (position == -1)
            {
                return "ROOT";
            }
            if (position == 0)
            {
                return entityName;
            }
            return dataReader.GetName(position - 1);
        }
    }
    public override string NamespaceURI => String.Empty;
    public override string Prefix => throw new NotImplementedException();
    public override string BaseURI => throw new NotImplementedException();
    public override bool IsEmptyElement => throw new NotImplementedException();
    public override string Value
    {
        get
        {
            if ((position < 1) || (dataReader.IsDBNull(position - 1)))
            {
                return string.Empty;
            }
            Type fieldType = dataReader.GetFieldType(position - 1);
            if (fieldType == typeof(int))
            {
                return XmlConvert.ToString(dataReader.GetInt32(position - 1));
            }
            else if (fieldType == typeof(Guid))
            {
                return XmlConvert.ToString(dataReader.GetGuid(position - 1));
            }
            else if (fieldType == typeof(long))
            {
                return XmlConvert.ToString(dataReader.GetInt32(position - 1));
            }
            else if (fieldType == typeof(decimal))
            {
                return XmlConvert.ToString(dataReader.GetDecimal(position - 1));
            }
            else if (fieldType == typeof(bool))
            {
                return XmlConvert.ToString(dataReader.GetBoolean(position - 1));
            }
            else if (fieldType == typeof(DateTime))
            {
                return XmlConvert.ToString(
                    dataReader.GetDateTime(position - 1),
                    XmlDateTimeSerializationMode.RoundtripKind
                );
            }
            else
            {
                return dataReader.GetValue(position - 1).ToString();
            }
        }
    }

    public override XPathNavigator Clone()
    {
        return new DataReaderXPathNavigator(
            dataReader,
            entityName,
            position,
            wasMoveToFirstChildInvoked
        );
    }

    public override bool IsSamePosition(XPathNavigator other)
    {
        throw new NotImplementedException();
    }

    public override bool MoveTo(XPathNavigator other)
    {
        if ((other as DataReaderXPathNavigator) == null)
        {
            return false;
        }
        wasMoveToFirstChildInvoked = (other as DataReaderXPathNavigator).wasMoveToFirstChildInvoked;
        position = (other as DataReaderXPathNavigator).Position;
        return true;
    }

    public override bool MoveToFirstAttribute()
    {
        if (position == -1)
        {
            return false;
        }
        if ((position == 0) && (dataReader.FieldCount > 0))
        {
            position = 1;
            return true;
        }
        return false;
    }

    public override bool MoveToFirstChild()
    {
        if ((position == -1) && (wasMoveToFirstChildInvoked || dataReader.Read()))
        {
            wasMoveToFirstChildInvoked = true;
            position = 0;
            return true;
        }
        return false;
    }

    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
    {
        throw new NotImplementedException();
    }

    public override bool MoveToId(string id)
    {
        throw new NotImplementedException();
    }

    public override bool MoveToNext()
    {
        if (position == 0)
        {
            return dataReader.Read();
        }
        return false;
    }

    public override bool MoveToNextAttribute()
    {
        if (position < 1)
        {
            return false;
        }
        if ((position + 1) < dataReader.FieldCount)
        {
            position++;
            return true;
        }
        return false;
    }

    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
    {
        return false;
    }

    public override bool MoveToParent()
    {
        if (position == -1)
        {
            return false;
        }
        position = position > 0 ? 0 : -1;
        return true;
    }

    public override bool MoveToPrevious()
    {
        return false;
    }
}
