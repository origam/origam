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
using System.Drawing;
using System.Windows.Forms;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Pads;
using Origam.Workbench.Services;

namespace Origam.Workbench.Editors;

/// <summary>
/// Summary description for PackageEditor.
/// </summary>
public class PackageEditor : AbstractViewContent
{
    private Package _package;
    private SchemaService _schema =
        ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
    private IPersistenceService _persistence =
        ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
    private System.Windows.Forms.ToolStrip toolBar1;
    private System.Windows.Forms.ImageList imageList1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.TextBox txtVersion;
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtCopyright;
    private System.Windows.Forms.Label lblCopyright;
    private System.Windows.Forms.Label lblVersion;
    private System.Windows.Forms.TabPage tabInfo;
    private System.Windows.Forms.TabPage tabReferences;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.TextBox txtDescription;
    private System.Windows.Forms.Label lblVersionInfo;
    private System.Windows.Forms.ToolStripButton btnAddReference;
    private System.Windows.Forms.ToolStripButton btnDelete;
    private System.Windows.Forms.Splitter splitter1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Label lblReferences;
    private System.Windows.Forms.Label label2;
    private Origam.Workbench.ExpressionBrowser ebrElements;
    private System.Windows.Forms.ListView lvwReferences;
    private System.Windows.Forms.ColumnHeader colReferencedPackage;
    private ContextMenuStrip mnuAddReference;
    private System.Windows.Forms.Label lblId;
    private System.Windows.Forms.TextBox txtId;
    private System.ComponentModel.IContainer components;

    public PackageEditor()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        this.Icon = Icon.FromHandle(new Bitmap(Images.ExtensionBrowser).GetHicon());
        this.tabInfo.BackColor = OrigamColorScheme.FormBackgroundColor;
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
            _schema = null;
            _persistence = null;
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
            new System.ComponentModel.ComponentResourceManager(typeof(PackageEditor));
        this.txtName = new System.Windows.Forms.TextBox();
        this.txtVersion = new System.Windows.Forms.TextBox();
        this.lblName = new System.Windows.Forms.Label();
        this.txtCopyright = new System.Windows.Forms.TextBox();
        this.toolBar1 = new System.Windows.Forms.ToolStrip();
        this.btnAddReference = new System.Windows.Forms.ToolStripButton();
        this.btnDelete = new System.Windows.Forms.ToolStripButton();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.mnuAddReference = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.lblCopyright = new System.Windows.Forms.Label();
        this.lblVersion = new System.Windows.Forms.Label();
        this.tabControl1 = new System.Windows.Forms.TabControl();
        this.tabInfo = new System.Windows.Forms.TabPage();
        this.txtDescription = new System.Windows.Forms.TextBox();
        this.lblDescription = new System.Windows.Forms.Label();
        this.tabReferences = new System.Windows.Forms.TabPage();
        this.panel1 = new System.Windows.Forms.Panel();
        this.ebrElements = new Origam.Workbench.ExpressionBrowser();
        this.label2 = new System.Windows.Forms.Label();
        this.splitter1 = new System.Windows.Forms.Splitter();
        this.panel2 = new System.Windows.Forms.Panel();
        this.lvwReferences = new System.Windows.Forms.ListView();
        this.colReferencedPackage = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.lblReferences = new System.Windows.Forms.Label();
        this.lblVersionInfo = new System.Windows.Forms.Label();
        this.lblId = new System.Windows.Forms.Label();
        this.txtId = new System.Windows.Forms.TextBox();
        this.toolBar1.SuspendLayout();
        this.tabControl1.SuspendLayout();
        this.tabInfo.SuspendLayout();
        this.tabReferences.SuspendLayout();
        this.panel1.SuspendLayout();
        this.panel2.SuspendLayout();
        this.SuspendLayout();
        //
        // txtName
        //
        this.txtName.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (
                    (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right
                )
            )
        );
        this.txtName.Location = new System.Drawing.Point(72, 8);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(720, 20);
        this.txtName.TabIndex = 0;
        this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
        //
        // txtVersion
        //
        this.txtVersion.Location = new System.Drawing.Point(72, 32);
        this.txtVersion.Name = "txtVersion";
        this.txtVersion.ReadOnly = true;
        this.txtVersion.Size = new System.Drawing.Size(168, 20);
        this.txtVersion.TabIndex = 1;
        //
        // lblName
        //
        this.lblName.Location = new System.Drawing.Point(8, 8);
        this.lblName.Name = "lblName";
        this.lblName.Size = new System.Drawing.Size(56, 16);
        this.lblName.TabIndex = 4;
        this.lblName.Text = "Name:";
        //
        // txtCopyright
        //
        this.txtCopyright.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (
                    (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right
                )
            )
        );
        this.txtCopyright.Location = new System.Drawing.Point(72, 8);
        this.txtCopyright.Name = "txtCopyright";
        this.txtCopyright.Size = new System.Drawing.Size(696, 20);
        this.txtCopyright.TabIndex = 5;
        this.txtCopyright.TextChanged += new System.EventHandler(this.txtCopyright_TextChanged);
        //
        // toolBar1
        //
        this.toolBar1.Items.AddRange(
            new System.Windows.Forms.ToolStripButton[] { this.btnAddReference, this.btnDelete }
        );
        this.toolBar1.ImageList = this.imageList1;
        this.toolBar1.Location = new System.Drawing.Point(0, 0);
        this.toolBar1.Name = "toolBar1";
        this.toolBar1.Size = new System.Drawing.Size(776, 26);
        this.toolBar1.TabIndex = 6;
        this.toolBar1.ItemClicked += toolBar1_ItemClicked;
        //
        // btnAddReference
        //
        this.btnAddReference.ImageIndex = 0;
        this.btnAddReference.Name = "btnAddReference";
        this.btnAddReference.Size = new System.Drawing.Size(104, 22);
        this.btnAddReference.Text = "Add Reference";
        this.btnAddReference.ToolTipText = "Add Reference";
        //
        // btnDelete
        //
        this.btnDelete.ImageIndex = 1;
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(125, 22);
        this.btnDelete.Text = "Remove Reference";
        this.btnDelete.ToolTipText = "Remove Reference";
        //
        // imageList1
        //
        this.imageList1.ImageStream = (
            (System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream"))
        );
        this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
        this.imageList1.Images.SetKeyName(0, "add_black.png");
        this.imageList1.Images.SetKeyName(1, "delete_black.png");
        this.imageList1.Images.SetKeyName(2, "09_packages-1.ico");
        //
        // mnuAddReference
        //
        this.mnuAddReference.Name = "mnuAddReference";
        this.mnuAddReference.Size = new System.Drawing.Size(61, 4);
        //
        // lblCopyright
        //
        this.lblCopyright.Location = new System.Drawing.Point(8, 8);
        this.lblCopyright.Name = "lblCopyright";
        this.lblCopyright.Size = new System.Drawing.Size(56, 16);
        this.lblCopyright.TabIndex = 7;
        this.lblCopyright.Text = "Copyright:";
        //
        // lblVersion
        //
        this.lblVersion.Location = new System.Drawing.Point(8, 32);
        this.lblVersion.Name = "lblVersion";
        this.lblVersion.Size = new System.Drawing.Size(56, 16);
        this.lblVersion.TabIndex = 8;
        this.lblVersion.Text = "Version:";
        //
        // tabControl1
        //
        this.tabControl1.Anchor = (
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
        this.tabControl1.Controls.Add(this.tabInfo);
        this.tabControl1.Controls.Add(this.tabReferences);
        this.tabControl1.Location = new System.Drawing.Point(8, 96);
        this.tabControl1.Name = "tabControl1";
        this.tabControl1.SelectedIndex = 0;
        this.tabControl1.Size = new System.Drawing.Size(784, 432);
        this.tabControl1.TabIndex = 3;
        //
        // tabInfo
        //
        this.tabInfo.Controls.Add(this.txtDescription);
        this.tabInfo.Controls.Add(this.lblDescription);
        this.tabInfo.Controls.Add(this.txtCopyright);
        this.tabInfo.Controls.Add(this.lblCopyright);
        this.tabInfo.Location = new System.Drawing.Point(4, 22);
        this.tabInfo.Name = "tabInfo";
        this.tabInfo.Size = new System.Drawing.Size(776, 406);
        this.tabInfo.TabIndex = 0;
        this.tabInfo.Text = "Information";
        //
        // txtDescription
        //
        this.txtDescription.Anchor = (
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
        this.txtDescription.Location = new System.Drawing.Point(8, 56);
        this.txtDescription.Multiline = true;
        this.txtDescription.Name = "txtDescription";
        this.txtDescription.Size = new System.Drawing.Size(760, 344);
        this.txtDescription.TabIndex = 9;
        this.txtDescription.TextChanged += new System.EventHandler(this.txtDescription_TextChanged);
        //
        // lblDescription
        //
        this.lblDescription.Location = new System.Drawing.Point(8, 40);
        this.lblDescription.Name = "lblDescription";
        this.lblDescription.Size = new System.Drawing.Size(152, 16);
        this.lblDescription.TabIndex = 8;
        this.lblDescription.Text = "Description:";
        //
        // tabReferences
        //
        this.tabReferences.Controls.Add(this.panel1);
        this.tabReferences.Controls.Add(this.splitter1);
        this.tabReferences.Controls.Add(this.panel2);
        this.tabReferences.Controls.Add(this.toolBar1);
        this.tabReferences.Location = new System.Drawing.Point(4, 22);
        this.tabReferences.Name = "tabReferences";
        this.tabReferences.Size = new System.Drawing.Size(776, 406);
        this.tabReferences.TabIndex = 1;
        this.tabReferences.Text = "Package References";
        //
        // panel1
        //
        this.panel1.Controls.Add(this.ebrElements);
        this.panel1.Controls.Add(this.label2);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(232, 25);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(544, 381);
        this.panel1.TabIndex = 14;
        //
        // ebrElements
        //
        this.ebrElements.AllowEdit = false;
        this.ebrElements.CheckSecurity = false;
        this.ebrElements.DisableOtherExtensionNodes = true;
        this.ebrElements.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ebrElements.Location = new System.Drawing.Point(0, 16);
        this.ebrElements.Name = "ebrElements";
        this.ebrElements.NodeUnderMouse = null;
        this.ebrElements.ShowFilter = false;
        this.ebrElements.Size = new System.Drawing.Size(544, 365);
        this.ebrElements.TabIndex = 13;
        this.ebrElements.QueryFilterNode += new Origam.Workbench.FilterEventHandler(
            this.ebrElements_QueryFilterNode
        );
        //
        // label2
        //
        this.label2.Dock = System.Windows.Forms.DockStyle.Top;
        this.label2.Location = new System.Drawing.Point(0, 0);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(544, 16);
        this.label2.TabIndex = 16;
        this.label2.Text = "Visible Referenced Elements";
        //
        // splitter1
        //
        this.splitter1.Location = new System.Drawing.Point(224, 25);
        this.splitter1.Name = "splitter1";
        this.splitter1.Size = new System.Drawing.Size(8, 381);
        this.splitter1.TabIndex = 13;
        this.splitter1.TabStop = false;
        //
        // panel2
        //
        this.panel2.Controls.Add(this.lvwReferences);
        this.panel2.Controls.Add(this.lblReferences);
        this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
        this.panel2.Location = new System.Drawing.Point(0, 25);
        this.panel2.Name = "panel2";
        this.panel2.Size = new System.Drawing.Size(224, 381);
        this.panel2.TabIndex = 15;
        //
        // lvwReferences
        //
        this.lvwReferences.Columns.AddRange(
            new System.Windows.Forms.ColumnHeader[] { this.colReferencedPackage }
        );
        this.lvwReferences.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvwReferences.FullRowSelect = true;
        this.lvwReferences.Location = new System.Drawing.Point(0, 16);
        this.lvwReferences.Name = "lvwReferences";
        this.lvwReferences.Size = new System.Drawing.Size(224, 365);
        this.lvwReferences.SmallImageList = this.imageList1;
        this.lvwReferences.TabIndex = 14;
        this.lvwReferences.UseCompatibleStateImageBehavior = false;
        this.lvwReferences.View = System.Windows.Forms.View.Details;
        this.lvwReferences.SelectedIndexChanged += new System.EventHandler(
            this.lvwReferences_SelectedIndexChanged
        );
        //
        // colReferencedPackage
        //
        this.colReferencedPackage.Text = "Referenced Package";
        this.colReferencedPackage.Width = 218;
        //
        // lblReferences
        //
        this.lblReferences.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblReferences.Location = new System.Drawing.Point(0, 0);
        this.lblReferences.Name = "lblReferences";
        this.lblReferences.Size = new System.Drawing.Size(224, 16);
        this.lblReferences.TabIndex = 15;
        this.lblReferences.Text = "References";
        //
        // lblVersionInfo
        //
        this.lblVersionInfo.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (
                    (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right
                )
            )
        );
        this.lblVersionInfo.BackColor = System.Drawing.SystemColors.Info;
        this.lblVersionInfo.Location = new System.Drawing.Point(248, 32);
        this.lblVersionInfo.Name = "lblVersionInfo";
        this.lblVersionInfo.Size = new System.Drawing.Size(544, 20);
        this.lblVersionInfo.TabIndex = 10;
        this.lblVersionInfo.Text = "Set version using the Deployment model elements.";
        this.lblVersionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblId
        //
        this.lblId.Location = new System.Drawing.Point(8, 56);
        this.lblId.Name = "lblId";
        this.lblId.Size = new System.Drawing.Size(56, 16);
        this.lblId.TabIndex = 12;
        this.lblId.Text = "Id:";
        //
        // txtId
        //
        this.txtId.Anchor = (
            (System.Windows.Forms.AnchorStyles)(
                (
                    (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right
                )
            )
        );
        this.txtId.Location = new System.Drawing.Point(72, 56);
        this.txtId.Name = "txtId";
        this.txtId.ReadOnly = true;
        this.txtId.Size = new System.Drawing.Size(720, 20);
        this.txtId.TabIndex = 2;
        //
        // PackageEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(800, 534);
        this.Controls.Add(this.lblId);
        this.Controls.Add(this.txtId);
        this.Controls.Add(this.txtName);
        this.Controls.Add(this.txtVersion);
        this.Controls.Add(this.lblVersionInfo);
        this.Controls.Add(this.tabControl1);
        this.Controls.Add(this.lblVersion);
        this.Controls.Add(this.lblName);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "PackageEditor";
        this.Closing += new System.ComponentModel.CancelEventHandler(this.PackageEditor_Closing);
        this.toolBar1.ResumeLayout(false);
        this.toolBar1.PerformLayout();
        this.tabControl1.ResumeLayout(false);
        this.tabInfo.ResumeLayout(false);
        this.tabInfo.PerformLayout();
        this.tabReferences.ResumeLayout(false);
        this.tabReferences.PerformLayout();
        this.panel1.ResumeLayout(false);
        this.panel2.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion
    protected override void ViewSpecificLoad(object objectToLoad)
    {
        txtName.TextChanged -= txtName_TextChanged;
        txtCopyright.TextChanged -= txtCopyright_TextChanged;
        txtDescription.TextChanged -= txtDescription_TextChanged;

        if (!(objectToLoad is Package))
        {
            throw new ArgumentOutOfRangeException(
                "objectToLoad",
                objectToLoad,
                ResourceUtils.GetString("ErrorEditPackagesOnly")
            );
        }

        _package = objectToLoad as Package;
        txtName.Text = _package.Name;
        txtVersion.Text = _package.Version;
        txtCopyright.Text = _package.Copyright;
        txtDescription.Text = _package.Description;
        txtId.Text = _package.PrimaryKey["Id"].ToString();
        LoadReferences();

        txtName.TextChanged += txtName_TextChanged;
        txtCopyright.TextChanged += txtCopyright_TextChanged;
        txtDescription.TextChanged += txtDescription_TextChanged;
    }

    public override void SaveObject()
    {
        // save package info
        _package.Name = txtName.Text;
        _package.Copyright = txtCopyright.Text;
        _package.Description = txtDescription.Text;
        _package.Persist();
        // delete removed references
        foreach (PackageReference reference in _package.References)
        {
            bool found = false;
            foreach (ListViewItem item in lvwReferences.Items)
            {
                PackageReference newReference = item.Tag as PackageReference;
                if (newReference.ReferencedPackageId == reference.ReferencedPackageId)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                reference.Delete();
            }
        }
        // persist new/changed references
        foreach (ListViewItem item in lvwReferences.Items)
        {
            PackageReference newReference = item.Tag as PackageReference;
            bool found = false;
            foreach (PackageReference reference in _package.References)
            {
                if (newReference.ReferencedPackageId == reference.ReferencedPackageId)
                {
                    found = true;
                    // maybe user removed and added back the same reference, so we store the
                    // original reference with possibly changed values
                    // so far there is nothing to change, so we just skip doing anything
                    break;
                }
            }
            if (!found)
            {
                newReference.Persist();
            }
        }
        this.IsDirty = false;
        RefreshPads();
    }

    private static void RefreshPads()
    {
        SchemaBrowser modelBrowser =
            WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
        modelBrowser?.EbrSchemaBrowser.RefreshRootNodeText();
        ExtensionPad packageBrowser =
            WorkbenchSingleton.Workbench.GetPad(typeof(ExtensionPad)) as ExtensionPad;
        packageBrowser?.LoadPackages();
    }

    private void LoadReferences()
    {
        lvwReferences.BeginUpdate();
        try
        {
            lvwReferences.Items.Clear();
            foreach (PackageReference reference in _package.References)
            {
                Package referencedPackage =
                    _persistence.SchemaListProvider.RetrieveInstance(
                        typeof(Package),
                        new ModelElementKey(reference.ReferencedPackageId)
                    ) as Package;
                lvwReferences.Items.Add(RenderReference(reference, referencedPackage.Name));
            }
        }
        finally
        {
            lvwReferences.EndUpdate();
        }
    }

    private ListViewItem RenderReference(PackageReference reference, string name)
    {
        ListViewItem item = new ListViewItem(name, 2);
        item.Tag = reference;
        return item;
    }

    private Package SelectedReferencedPackage
    {
        get
        {
            if (lvwReferences.SelectedItems.Count == 1)
            {
                PackageReference reference = lvwReferences.SelectedItems[0].Tag as PackageReference;

                if (reference.IsPersisted)
                {
                    try
                    {
                        return (
                            lvwReferences.SelectedItems[0].Tag as PackageReference
                        ).ReferencedPackage;
                    }
                    catch
                    {
                        return null;
                    }
                }

                return null;
            }

            return null;
        }
    }

    private void lvwReferences_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        ebrElements.RemoveAllNodes();
        Package referencedPackage = this.SelectedReferencedPackage;
        if (referencedPackage != null)
        {
            ebrElements.AddRootNode(referencedPackage);
        }
    }

    private void ebrElements_QueryFilterNode(
        object sender,
        Origam.Workbench.ExpressionBrowserEventArgs e
    )
    {
        Package referencedPackage = this.SelectedReferencedPackage;
        if (referencedPackage != null)
        {
            if (e.QueriedObject is ISchemaItem)
            {
                if (!e.Filter)
                {
                    if ((e.QueriedObject as ISchemaItem).ParentItem == null)
                    {
                        e.Filter = !(e.QueriedObject as ISchemaItem).Package.PrimaryKey.Equals(
                            SelectedReferencedPackage.PrimaryKey
                        );
                    }
                    else
                    {
                        e.Filter = true;
                    }
                }
            }
            else if (e.QueriedObject is SchemaItemGroup)
            {
                e.Filter = ShouldFilterGroup(e.QueriedObject as SchemaItemGroup);
            }
            else if (e.QueriedObject is NonpersistentSchemaItemNode)
            {
                e.Filter = true;
            }
            else if (e.QueriedObject is SchemaItemAncestor)
            {
                e.Filter = true;
            }
        }
    }

    private bool ShouldFilterGroup(SchemaItemGroup group)
    {
        if (group.Package.PrimaryKey.Equals(SelectedReferencedPackage.PrimaryKey))
        {
            return false;
        }

        foreach (ISchemaItem child in group.ChildItems)
        {
            if (child.Package.PrimaryKey.Equals(SelectedReferencedPackage.PrimaryKey))
            {
                return false;
            }
            foreach (SchemaItemGroup childGroup in group.ChildGroups)
            {
                if (ShouldFilterGroup(childGroup) == false)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void toolBar1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        try
        {
            mnuAddReference.Items.Clear();
            if (e.ClickedItem == btnDelete)
            {
                if (lvwReferences.SelectedItems.Count == 1)
                {
                    PackageReference reference =
                        lvwReferences.SelectedItems[0].Tag as PackageReference;
                    if (
                        reference.IsPersisted == false
                        || IsPackageReferenced(SelectedReferencedPackage) == false
                    )
                    {
                        lvwReferences.SelectedItems[0].Remove();
                        this.IsDirty = true;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            ResourceUtils.GetString("ErrorRemovePackageReference")
                        );
                    }
                }
            }
            else if (e.ClickedItem == btnAddReference)
            {
                foreach (Package package in _schema.AllPackages)
                {
                    bool found = package.PrimaryKey.Equals(_schema.ActiveExtension.PrimaryKey);
                    foreach (ListViewItem li in lvwReferences.Items)
                    {
                        Guid loadedPackageId = (li.Tag as PackageReference).ReferencedPackageId;
                        if (loadedPackageId.Equals(package.PrimaryKey["Id"]))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        AsMenuCommand item = new AsMenuCommand(package.Name);
                        item.Image = imageList1.Images[2];
                        item.Tag = package;
                        item.Click += new EventHandler(AddPackage_Click);
                        mnuAddReference.Items.Add(item);
                    }
                }
                mnuAddReference.Show(toolBar1, new Point(toolBar1.Left, toolBar1.Bottom));
            }
        }
        catch (Exception ex)
        {
            Origam.UI.AsMessageBox.ShowError(
                this,
                ex.Message,
                ResourceUtils.GetString("ErrorTitle"),
                ex
            );
        }
    }

    private bool IsPackageReferenced(Package package)
    {
        if (package == null)
        {
            return false;
        }

        List<ISchemaItem> allCurrent =
            _package.PersistenceProvider.RetrieveListByPackage<ISchemaItem>(
                _schema.ActiveExtension.Id
            );
        List<ISchemaItem> allReferenced =
            _package.PersistenceProvider.RetrieveListByPackage<ISchemaItem>(package.Id);
        foreach (ISchemaItem item in allCurrent)
        {
            List<ISchemaItem> dep = item.GetDependencies(false);

            foreach (ISchemaItem refItem in allReferenced)
            {
                // check if parent item is not from the referenced package
                if (
                    item.ParentItem != null
                    && item.ParentItem.PrimaryKey.Equals(refItem.PrimaryKey)
                )
                {
                    return true;
                }
                // check if there is no reference to the referenced package
                foreach (ISchemaItem depItem in dep)
                {
                    if (depItem != null && depItem.PrimaryKey.Equals(refItem.PrimaryKey))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void AddPackage_Click(object sender, EventArgs e)
    {
        Package referencedPackage = (sender as AsMenuCommand).Tag as Package;

        foreach (PackageReference r in referencedPackage.References)
        {
            if (r.ReferencedPackage.PrimaryKey.Equals(_package.PrimaryKey))
            {
                MessageBox.Show(ResourceUtils.GetString("ErrorAddReference"));
                return;
            }
        }

        PackageReference reference = new PackageReference();
        reference.Package = _package;
        reference.ReferencedPackage = referencedPackage;
        reference.PersistenceProvider = _package.PersistenceProvider;
        lvwReferences.Items.Add(RenderReference(reference, referencedPackage.Name));
        this.IsDirty = true;
    }

    private void txtName_TextChanged(object sender, System.EventArgs e)
    {
        this.TitleName = txtName.Text;
        this.IsDirty = true;
    }

    private void txtCopyright_TextChanged(object sender, System.EventArgs e)
    {
        this.IsDirty = true;
    }

    private void txtDescription_TextChanged(object sender, System.EventArgs e)
    {
        this.IsDirty = true;
    }

    private void PackageEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        this.lvwReferences.SelectedIndexChanged -= new System.EventHandler(
            this.lvwReferences_SelectedIndexChanged
        );
        this.ebrElements.RemoveAllNodes();
    }
}
