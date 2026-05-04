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
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsCheckBox.
/// </summary>
[ToolboxBitmap(t: typeof(AsCheckBox))]
public class AsCheckBox : CheckBox, IAsControl, IAsCaptionControl
{
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    public event EventHandler valueChanged;

    public AsCheckBox()
    {
        this.FlatStyle = FlatStyle.Flat;
        this.BackColor = System.Drawing.Color.Transparent;
        this.CheckedChanged += new EventHandler(AsCheckBox_CheckedChanged);
        this.Click += new EventHandler(AsCheckBox_Click);
        this.KeyPress += new KeyPressEventHandler(AsCheckBox_KeyPress);
        this.EnabledChanged += new EventHandler(AsCheckBox_EnabledChanged);
        this.DataBindings.CollectionChanged += new CollectionChangeEventHandler(
            DataBindings_CollectionChanged
        );
    }

    #region IAsControl Members
    public object Value
    {
        get
        {
            if (this.CheckState == CheckState.Indeterminate)
            {
                return DBNull.Value;
            }

            return this.Checked;
        }
        set
        {
            if (value == DBNull.Value | value == null)
            {
                if (this.ThreeState)
                {
                    this.CheckState = CheckState.Indeterminate;
                }
                else
                {
                    this.Checked = false;
                }
            }
            else if (value is bool)
            {
                if (this.ThreeState)
                {
                    if ((bool)value)
                    {
                        this.CheckState = CheckState.Checked;
                    }
                    else
                    {
                        this.CheckState = CheckState.Unchecked;
                    }
                }
                else
                {
                    this.Checked = (bool)value;
                }
            }
        }
    }
    public string DefaultBindableProperty
    {
        get { return "Value"; }
    }
    #endregion
    #region Properties
    public override string Text
    {
        get => base.Text;
        set
        {
            if (value != null && !value.StartsWith(value: "AsCheckBox"))
            {
                base.Text = value;
            }
        }
    }
    int _gridColumnWidth;

    [Category(category: "(ORIGAM)")]
    [DefaultValue(value: 100)]
    [Description(description: CaptionDoc.GridColumnWidthDescription)]
    public int GridColumnWidth
    {
        get { return _gridColumnWidth; }
        set { _gridColumnWidth = value; }
    }
    private bool _readOnly = false;

    [Browsable(browsable: true)]
    [Category(category: "Behavior")]
    [DefaultValue(value: false)]
    public bool ReadOnly
    {
        get { return _readOnly; }
        set
        {
            _readOnly = value;
            if (!this.DesignMode)
            {
                this.Enabled = !value;
            }
        }
    }
    private bool _hideOnForm = false;
    public bool HideOnForm
    {
        get { return _hideOnForm; }
        set
        {
            _hideOnForm = value;
            if (value && !this.DesignMode)
            {
                this.Hide();
            }
        }
    }
    private bool _enabled = false;
    public new bool Enabled
    {
        get
        {
            if (this.ReadOnly)
            {
                return false;
            }

            return _enabled;
        }
        set
        {
            _enabled = value;
            if (this.ReadOnly)
            {
                if (this.DesignMode)
                {
                    base.Enabled = value;
                }
                else
                {
                    base.Enabled = false;
                }
            }
            else
            {
                base.Enabled = value;
            }
        }
    }
    private Guid _styleId;

    [Browsable(browsable: false)]
    public Guid StyleId
    {
        get { return _styleId; }
        set { _styleId = value; }
    }

    [TypeConverter(type: typeof(StylesConverter))]
    public UIStyle Style
    {
        get
        {
            return (UIStyle)
                _persistence.SchemaProvider.RetrieveInstance(
                    type: typeof(UIStyle),
                    primaryKey: new ModelElementKey(id: this.StyleId)
                );
        }
        set { this.StyleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    #endregion
    #region Events
    void OnValueChanged(EventArgs e)
    {
        if (valueChanged != null)
        {
            valueChanged(sender: this, e: e);
        }
        this.OnValidating(e: new CancelEventArgs(cancel: false));
    }
    #endregion
    #region Event Handlers
    private void AsCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        //OnValueChanged(EventArgs.Empty);
    }

    private void AsCheckBox_Click(object sender, EventArgs e)
    {
        OnValueChanged(e: EventArgs.Empty);
        System.Diagnostics.Debug.WriteLine(message: "Checked changed");
    }

    private void AsCheckBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        //OnValueChanged(EventArgs.Empty);
    }

    private void AsCheckBox_EnabledChanged(object sender, EventArgs e)
    {
        if (this.ReadOnly & base.Enabled)
        {
            this.Enabled = false;
        }
    }

    private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
    {
        if (this.DesignMode)
        {
            if (this.Text == "")
            {
                if (e.Element != null)
                {
                    if ((e.Element as Binding).PropertyName == this.DefaultBindableProperty)
                    {
                        if (e.Action == CollectionChangeAction.Remove)
                        {
                            this.Text = "";
                        }
                        else
                        {
                            try
                            {
                                this.Text = ColumnCaption(binding: (e.Element as Binding));
                            }
                            catch
                            {
                                this.Text = "????";
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region IAsCaptionControl Members
    public string Caption
    {
        get { return base.Text; }
        set { base.Text = value ?? ""; }
    }
    private string _gridColumnCaption;
    public string GridColumnCaption
    {
        get { return _gridColumnCaption; }
        set { _gridColumnCaption = value; }
    }
    public int CaptionLength
    {
        get { return this.Width; }
        set
        {
            //this.Width = value;
        }
    }
    public CaptionPosition CaptionPosition
    {
        get { return CaptionPosition.Right; }
        set
        {
            // TODO:  Add AsCheckBox.CaptionPosition setter implementation
        }
    }
    #endregion
    #region Private Methods
    private string ColumnCaption(Binding binding)
    {
        if (binding.DataSource is DataSet)
        {
            DataSet dataset = binding.DataSource as DataSet;
            // Get Table
            DataTable table = dataset.Tables[
                name: TableName(ds: dataset, dataMember: binding.BindingMemberInfo.BindingMember)
            ];

            if (table != null)
            {
                if (table.Columns.Contains(name: binding.BindingMemberInfo.BindingField))
                {
                    return table.Columns[name: binding.BindingMemberInfo.BindingField].Caption;
                }
            }
        }
        return binding.BindingMemberInfo.BindingField;
    }

    private string TableName(DataSet ds, string dataMember)
    {
        // In case that dataMember is a path through relations, we find the last table
        // so we can take a caption out of it
        string tableName = "";
        if (dataMember.IndexOf(value: ".") > 0)
        {
            string[] path = dataMember.Split(separator: ".".ToCharArray());
            DataTable table = ds.Tables[name: path[0]];
            for (int i = 1; i < path.Length - 1; i++)
            {
                table = table.ChildRelations[name: path[i]].ChildTable;
            }
            if (table != null)
            {
                tableName = table.TableName;
            }
        }
        else
        {
            tableName = dataMember;
        }

        return tableName;
    }

    private void ResetCaption()
    {
        if (this.Caption == "")
        {
            foreach (Binding binding in this.DataBindings)
            {
                if (binding.PropertyName == "Text")
                {
                    try
                    {
                        base.Text = ColumnCaption(binding: binding);
                    }
                    catch
                    {
                        base.Text = "????";
                    }
                }
            }
        }
    }
    #endregion
}
