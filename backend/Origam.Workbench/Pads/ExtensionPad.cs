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
using System.Collections.Generic;
using System.Windows.Forms;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workbench.Pads;

/// <summary>
/// Summary description for ExtensionPad.
/// </summary>
public class ExtensionPad : AbstractPadContent
{
    private WorkbenchSchemaService _schema =
        ServiceManager.Services.GetService(typeof(WorkbenchSchemaService))
        as WorkbenchSchemaService;

    //		public Origam.Workbench.ExpressionBrowser ebrSchemaBrowser;
    private System.Windows.Forms.ToolBar toolBar1;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.ToolBarButton tbrNew;
    private System.Windows.Forms.ListView lvwPackages;
    private System.Windows.Forms.ColumnHeader colPackage;
    private System.Windows.Forms.ColumnHeader colVersion;
    private System.Windows.Forms.ToolBarButton tbrRemove;
    private System.ComponentModel.IContainer components;

    public ExtensionPad()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(ExtensionPad));
        this.toolBar1 = new System.Windows.Forms.ToolBar();
        this.tbrNew = new System.Windows.Forms.ToolBarButton();
        this.tbrRemove = new System.Windows.Forms.ToolBarButton();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.lvwPackages = new System.Windows.Forms.ListView();
        this.colPackage = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.colVersion = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.SuspendLayout();
        //
        // toolBar1
        //
        this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolBar1.Buttons.AddRange(
            new System.Windows.Forms.ToolBarButton[] { this.tbrNew, this.tbrRemove }
        );
        this.toolBar1.DropDownArrows = true;
        this.toolBar1.ImageList = this.imageList1;
        this.toolBar1.Location = new System.Drawing.Point(0, 0);
        this.toolBar1.Name = "toolBar1";
        this.toolBar1.ShowToolTips = true;
        this.toolBar1.Size = new System.Drawing.Size(272, 28);
        this.toolBar1.TabIndex = 1;
        this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
        this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(
            this.toolBar1_ButtonClick
        );
        //
        // tbrNew
        //
        this.tbrNew.Enabled = false;
        this.tbrNew.ImageIndex = 0;
        this.tbrNew.Name = "tbrNew";
        this.tbrNew.Text = "Add";
        //
        // tbrRemove
        //
        this.tbrRemove.Enabled = false;
        this.tbrRemove.ImageIndex = 1;
        this.tbrRemove.Name = "tbrRemove";
        this.tbrRemove.Text = "Remove";
        //
        // imageList1
        //
        this.imageList1.ImageStream = (
            (System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream"))
        );
        this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
        this.imageList1.Images.SetKeyName(0, "add_black.png");
        this.imageList1.Images.SetKeyName(1, "delete_black.png");
        this.imageList1.Images.SetKeyName(2, "");
        this.imageList1.Images.SetKeyName(3, "09_packages-1.ico");
        //
        // lvwPackages
        //
        this.lvwPackages.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.lvwPackages.Columns.AddRange(
            new System.Windows.Forms.ColumnHeader[] { this.colPackage, this.colVersion }
        );
        this.lvwPackages.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvwPackages.FullRowSelect = true;
        this.lvwPackages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
        this.lvwPackages.Location = new System.Drawing.Point(0, 28);
        this.lvwPackages.Name = "lvwPackages";
        this.lvwPackages.Size = new System.Drawing.Size(272, 245);
        this.lvwPackages.SmallImageList = this.imageList1;
        this.lvwPackages.Sorting = System.Windows.Forms.SortOrder.Ascending;
        this.lvwPackages.TabIndex = 0;
        this.lvwPackages.UseCompatibleStateImageBehavior = false;
        this.lvwPackages.View = System.Windows.Forms.View.Details;
        this.lvwPackages.DoubleClick += new System.EventHandler(this.lvwPackages_DoubleClick);
        this.lvwPackages.KeyDown += new System.Windows.Forms.KeyEventHandler(
            this.lvwPackages_KeyDown
        );
        //
        // colPackage
        //
        this.colPackage.Text = "Package";
        this.colPackage.Width = 173;
        //
        // colVersion
        //
        this.colVersion.Text = "Version";
        this.colVersion.Width = 89;
        //
        // ExtensionPad
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(272, 273);
        this.Controls.Add(this.lvwPackages);
        this.Controls.Add(this.toolBar1);
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
        this.Name = "ExtensionPad";
        this.TabText = "Packages";
        this.Text = "Package Browser";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion
    public void LoadPackages()
    {
        try
        {
            lvwPackages.SuspendLayout();
            Key selectedId = null;
            if (lvwPackages.SelectedItems.Count > 0)
            {
                selectedId = (lvwPackages.SelectedItems[0].Tag as Package).PrimaryKey;
            }
            lvwPackages.Items.Clear();
            IPersistenceService persistenceService =
                ServiceManager.Services.GetService(typeof(IPersistenceService))
                as IPersistenceService;
            List<Package> packageList = persistenceService.SchemaListProvider.RetrieveList<Package>(
                null
            );
            foreach (Package extension in packageList)
            {
                ListViewItem item = lvwPackages.Items.Add(extension.Name, 3);
                item.SubItems.Add(extension.Version);
                item.Tag = extension;
                if (selectedId != null && selectedId.Equals(extension.PrimaryKey))
                {
                    item.Selected = true;
                }
            }
            if (lvwPackages.SelectedItems.Count > 0)
            {
                lvwPackages.SelectedItems[0].EnsureVisible();
            }
        }
        finally
        {
            lvwPackages.ResumeLayout();
        }
        tbrNew.Enabled = true;
        tbrRemove.Enabled = true;
    }

    public void UpdateExtensionInfo(Package updatedPackage)
    {
        foreach (ListViewItem item in lvwPackages.Items)
        {
            if (item.Tag is Package package && package.Id == updatedPackage.Id)
            {
                item.Tag = updatedPackage;
                item.SubItems[1].Text = updatedPackage.VersionString;
                return;
            }
        }
    }

    public void UnloadPackages()
    {
        lvwPackages.Items.Clear();
        tbrNew.Enabled = false;
        tbrRemove.Enabled = false;
    }

    public Package SelectedExtension
    {
        get
        {
            if (lvwPackages.SelectedItems.Count == 1)
            {
                return lvwPackages.SelectedItems[0].Tag as Package;
            }

            return null;
        }
        set
        {
            lvwPackages.SelectedItems.Clear();
            if (value != null)
            {
                foreach (ListViewItem item in lvwPackages.Items)
                {
                    if ((item.Tag as Package).PrimaryKey.Equals(value.PrimaryKey))
                    {
                        item.Selected = true;
                    }
                }
            }
        }
    }

    private void toolBar1_ButtonClick(
        object sender,
        System.Windows.Forms.ToolBarButtonClickEventArgs e
    )
    {
        if (e.Button == tbrNew)
        {
            AddPackage addPackageDialog = new AddPackage();
            if (addPackageDialog.ShowDialog() == DialogResult.OK)
            {
                string packageName = addPackageDialog.PackageName;
                Guid packageId = Guid.NewGuid();
                PackageHelper.CreatePackage(
                    packageName,
                    packageId,
                    new Guid("147FA70D-6519-4393-B5D0-87931F9FD609")
                );
                LoadPackages();
                _schema.SchemaBrowser.EbrSchemaBrowser.RefreshAllNodes();
                Commands.ViewSchemaBrowserPad cmdViewBrowser = new Commands.ViewSchemaBrowserPad();
                cmdViewBrowser.Run();
            }
        }
        if (e.Button == tbrRemove)
        {
            if (SelectedExtension == null)
            {
                AsMessageBox.ShowError(this, "Select a package to delete.", "Remove Package", null);
            }
            else
            {
                if (
                    MessageBox.Show(
                        this,
                        "Do you really want to remove the package '"
                            + SelectedExtension.Name
                            + "'?",
                        "Remove Package",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2
                    ) == DialogResult.Yes
                )
                {
                    IPersistenceService persistenceService =
                        ServiceManager.Services.GetService(typeof(IPersistenceService))
                        as IPersistenceService;
                    persistenceService.SchemaProvider.DeletePackage(
                        (Guid)SelectedExtension.PrimaryKey["Id"]
                    );
                    LoadPackages();
                }
            }
        }
    }

    private void OpenSelectedPackage()
    {
        try
        {
            Commands.LoadSelectedPackage cmd = new Origam.Workbench.Commands.LoadSelectedPackage();
            if (cmd.IsEnabled)
            {
                cmd.Run();
            }
        }
        catch (Exception ex)
        {
            Origam.UI.AsMessageBox.ShowError(
                this.FindForm(),
                ex.Message,
                ResourceUtils.GetString("ErrorWhenLoadPackage", this.SelectedExtension.Name),
                ex
            );
        }
    }

    private void lvwPackages_DoubleClick(object sender, System.EventArgs e)
    {
        OpenSelectedPackage();
    }

    private void lvwPackages_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            OpenSelectedPackage();
        }
    }
}
