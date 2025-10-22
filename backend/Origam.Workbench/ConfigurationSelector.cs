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

namespace Origam;

/// <summary>
/// Summary description for ConfigurationSelector.
/// </summary>
public class ConfigurationSelector : System.Windows.Forms.Form
{
    private System.Windows.Forms.ListView lvwConfigurations;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Button btnCancel;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public ConfigurationSelector()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        this.btnOK.BackColor = OrigamColorScheme.ButtonBackColor;
        this.btnOK.ForeColor = OrigamColorScheme.ButtonForeColor;
        this.btnCancel.BackColor = OrigamColorScheme.ButtonBackColor;
        this.btnCancel.ForeColor = OrigamColorScheme.ButtonForeColor;
        this.lblDescription.BackColor = OrigamColorScheme.TitleActiveEndColor;
        this.lblDescription.ForeColor = OrigamColorScheme.TitleActiveForeColor;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.Resources.ResourceManager resources = new System.Resources.ResourceManager(
            typeof(ConfigurationSelector)
        );
        this.lvwConfigurations = new System.Windows.Forms.ListView();
        this.colName = new System.Windows.Forms.ColumnHeader();
        this.lblDescription = new System.Windows.Forms.Label();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnOK = new System.Windows.Forms.Button();
        this.SuspendLayout();
        //
        // lvwConfigurations
        //
        this.lvwConfigurations.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (
                    (
                        (
                            System.Windows.Forms.AnchorStyles.Top
                            | System.Windows.Forms.AnchorStyles.Bottom
                        ) | System.Windows.Forms.AnchorStyles.Left
                    ) | System.Windows.Forms.AnchorStyles.Right
                )
            )
        );
        this.lvwConfigurations.Columns.AddRange(
            new System.Windows.Forms.ColumnHeader[] { this.colName }
        );
        this.lvwConfigurations.FullRowSelect = true;
        this.lvwConfigurations.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.lvwConfigurations.HideSelection = false;
        this.lvwConfigurations.Location = new System.Drawing.Point(0, 24);
        this.lvwConfigurations.MultiSelect = false;
        this.lvwConfigurations.Name = "lvwConfigurations";
        this.lvwConfigurations.Size = new System.Drawing.Size(320, 288);
        this.lvwConfigurations.TabIndex = 0;
        this.lvwConfigurations.View = System.Windows.Forms.View.Details;
        this.lvwConfigurations.DoubleClick += new System.EventHandler(
            this.lvwConfigurations_DoubleClick
        );
        //
        // colName
        //
        this.colName.Text = "Name";
        this.colName.Width = 314;
        //
        // lblDescription
        //
        this.lblDescription.BackColor = System.Drawing.SystemColors.Info;
        this.lblDescription.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblDescription.Location = new System.Drawing.Point(0, 0);
        this.lblDescription.Name = "lblDescription";
        this.lblDescription.Size = new System.Drawing.Size(320, 24);
        this.lblDescription.TabIndex = 1;
        this.lblDescription.Text = "Please choose the configuration you want to start:";
        this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // btnCancel
        //
        this.btnCancel.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)
            )
        );
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.btnCancel.Location = new System.Drawing.Point(104, 320);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(96, 24);
        this.btnCancel.TabIndex = 1;
        this.btnCancel.Text = "Cancel";
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        //
        // btnOK
        //
        this.btnOK.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)
            )
        );
        this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
        this.btnOK.Location = new System.Drawing.Point(208, 320);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(96, 24);
        this.btnOK.TabIndex = 0;
        this.btnOK.Text = "OK";
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        //
        // ConfigurationSelector
        //
        this.AcceptButton = this.btnOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.BackColor = System.Drawing.Color.FloralWhite;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(320, 350);
        this.ControlBox = false;
        this.Controls.Add(this.lvwConfigurations);
        this.Controls.Add(this.lblDescription);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "ConfigurationSelector";
        this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Select Configuration";
        this.ResumeLayout(false);
    }
    #endregion
    private OrigamSettingsCollection _configurations;

    private void btnOK_Click(object sender, System.EventArgs e)
    {
        if (lvwConfigurations.SelectedItems.Count != 1)
        {
            MessageBox.Show(
                this,
                "Please select a configuration.",
                "Configuration Selection",
                MessageBoxButtons.OK,
                MessageBoxIcon.Stop
            );
        }
        else
        {
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
        foreach (ListViewItem item in lvwConfigurations.Items)
        {
            item.Selected = false;
        }
        this.Hide();
    }

    private void lvwConfigurations_DoubleClick(object sender, System.EventArgs e)
    {
        btnOK_Click(this, EventArgs.Empty);
    }

    public OrigamSettingsCollection Configurations
    {
        get { return _configurations; }
        set
        {
            _configurations = value;
            lvwConfigurations.Items.Clear();
            if (value != null)
            {
                foreach (OrigamSettings setting in _configurations)
                {
                    ListViewItem item = new ListViewItem(setting.Name);
                    item.Tag = setting;
                    lvwConfigurations.Items.Add(item);
                }
            }
        }
    }
    public OrigamSettings SelectedConfiguration
    {
        get
        {
            if (lvwConfigurations.SelectedItems.Count == 1)
            {
                return lvwConfigurations.SelectedItems[0].Tag as OrigamSettings;
            }

            return null;
        }
    }
}
