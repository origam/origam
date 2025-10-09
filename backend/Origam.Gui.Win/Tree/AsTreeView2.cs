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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsTextBox.
/// </summary>
[ToolboxBitmap(typeof(AsTextBox))]
public class AsTreeView2 : TextBox
{
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;

    #region Handling base events
    protected override void InitLayout()
    {
        if (this.Disposing)
            return;
        this.BorderStyle = BorderStyle.Fixed3D;
        this.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.AcceptsTab = true;
        this.Multiline = true;
        base.InitLayout();
    }
    #endregion
    private Guid _styleId;

    [Browsable(false)]
    public Guid StyleId
    {
        get { return _styleId; }
        set { _styleId = value; }
    }
    private Guid _treeId;

    [Browsable(false)]
    public Guid TreeId
    {
        get { return _treeId; }
        set { _treeId = value; }
    }

    [TypeConverter(typeof(TreeStructureConverter))]
    public TreeStructure Tree
    {
        get
        {
            return (TreeStructure)
                _persistence.SchemaProvider.RetrieveInstance(
                    typeof(TreeStructure),
                    new ModelElementKey(this.TreeId)
                );
        }
        set { this.TreeId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]); }
    }

    [TypeConverter(typeof(StylesConverter))]
    public UIStyle Style
    {
        get
        {
            return (UIStyle)
                _persistence.SchemaProvider.RetrieveInstance(
                    typeof(UIStyle),
                    new ModelElementKey(this.StyleId)
                );
        }
        set { this.StyleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]); }
    }
    private string _formParameterName;
    public string FormParameterName
    {
        get { return _formParameterName; }
        set { _formParameterName = value; }
    }
}
