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
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workbench;

public class SchemaBrowser : AbstractPadContent, IBrowserPad
{
    public ExpressionBrowser EbrSchemaBrowser;
    private readonly System.ComponentModel.Container components = null;
    private readonly SchemaService schemaService =
        ServiceManager.Services.GetService<SchemaService>();
    public ImageList ImageList => EbrSchemaBrowser.imgList;

    public SchemaBrowser()
    {
        InitializeComponent();
        schemaService.SchemaLoaded += _schemaService_SchemaLoaded;
        schemaService.SchemaUnloaded += SchemaService_SchemaUnloaded;
    }

    private void SchemaService_SchemaUnloaded(object sender, EventArgs e)
    {
        EbrSchemaBrowser.RemoveAllNodes();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
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
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(SchemaBrowser));
        this.EbrSchemaBrowser = new Origam.Workbench.ExpressionBrowser();
        this.SuspendLayout();
        //
        // EbrSchemaBrowser
        //
        this.EbrSchemaBrowser.AllowEdit = true;
        this.EbrSchemaBrowser.CheckSecurity = false;
        this.EbrSchemaBrowser.DisableOtherExtensionNodes = true;
        this.EbrSchemaBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.EbrSchemaBrowser.Location = new System.Drawing.Point(0, 0);
        this.EbrSchemaBrowser.Name = "EbrSchemaBrowser";
        this.EbrSchemaBrowser.NodeUnderMouse = null;
        this.EbrSchemaBrowser.ShowFilter = false;
        this.EbrSchemaBrowser.Size = new System.Drawing.Size(292, 271);
        this.EbrSchemaBrowser.TabIndex = 1;
        //
        // SchemaBrowser
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(292, 271);
        this.Controls.Add(this.EbrSchemaBrowser);
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
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "SchemaBrowser";
        this.TabText = "Model Browser";
        this.Text = "Model Browser";
        this.ResumeLayout(false);
    }
    #endregion
    private void _schemaService_SchemaLoaded(object sender, bool isInteractive)
    {
        EbrSchemaBrowser.RemoveAllNodes();
        EbrSchemaBrowser.AddRootNode(schemaService.ActiveExtension);
    }

    public override void RedrawContent()
    {
        EbrSchemaBrowser.Redraw();
    }

    public int ImageIndex(string icon)
    {
        return this.ImageList.ImageIndex(icon);
    }
}
