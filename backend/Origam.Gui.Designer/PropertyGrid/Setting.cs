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

/* All code is written my me,Ben Ratzlaff and is available for any free use except where noted.
 *I assume no responsibility for how anyone uses the information available*/

using System;

namespace Origam.Gui.Designer.PropertyGrid;

/// <summary>
/// Stores information to be displayed in the CustomPropertyGrid
/// </summary>
public class Setting
{
    private object val;
    private string desc,
        category,
        name;
    private Type _type;
    private System.ComponentModel.TypeConverterAttribute _typeConverter;
    private System.ComponentModel.EditorAttribute _uiTypeEditor;
    public event EventHandler ValueChanged;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="val">The current value of the setting</param>
    /// <param name="desc">The setting's description</param>
    /// <param name="category">The setting's category</param>
    /// <param name="update">An eventhandler that will be called if CustomPropertyGrid.InstantUpdate is true</param>
    public Setting(object val, string desc, string category, EventHandler update, Type type)
    {
        this.val = val;
        this.desc = desc;
        this.category = category;
        this._type = type;
        if (update != null)
        {
            ValueChanged += update;
        }
    }

    #region Other constructors that call the one above
    public Setting(object val, string desc, string category)
        : this(val, desc, category, null, null) { }

    public Setting(object val, string desc)
        : this(val, desc, null, null, null) { }

    public Setting(object val, Type type)
        : this(val, null, null, null, type) { }
    #endregion
    #region get/set properties for the private data
    public object Value
    {
        get { return val; }
        set { val = value; }
    }
    public Type Type
    {
        get { return _type; }
        set { _type = value; }
    }
    public System.ComponentModel.TypeConverterAttribute TypeConverter
    {
        get { return _typeConverter; }
        set { _typeConverter = value; }
    }
    public System.ComponentModel.EditorAttribute UITypeEditor
    {
        get { return _uiTypeEditor; }
        set { _uiTypeEditor = value; }
    }
    public string Description
    {
        get { return desc; }
        set { desc = value; }
    }
    public string Category
    {
        get { return category; }
        set { category = value; }
    }
    public string Name
    {
        get { return name; }
        set { name = value; }
    }
    #endregion
    /// <summary>
    /// Allows an external object to force calling the event
    /// </summary>
    /// <param name="e"></param>
    public void FireUpdate(EventArgs e)
    {
        //I didnt do this in the Value's set method because sometimes I want to set the Value without firing the event
        //I could do the same thing with a second property, but this works fine.
        if (ValueChanged != null)
        {
            ValueChanged(this, e);
        }
    }
}
