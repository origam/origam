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

using Origam.Workbench.Pads;
using Origam.Gui.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using core = Origam.Workbench.Services.CoreServices;
namespace Origam.Workbench;
public partial class SqlViewer : AbstractViewContent, IToolStripContainer
{
    public SqlViewer(Platform platform)
    {
        InitializeComponent();
        Platform = platform;
    }
    public override object Content
    {
        get
        {
            return editor.Text; ;
        }
        set
        {
            editor.Text = value as string;
        }
    }
    public event EventHandler ToolStripsLoaded
    {
        add { }
        remove { }
    }
    public event EventHandler AllToolStripsRemoved
    {
        add { }
        remove { }
    }
    public event EventHandler ToolStripsNeedUpdate
    {
        add { }
        remove { }
    }
    private void btnExecuteSql_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(editor.Text))
        {
            return;
        }
        var dataService = core.DataServiceFactory.GetDataService(Platform);
        string result = dataService.ExecuteUpdate(editor.Text, null);
        OutputPad outputPad = WorkbenchSingleton.Workbench.GetPad(
            typeof(OutputPad)) as OutputPad;
        outputPad.SetOutputText(result);
        outputPad.Show();
        editor.Focus();
    }
    protected override void ViewSpecificLoad(object objectToLoad)
    {
        this.Content = objectToLoad;
    }
    public override bool IsViewOnly
    {
        get
        {
            return true;
        }
        set
        {
            base.IsViewOnly = value;
        }
    }
    public Platform Platform { get; }
    public List<ToolStrip> GetToolStrips(int maxWidth = -1)
    {
        return new List<ToolStrip> { toolStrip1 };
    }
}
