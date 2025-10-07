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
using Origam.UI;

namespace Origam.Schema;

/// <summary>
/// Summary description for SchemaItemProviderGroup.
/// </summary>
public class SchemaItemProviderGroup : IBrowserNode2, IComparable
{
    private string _id;
    private string _icon;
    private string _text;
    private BrowserNodeCollection _children = new BrowserNodeCollection();
    private int _order;

    public SchemaItemProviderGroup(string id, string text, string icon, int order)
    {
        _id = id;
        _text = text;
        _icon = icon;
        _order = order;
    }

    #region IBrowserNode2 Members
    public bool CanMove(IBrowserNode2 newNode)
    {
        return false;
    }

    public IBrowserNode2 ParentNode
    {
        get { return null; }
        set
        {
            // TODO:  Add SchemaItemProviderGroup.ParentNode setter implementation
        }
    }
    public string NodeId
    {
        get { return _id; }
    }
    public byte[] NodeImage
    {
        get { return null; }
    }
    public bool CanDelete
    {
        get { return false; }
    }

    public void Delete()
    {
        throw new InvalidOperationException();
    }

    public bool Hide
    {
        get
        {
            // TODO:  Add SchemaItemProviderGroup.Hide getter implementation
            return false;
        }
        set
        {
            // TODO:  Add SchemaItemProviderGroup.Hide setter implementation
        }
    }
    public virtual string FontStyle
    {
        get { return "Regular"; }
    }
    #endregion
    #region IBrowserNode Members
    public bool HasChildNodes
    {
        get { return true; }
    }
    public bool CanRename
    {
        get { return false; }
    }

    public BrowserNodeCollection ChildNodes()
    {
        return _children;
    }

    public string NodeText
    {
        get { return _text; }
        set { throw new InvalidOperationException(); }
    }
    public string Icon
    {
        get { return _icon; }
    }
    #endregion
    #region IComparable Members
    public int Order
    {
        get { return _order; }
    }

    public int CompareTo(object obj)
    {
        SchemaItemProviderGroup group = obj as SchemaItemProviderGroup;
        if (group != null)
        {
            return _order.CompareTo(group.Order);
        }
        else
        {
            throw new InvalidCastException();
        }
    }
    #endregion
}
