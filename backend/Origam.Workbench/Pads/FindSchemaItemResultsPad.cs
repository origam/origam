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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Workbench.Commands;

namespace Origam.Workbench.Pads;

public class FindSchemaItemResultsPad : AbstractResultPad
{
    private System.Windows.Forms.ListView lvwResults;
    private System.Windows.Forms.ColumnHeader colItemType;
    private System.Windows.Forms.ColumnHeader colRootType;
    private System.Windows.Forms.ColumnHeader colItemPath;
    private System.Windows.Forms.ColumnHeader colFolderPath;
    private System.ComponentModel.IContainer components = null;
    private int sortColumn;
    private SchemaBrowser _schemaBrowser;
    private ColumnHeader colPackageName;
    private ColumnHeader colPackageReference;
    private List<ISchemaItem> _results = new();

    public FindSchemaItemResultsPad()
    {
        // This call is required by the Windows Form Designer.
        InitializeComponent();
        _schemaBrowser =
            WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
        lvwResults.SmallImageList = _schemaBrowser.EbrSchemaBrowser.imgList;
        lvwResults.ColumnClick += OnColumnClick;
    }

    private void OnColumnClick(object sender, ColumnClickEventArgs eventArgs)
    {
        if (eventArgs.Column != sortColumn)
        {
            sortColumn = eventArgs.Column;
            lvwResults.Sorting = SortOrder.Ascending;
        }
        else
        {
            lvwResults.Sorting =
                lvwResults.Sorting == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
        }
        this.lvwResults.ListViewItemSorter = new ListViewItemComparer(
            eventArgs.Column,
            lvwResults.Sorting
        );
        lvwResults.Sort();
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
            _schemaBrowser = null;
        }
        base.Dispose(disposing);
    }

    #region Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(FindSchemaItemResultsPad));
        this.lvwResults = new System.Windows.Forms.ListView();
        this.colItemPath = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.colRootType = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.colItemType = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.colFolderPath = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.colPackageName = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.colPackageReference = (
            (System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader())
        );
        this.SuspendLayout();
        //
        // lvwResults
        //
        this.lvwResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.lvwResults.Columns.AddRange(
            new System.Windows.Forms.ColumnHeader[]
            {
                this.colItemPath,
                this.colRootType,
                this.colItemType,
                this.colFolderPath,
                this.colPackageName,
                this.colPackageReference,
            }
        );
        this.lvwResults.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvwResults.FullRowSelect = true;
        this.lvwResults.Location = new System.Drawing.Point(0, 0);
        this.lvwResults.MultiSelect = false;
        this.lvwResults.Name = "lvwResults";
        this.lvwResults.Size = new System.Drawing.Size(816, 245);
        this.lvwResults.Sorting = System.Windows.Forms.SortOrder.Ascending;
        this.lvwResults.TabIndex = 0;
        this.lvwResults.UseCompatibleStateImageBehavior = false;
        this.lvwResults.View = System.Windows.Forms.View.Details;
        this.lvwResults.DoubleClick += new System.EventHandler(this.lvwResults_DoubleClick);
        this.lvwResults.KeyDown += new System.Windows.Forms.KeyEventHandler(
            this.lvwResults_KeyDown
        );
        //
        // colItemPath
        //
        this.colItemPath.Text = "Found In";
        this.colItemPath.Width = 266;
        //
        // colRootType
        //
        this.colRootType.Text = "Root Type";
        this.colRootType.Width = 165;
        //
        // colItemType
        //
        this.colItemType.Text = "Type";
        this.colItemType.Width = 120;
        //
        // colFolderPath
        //
        this.colFolderPath.Text = "Folder";
        this.colFolderPath.Width = 144;
        //
        // colPackageName
        //
        this.colPackageName.Text = "Package";
        this.colPackageName.Width = 130;
        //
        // colPackageReference
        //
        this.colPackageReference.Text = "Package Reference";
        this.colPackageReference.Width = 119;
        //
        // FindSchemaItemResultsPad
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(816, 245);
        this.Controls.Add(this.lvwResults);
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
        this.Name = "FindSchemaItemResultsPad";
        this.TabText = "Find Results";
        this.Text = "Find Results";
        this.ResumeLayout(false);
    }
    #endregion
    #region Public Methods
    public void ResetResults()
    {
        lvwResults.Items.Clear();
        _results.Clear();
        if (_schemaBrowser != null)
        {
            _schemaBrowser.RedrawContent();
        }
    }

    public void DisplayResults(ISchemaItem[] results)
    {
        ResetResults();
        if (results.Length > 0)
        {
            List<Guid> referencePackages = new List<Guid>();
            TreeNode treenode = (
                WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser
            ).EbrSchemaBrowser.GetFirstNode();
            if (treenode != null)
            {
                var itm = (Package)treenode.Tag;
                referencePackages = itm
                    .IncludedPackages.Select(x =>
                    {
                        return x.Id;
                    })
                    .ToList();
                referencePackages.Add(itm.Id);
                //if (!((SchemaExtension)treenode.Tag).Id.Equals(SchemaExtensionIdItem))
            }

            ListViewItem[] resultListItems = new ListViewItem[results.LongLength];
            for (int i = 0; i < results.LongLength; i++)
            {
                var item = results[i];
                resultListItems[i] = GetResult(item, referencePackages);
                _results.Add(item);
            }
            lvwResults.Items.AddRange(resultListItems);
            ViewFindSchemaItemResultsPad cmd = new ViewFindSchemaItemResultsPad();
            cmd.Run();
        }
        _schemaBrowser.RedrawContent();
    }

    public List<ISchemaItem> Results
    {
        get { return _results; }
    }

    private ListViewItem GetResult(ISchemaItem item, List<Guid> referencePackages)
    {
        if (item == null)
        {
            return null;
        }
        string name = item.ModelDescription();
        item.PersistenceProvider.RestrictToLoadedPackage(false);
        string rootName = item.RootItem.ModelDescription();
        if (name == null)
        {
            name = item.ItemType;
        }

        if (rootName == null)
        {
            rootName = item.RootItem.ItemType;
        }

        ListViewItem newItem = new ListViewItem(
            new string[]
            {
                item.Path,
                rootName,
                name,
                item.RootItem.Group == null ? "" : item.RootItem.Group.Path,
                item.PackageName,
                referencePackages.Contains(item.SchemaExtensionId) ? "Yes" : "No",
            }
        );
        newItem.Tag = item;
        newItem.ImageIndex = _schemaBrowser.ImageIndex(item.RootItem.Icon);
        item.PersistenceProvider.RestrictToLoadedPackage(true);
        return newItem;
    }
    #endregion
    private void ActivateItem()
    {
        if (lvwResults.SelectedItems.Count > 0)
        {
            try
            {
                ISchemaItem schemaItem = lvwResults.SelectedItems[0].Tag as ISchemaItem;
                OpenParentPackage(schemaItem.SchemaExtensionId);
                _schemaBrowser.EbrSchemaBrowser.SelectItem(schemaItem);
                ViewSchemaBrowserPad cmd = new ViewSchemaBrowserPad();
                cmd.Run();
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
    }

    private void lvwResults_DoubleClick(object sender, System.EventArgs e)
    {
        ActivateItem();
    }

    private void lvwResults_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            ActivateItem();
        }
    }

    public void Clear()
    {
        lvwResults.Items.Clear();
    }
}

internal class ListViewItemComparer : IComparer
{
    private readonly int col;
    private readonly SortOrder order;

    public ListViewItemComparer(int column, SortOrder order)
    {
        col = column;
        this.order = order;
    }

    public int Compare(object x, object y)
    {
        int returnVal = -1;
        returnVal = String.Compare(
            ((ListViewItem)x).SubItems[col].Text,
            ((ListViewItem)y).SubItems[col].Text
        );
        if (order == SortOrder.Descending)
        {
            returnVal *= -1;
        }
        return returnVal;
    }
}
