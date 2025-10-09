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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Origam.DA;
using Origam.Schema;

namespace Origam.Workbench;

/// <summary>
/// Summary description for SchemaCompareEditor.
/// </summary>
public class SchemaCompareEditor : AbstractViewContent
{
    private System.Windows.Forms.ColumnHeader colType;
    private System.Windows.Forms.ColumnHeader colName;
    private System.Windows.Forms.ColumnHeader colDifference;
    private System.Windows.Forms.ColumnHeader colRemark;
    private System.Windows.Forms.ListView lvwResults;
    private System.Windows.Forms.ToolBar toolBar1;
    private System.Windows.Forms.ToolBarButton btnScript;
    private System.Windows.Forms.ToolBarButton btnImport;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public SchemaCompareEditor()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
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
        this.colType = new System.Windows.Forms.ColumnHeader();
        this.colName = new System.Windows.Forms.ColumnHeader();
        this.colDifference = new System.Windows.Forms.ColumnHeader();
        this.colRemark = new System.Windows.Forms.ColumnHeader();
        this.lvwResults = new System.Windows.Forms.ListView();
        this.toolBar1 = new System.Windows.Forms.ToolBar();
        this.btnScript = new System.Windows.Forms.ToolBarButton();
        this.btnImport = new System.Windows.Forms.ToolBarButton();
        this.SuspendLayout();
        //
        // colType
        //
        this.colType.Text = "Type";
        this.colType.Width = 109;
        //
        // colName
        //
        this.colName.Text = "Name";
        this.colName.Width = 259;
        //
        // colDifference
        //
        this.colDifference.Text = "Difference";
        this.colDifference.Width = 118;
        //
        // colRemark
        //
        this.colRemark.Text = "Remark";
        this.colRemark.Width = 369;
        //
        // lvwResults
        //
        this.lvwResults.AllowColumnReorder = true;
        this.lvwResults.CheckBoxes = true;
        this.lvwResults.Columns.AddRange(
            new System.Windows.Forms.ColumnHeader[]
            {
                this.colType,
                this.colName,
                this.colDifference,
                this.colRemark,
            }
        );
        this.lvwResults.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvwResults.FullRowSelect = true;
        this.lvwResults.GridLines = true;
        this.lvwResults.Location = new System.Drawing.Point(0, 28);
        this.lvwResults.Name = "lvwResults";
        this.lvwResults.Size = new System.Drawing.Size(640, 329);
        this.lvwResults.Sorting = System.Windows.Forms.SortOrder.Ascending;
        this.lvwResults.TabIndex = 0;
        this.lvwResults.View = System.Windows.Forms.View.Details;
        this.lvwResults.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(
            this.lvwResults_ItemCheck
        );
        //
        // toolBar1
        //
        this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolBar1.Buttons.AddRange(
            new System.Windows.Forms.ToolBarButton[] { this.btnScript, this.btnImport }
        );
        this.toolBar1.ButtonSize = new System.Drawing.Size(100, 22);
        this.toolBar1.DropDownArrows = true;
        this.toolBar1.Location = new System.Drawing.Point(0, 0);
        this.toolBar1.Name = "toolBar1";
        this.toolBar1.ShowToolTips = true;
        this.toolBar1.Size = new System.Drawing.Size(640, 28);
        this.toolBar1.TabIndex = 1;
        this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
        this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(
            this.toolBar1_ButtonClick
        );
        //
        // btnScript
        //
        this.btnScript.Text = "Script";
        //
        // btnImport
        //
        this.btnImport.Text = "Import";
        //
        // SchemaCompareEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(640, 357);
        this.Controls.Add(this.lvwResults);
        this.Controls.Add(this.toolBar1);
        this.DockableAreas = WeifenLuo.WinFormsUI.DockAreas.Document;
        this.IsViewOnly = true;
        this.Name = "SchemaCompareEditor";
        this.ShowInTaskbar = false;
        this.TabText = "Schema Compare:";
        this.Text = "Schema Compare:";
        this.TitleName = "Schema Compare:";
        this.ResumeLayout(false);
    }
    #endregion
    #region Public Methods
    public void DisplayResults(ArrayList results)
    {
        foreach (SchemaDbCompareResult result in results)
        {
            ListViewItem item = new ListViewItem(
                new string[]
                {
                    SchemaItemDescription(result.SchemaItemType),
                    result.ItemName,
                    result.ResultType.ToString(),
                    result.Remark,
                }
            );

            if (result.ResultType == DbCompareResultType.ExistingButDifferent)
            {
                item.ForeColor = Color.Gray;
            }
            item.Tag = result;
            lvwResults.Items.Add(item);
        }
    }
    #endregion
    #region Private Methods
    private string SchemaItemDescription(Type type)
    {
        object[] attributes = type.GetCustomAttributes(
            typeof(SchemaItemDescriptionAttribute),
            true
        );
        if (attributes != null && attributes.Length > 0)
            return (attributes[0] as SchemaItemDescriptionAttribute).Name;
        else
            return type.Name;
    }

    private ArrayList SelectedResults()
    {
        ArrayList result = new ArrayList();
        foreach (ListViewItem item in lvwResults.CheckedItems)
        {
            result.Add(item.Tag);
        }
        return result;
    }
    #endregion
    #region Event Handlers
    private void lvwResults_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
    {
        if (
            (lvwResults.Items[e.Index].Tag as SchemaDbCompareResult).ResultType
            == DbCompareResultType.ExistingButDifferent
        )
        {
            e.NewValue = CheckState.Unchecked;
        }
    }

    private void toolBar1_ButtonClick(
        object sender,
        System.Windows.Forms.ToolBarButtonClickEventArgs e
    )
    {
        Pads.OutputPad _pad =
            WorkbenchSingleton.Workbench.GetPad(typeof(Pads.OutputPad)) as Pads.OutputPad;
        if (e.Button == btnImport) { }
        else if (e.Button == btnScript)
        {
            StringBuilder script = new StringBuilder();
            foreach (SchemaDbCompareResult result in SelectedResults())
            {
                if (
                    result.ResultType == DbCompareResultType.MissingInDatabase
                    & result.Script != ""
                )
                {
                    script.Append(result.Script);
                    script.Append(Environment.NewLine);
                    script.Append(Environment.NewLine);
                }
            }
            foreach (SchemaDbCompareResult result in SelectedResults())
            {
                if (
                    result.ResultType == DbCompareResultType.MissingInDatabase
                    & result.Script2 != ""
                )
                {
                    script.Append(result.Script2);
                    script.Append(Environment.NewLine);
                    script.Append(Environment.NewLine);
                }
            }
            _pad.SetOutputText(script.ToString());
        }
    }
    #endregion
}
