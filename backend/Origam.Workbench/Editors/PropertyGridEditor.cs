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
using System.Windows.Forms;
using Origam.UI;
using Origam.Workbench.PropertyGrid;

namespace Origam.Workbench.Editors;

public class PropertyGridEditor : AbstractEditor
{
    private readonly bool closeOnLinkClick;
    private PropertyGridEx propertyGrid1;

    private void InitializeComponent()
    {
        this.propertyGrid1 = new PropertyGridEx();
        propertyGrid1.LinkClicked += (sender, args) =>
        {
            if (closeOnLinkClick)
            {
                Close();
            }
        };
        this.SuspendLayout();
        //
        // propertyGrid1
        //
        this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.propertyGrid1.HelpBackColor = System.Drawing.Color.LightYellow;
        this.propertyGrid1.Location = new System.Drawing.Point(x: 0, y: 40);
        this.propertyGrid1.Name = "propertyGrid1";
        this.propertyGrid1.Size = new System.Drawing.Size(width: 687, height: 526);
        this.propertyGrid1.TabIndex = 0;
        this.propertyGrid1.PropertyValueChanged +=
            new System.Windows.Forms.PropertyValueChangedEventHandler(
                this.propertyGrid1_PropertyValueChanged
            );
        //
        // TestEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(width: 5, height: 13);
        this.ClientSize = new System.Drawing.Size(width: 766, height: 566);
        this.Controls.Add(value: this.propertyGrid1);
        this.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 8.25F,
            style: System.Drawing.FontStyle.Regular,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.Name = "TestEditor";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Controls.SetChildIndex(child: this.propertyGrid1, newIndex: 0);
        this.ResumeLayout(performLayout: false);
        this.PerformLayout();
    }

    public PropertyGridEditor(bool closeOnLinkClick)
    {
        this.closeOnLinkClick = closeOnLinkClick;
        InitializeComponent();
        this.ContentLoaded += new EventHandler(PropertyGridEditor_ContentLoaded);
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        this.propertyGrid1.LineColor = OrigamColorScheme.PropertyGridHeaderColor;
        this.propertyGrid1.HelpBackColor = OrigamColorScheme.MdiBackColor;
        ;
        this.propertyGrid1.HelpForeColor = OrigamColorScheme.MdiForeColor;
        this.propertyGrid1.SelectedItemWithFocusBackColor = OrigamColorScheme.TabActiveStartColor;
        WorkbenchSingleton.Workbench.ViewOpened += Workbench_ViewOpened;
        this.Shown += new EventHandler(PropertyGridEditor_Shown);
    }

    private void PropertyGridEditor_Shown(object sender, EventArgs e)
    {
        propertyGrid1.SetSplitter();
    }

    /// <summary>
    /// If another editor opens while in dialog mode, we close the dialog.
    /// This happens when user double clicks on a model element link.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Workbench_ViewOpened(object sender, ViewContentEventArgs e)
    {
        if (!IsDirty && Modal)
        {
            Close();
        }
    }

    private void propertyGrid1_PropertyValueChanged(
        object s,
        System.Windows.Forms.PropertyValueChangedEventArgs e
    )
    {
        if (this.IsReadOnly)
        {
            e.ChangedItem.PropertyDescriptor.SetValue(
                component: propertyGrid1.SelectedObject,
                value: e.OldValue
            );
            MessageBox.Show(
                owner: this,
                text: ResourceUtils.GetString(key: "ErrorElementReadOnly"),
                caption: ResourceUtils.GetString(key: "ErrorTitle"),
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
        else
        {
            this.IsDirty = true;
            this.TitleName = ModelContent.Name;
        }
    }

    private void PropertyGridEditor_ContentLoaded(object sender, EventArgs e)
    {
        this.propertyGrid1.SelectedObject = ModelContent;
        propertyGrid1.Select();
    }

    public override void SaveObject()
    {
        propertyGrid1.Refresh();
        base.SaveObject();
    }
}
