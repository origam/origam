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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Origam.Schema;
using Origam.UI;

namespace Origam.Workbench.Pads;

/// <summary>
/// Summary description for PropertyPad.
/// </summary>
public class PropertyPad : AbstractPadContent, IPropertyPad
{
    private System.Windows.Forms.ComboBox cboComponents;
    private PropertyGrid.PropertyGridEx pgrid;
    private bool _selectComponent = true;

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(PropertyPad));
        this.pgrid = new Origam.Workbench.PropertyGrid.PropertyGridEx();
        this.cboComponents = new System.Windows.Forms.ComboBox();
        this.SuspendLayout();
        //
        // pgrid
        //
        this.pgrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pgrid.HelpBackColor = System.Drawing.Color.LightYellow;
        this.pgrid.LineColor = System.Drawing.SystemColors.ScrollBar;
        this.pgrid.Location = new System.Drawing.Point(0, 21);
        this.pgrid.Name = "pgrid";
        this.pgrid.Size = new System.Drawing.Size(292, 252);
        this.pgrid.TabIndex = 0;
        this.pgrid.PropertyValueChanged +=
            new System.Windows.Forms.PropertyValueChangedEventHandler(
                this.pgrid_PropertyValueChanged
            );
        //
        // cboComponents
        //
        this.cboComponents.Dock = System.Windows.Forms.DockStyle.Top;
        this.cboComponents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboComponents.Location = new System.Drawing.Point(0, 0);
        this.cboComponents.Name = "cboComponents";
        this.cboComponents.Size = new System.Drawing.Size(292, 21);
        this.cboComponents.Sorted = true;
        this.cboComponents.TabIndex = 1;
        this.cboComponents.SelectedIndexChanged += new System.EventHandler(
            this.cboComponents_SelectedIndexChanged
        );
        //
        // PropertyPad
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(292, 273);
        this.Controls.Add(this.pgrid);
        this.Controls.Add(this.cboComponents);
        this.DockAreas = (
            (WeifenLuo.WinFormsUI.Docking.DockAreas)(
                (
                    (
                        (
                            (
                                WeifenLuo.WinFormsUI.Docking.DockAreas.Float
                                | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft
                            ) | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight
                        ) | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop
                    ) | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom
                )
            )
        );
        this.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            8.25F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(238))
        );
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "PropertyPad";
        this.TabText = "Properties";
        this.Text = "Properties";
        this.ResumeLayout(false);
    }

    public PropertyPad()
    {
        InitializeComponent();
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        this.pgrid.LineColor = OrigamColorScheme.PropertyGridHeaderColor;
        this.pgrid.HelpBackColor = OrigamColorScheme.MdiBackColor;
        ;
        this.pgrid.HelpForeColor = OrigamColorScheme.MdiForeColor;
        this.pgrid.SelectedItemWithFocusBackColor = OrigamColorScheme.TabActiveStartColor;
        this.pgrid.SelectedObjectsChanged += new EventHandler(pgrid_SelectedObjectsChanged);
    }

    #region IPropertyPad Members
    public System.Windows.Forms.PropertyGrid PropertyGrid
    {
        get { return this.pgrid; }
    }
    #endregion
    private void pgrid_SelectedObjectsChanged(object sender, EventArgs e)
    {
        _selectComponent = false;
        RefreshControlList();
        _selectComponent = true;
    }

    private void cboComponents_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        if (!_selectComponent)
        {
            return;
        }

        PropertyPadListItem item = cboComponents.SelectedItem as PropertyPadListItem;
        if (item != null)
        {
            ISelectionService svc =
                item.Control.Site.GetService(typeof(ISelectionService)) as ISelectionService;
            var list = new List<Control>();
            list.Add(item.Control);
            svc.SetSelectedComponents(list);
        }
    }

    public Func<bool> ReadOnlyGetter { get; set; } = null;

    private void pgrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        if (ReadOnlyGetter != null && ReadOnlyGetter() && !Equals(e.OldValue, e.ChangedItem.Value))
        {
            e.ChangedItem.PropertyDescriptor.SetValue(pgrid.SelectedObject, e.OldValue);
            MessageBox.Show(
                this,
                ResourceUtils.GetString("ErrorElementReadOnly"),
                ResourceUtils.GetString("ErrorTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }
        RefreshControlList();
    }

    private void RefreshControlList()
    {
        cboComponents.Items.Clear();
        Component selectedItem = null;

        if (pgrid.SelectedObjects.Length > 0)
        {
            selectedItem = pgrid.SelectedObjects[0] as Control;
            PropertyPadListItem selectedCboItem = null;
            if (selectedItem != null)
            {
                foreach (object component in selectedItem.Site.Container.Components)
                {
                    Control control = component as Control;
                    if (control != null && control.Tag is ISchemaItem)
                    {
                        PropertyPadListItem item = new PropertyPadListItem(control);
                        cboComponents.Items.Add(item);
                        if (pgrid.SelectedObjects.Length == 1 && selectedItem == control)
                        {
                            selectedCboItem = item;
                        }
                    }
                }
                cboComponents.SelectedItem = selectedCboItem;
            }
        }
    }
}
