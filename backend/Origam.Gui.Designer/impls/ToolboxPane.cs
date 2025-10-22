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

//------------------------------------------------------------------------------
/// <copyright from='1997' to='2002' company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///
///    This source code is intended only as a supplement to Microsoft
///    Development Tools and/or on-line documentation.  See these other
///    materials for detailed information regarding Microsoft code samples.
///
/// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using Origam.Schema.GuiModel;
using Origam.UI;

namespace Origam.Gui.Designer;

/// Our implementation of a toolbox. We kept the actual toolbox control
/// separate from the IToolboxService, but the toolbox could easily
/// implement the service itself. Note that IToolboxUser does not pertain
/// to the toolbox, but instead is implemented by the designers that receive
/// ToolboxItems.
public class ToolboxPane : System.Windows.Forms.UserControl
{
    private DesignerHostImpl host;
    private ToolboxItem pointer; // a "null" tool
    private int selectedIndex; // the index of the currently selected tool
    private bool initialPaint = true; // see ToolboxPane_Paint method

    // We load types into our categories in a rather primitive way. It is easier than
    // dealing with type resolution, but we can only do this since our list of tools
    // is standard and unchanging.
    //
    private System.Windows.Forms.ListBox listCommon;
    private System.Windows.Forms.ListBox listSpecial;
    private System.Windows.Forms.TabPage tabSpecial;
    private System.Windows.Forms.TabPage tabCommon;
    private System.Windows.Forms.TabControl tabControl;

    private System.ComponentModel.Container components = null;

    public ToolboxPane()
    {
        // This call is required by the Windows.Forms Form Designer.
        InitializeComponent();
        pointer = new ToolboxItem();
        pointer.DisplayName = "<Pointer>";
        pointer.Bitmap = new Bitmap(16, 16);

        tabCommon.BackColor = OrigamColorScheme.ToolbarBaseColor;
        tabSpecial.BackColor = OrigamColorScheme.ToolbarBaseColor;
        listCommon.BackColor = OrigamColorScheme.ToolbarBaseColor;
        listSpecial.BackColor = OrigamColorScheme.ToolbarBaseColor;

        ListBox list = this.tabControl.SelectedTab.Controls[0] as ListBox;
    }

    /// Clean up any resources being used.
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

    // Properties

    /// We need access to the designers.
    public DesignerHostImpl Host
    {
        get { return host; }
        set { host = value; }
    }

    /// The currently selected tool is defined by our currently selected
    /// category (ListBox) and our selectedIndex member.
    public ToolboxItem SelectedTool
    {
        get
        {
            if (this.tabControl.SelectedTab.Controls.Count == 0)
            {
                return null;
            }

            ListBox list = this.tabControl.SelectedTab.Controls[0] as ListBox;
            if (list.Items.Count == 0)
            {
                return null;
            }

            return list.Items[selectedIndex] as ToolboxItem;
        }
    }

    /// The name of our selected category (Windows Forms, Components, etc.)
    /// This property (and the next few) are all in place to support
    /// methods of the IToolboxService.
    public string SelectedCategory
    {
        get { return tabControl.SelectedTab.Text; }
    }

    /// The names of all our categories.
    public CategoryNameCollection CategoryNames
    {
        get
        {
            string[] categories = new string[tabControl.TabPages.Count];
            for (int i = 0; i < tabControl.TabPages.Count; i++)
            {
                categories[i] = tabControl.TabPages[i].Text;
            }
            return new CategoryNameCollection(categories);
        }
    }

    // Methods

    /// The IToolboxService has methods for getting tool collections using
    /// an optional category parameter. We support that request here.
    public ToolboxItemCollection GetToolsFromCategory(string category)
    {
        foreach (TabPage tab in tabControl.TabPages)
        {
            if (tab.Text == category)
            {
                ListBox list = tab.Controls[0] as ListBox;
                ToolboxItem[] tools = new ToolboxItem[list.Items.Count];
                list.Items.CopyTo(tools, 0);
                return new ToolboxItemCollection(tools);
            }
        }

        return null;
    }

    /// Get all of our tools.
    public ToolboxItemCollection GetAllTools()
    {
        var toolsAL = new List<object>();
        foreach (TabPage tab in tabControl.TabPages)
        {
            ListBox list = tab.Controls[0] as ListBox;
            toolsAL.Add(list.Items);
        }
        ToolboxItem[] tools = new ToolboxItem[toolsAL.Count];
        toolsAL.CopyTo(tools);
        return new ToolboxItemCollection(tools);
    }

    /// Resets the selection to the pointer. Note that this is the only method
    /// which allows our IToolboxService to set our selection. It calls this method
    /// after a tool has been used.
    public void SelectPointer()
    {
        if (DesignMode)
        {
            return;
        }
        ListBox list = this.tabControl.SelectedTab.Controls[0] as ListBox;
        if (list.SelectedIndex > 0)
        {
            list.Invalidate(list.GetItemRectangle(list.SelectedIndex));
        }
        selectedIndex = 0;
        list.SelectedIndex = 0;
        list.Invalidate(list.GetItemRectangle(selectedIndex));
    }

    #region Component Designer generated code

    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    private void InitializeComponent()
    {
        this.listCommon = new System.Windows.Forms.ListBox();
        this.listSpecial = new System.Windows.Forms.ListBox();
        this.tabSpecial = new System.Windows.Forms.TabPage();
        this.tabCommon = new System.Windows.Forms.TabPage();
        this.tabControl = new System.Windows.Forms.TabControl();
        this.tabSpecial.SuspendLayout();
        this.tabCommon.SuspendLayout();
        this.tabControl.SuspendLayout();
        this.SuspendLayout();
        //
        // listCommon
        //
        this.listCommon.BackColor = System.Drawing.Color.LightSlateGray;
        this.listCommon.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listCommon.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
        this.listCommon.Location = new System.Drawing.Point(0, 0);
        this.listCommon.Name = "listCommon";
        this.listCommon.Size = new System.Drawing.Size(280, 526);
        this.listCommon.TabIndex = 0;
        this.listCommon.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
        this.listCommon.MouseDown += new System.Windows.Forms.MouseEventHandler(
            this.list_MouseDown
        );
        this.listCommon.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(
            this.list_MeasureItem
        );
        this.listCommon.DrawItem += new System.Windows.Forms.DrawItemEventHandler(
            this.list_DrawItem
        );
        //
        // listSpecial
        //
        this.listSpecial.BackColor = System.Drawing.Color.LightSlateGray;
        this.listSpecial.Dock = System.Windows.Forms.DockStyle.Fill;
        this.listSpecial.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
        this.listSpecial.Location = new System.Drawing.Point(0, 0);
        this.listSpecial.Name = "listSpecial";
        this.listSpecial.Size = new System.Drawing.Size(280, 526);
        this.listSpecial.TabIndex = 0;
        this.listSpecial.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
        this.listSpecial.MouseDown += new System.Windows.Forms.MouseEventHandler(
            this.list_MouseDown
        );
        this.listSpecial.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(
            this.list_MeasureItem
        );
        this.listSpecial.DrawItem += new System.Windows.Forms.DrawItemEventHandler(
            this.list_DrawItem
        );
        //
        // tabSpecial
        //
        this.tabSpecial.BackColor = System.Drawing.Color.LightSlateGray;
        this.tabSpecial.Controls.Add(this.listSpecial);
        this.tabSpecial.Location = new System.Drawing.Point(4, 22);
        this.tabSpecial.Name = "tabSpecial";
        this.tabSpecial.Size = new System.Drawing.Size(280, 526);
        this.tabSpecial.TabIndex = 1;
        this.tabSpecial.Text = "Entity not selected";
        //
        // tabCommon
        //
        this.tabCommon.BackColor = System.Drawing.Color.LightSlateGray;
        this.tabCommon.Controls.Add(this.listCommon);
        this.tabCommon.Location = new System.Drawing.Point(4, 22);
        this.tabCommon.Name = "tabCommon";
        this.tabCommon.Size = new System.Drawing.Size(280, 526);
        this.tabCommon.TabIndex = 3;
        this.tabCommon.Text = "Custom Controls";
        //
        // tabControl
        //
        this.tabControl.Controls.Add(this.tabSpecial);
        this.tabControl.Controls.Add(this.tabCommon);
        this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControl.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            8.25F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.Point,
            ((System.Byte)(238))
        );
        this.tabControl.ItemSize = new System.Drawing.Size(99, 18);
        this.tabControl.Location = new System.Drawing.Point(0, 0);
        this.tabControl.Multiline = true;
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new System.Drawing.Size(288, 552);
        this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
        this.tabControl.TabIndex = 1;
        this.tabControl.SelectedIndexChanged += new System.EventHandler(
            this.tabControl_SelectedIndexChanged
        );
        //
        // ToolboxPane
        //
        this.Controls.Add(this.tabControl);
        this.Name = "ToolboxPane";
        this.Size = new System.Drawing.Size(288, 552);
        this.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolboxPane_Paint);
        this.tabSpecial.ResumeLayout(false);
        this.tabCommon.ResumeLayout(false);
        this.tabControl.ResumeLayout(false);
        this.ResumeLayout(false);
    }
    #endregion

    public void LoadToolbox(FDToolbox tools)
    {
        foreach (TabPage tab in tabControl.TabPages)
        {
            ListBox list = tab.Controls[0] as ListBox;
            list.Items.Clear();
            list.Items.Add(pointer);
        }

        if (
            tools != null
            && tools.FDToolboxCategories != null
            && tools.FDToolboxCategories.Length > 0
        )
        {
            int i = 0;
            foreach (Category cat in tools.FDToolboxCategories)
            {
                LoadCategory(cat, i);
                i++;
            }
        }
    }

    private void LoadCategory(Category cat, int tabPageNumber)
    {
        // if we have items in the category...
        if (cat != null && cat.FDToolboxItem != null && cat.FDToolboxItem.Length > 0)
        {
            TabPage tab = tabControl.TabPages[tabPageNumber];
            tab.Text = cat.DisplayName;
            ListBox listBox = tab.Controls[0] as ListBox;
            foreach (FDToolboxItem item in cat.FDToolboxItem)
            {
                LoadItem(item, listBox);
            }
        }
    }

    private void LoadItem(FDToolboxItem item, ListBox listBox)
    {
        if (item == null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(item.Type) || item.Type.Trim() == ",")
        {
            throw new AggregateException(
                string.Format(Workbench.Strings.CannotLoadItem, item.Name)
            );
        }
        // load the type
        string[] assemblyClass = item.Type.Split(new char[] { ',' });
        if (assemblyClass[0].Trim() == "" || assemblyClass[1].Trim() == "")
        {
            throw new AggregateException(
                string.Format(Workbench.Strings.CannotLoadItem, item.Name)
            );
        }
        Type toolboxItemType = GetTypeFromLoadedAssembly(
            classname: assemblyClass[0],
            assembly: assemblyClass[1]
        );
        ModelToolboxItem ti = new ModelToolboxItem(toolboxItemType);
        ToolboxBitmapAttribute tba =
            TypeDescriptor.GetAttributes(toolboxItemType)[typeof(ToolboxBitmapAttribute)]
            as ToolboxBitmapAttribute;
        if (tba != null)
        {
            ti.Bitmap = (Bitmap)tba.GetImage(toolboxItemType);
        }

        ti.ControlItem = item.ControlItem;
        ti.DisplayName = item.Name;

        //add ControlSetItem
        if (item.IsComplexType)
        {
            ti.IsComplexType = true;
            ti.PanelControlSet = item.PanelSetItem;
        }
        else if (item.IsExternal)
        {
            ti.IsComplexType = false;
            ti.IsExternal = true;
            ti.IsFieldControl = item.IsFieldItem;
        }

        if (item.IsFieldItem)
        {
            ti.IsFieldControl = true;
            ti.FieldName = item.ColumnName;
        }

        listBox.Items.Add(ti);
    }

    private Type GetTypeFromLoadedAssembly(string classname, string assembly)
    {
        // try to load it with a partial name
        // Assembly asm = Assembly.Load(classname + "," + assembly);

#pragma warning disable CS0618 // Type or member is obsolete, Assembly.Load(classname + "," + assembly); does not work
        Assembly asm = Assembly.LoadWithPartialName(assembly);
#pragma warning restore CS0618 // Type or member is obsolete
        if (asm != null)
        {
            return asm.GetType(classname);
        }

        throw new Exception(
            $"Could not load: {classname} from assembly: {assembly}. Check that the class name and assembly name are correct"
        );
    }

    // Event Handlers

    /// Each ToolboxItem contains a string and a bitmap. We draw each of these each time
    /// we draw a ListBox item (a tool).
    private void list_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
    {
        ListBox lbSender = sender as ListBox;

        SolidBrush backgroundBrush = null;
        SolidBrush foregroundBrush = null;

        try
        {
            // If this tool is the currently selected tool, draw it with a highlight.
            if (selectedIndex == e.Index)
            {
                backgroundBrush = new SolidBrush(OrigamColorScheme.GridSelectionBackColor);
                foregroundBrush = new SolidBrush(OrigamColorScheme.GridSelectionForeColor);
            }
            else
            {
                backgroundBrush = new SolidBrush(OrigamColorScheme.GridHeaderBackColor);
                foregroundBrush = new SolidBrush(OrigamColorScheme.GridHeaderForeColor);
            }

            e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            ToolboxItem tbi = lbSender.Items[e.Index] as ToolboxItem;
            Rectangle BitmapBounds = new Rectangle(
                e.Bounds.Location.X,
                e.Bounds.Location.Y,
                tbi.Bitmap.Width,
                e.Bounds.Height
            );
            Rectangle StringBounds = new Rectangle(
                e.Bounds.Location.X + BitmapBounds.Width,
                e.Bounds.Location.Y,
                e.Bounds.Width - BitmapBounds.Width,
                e.Bounds.Height
            );
            e.Graphics.DrawImage(tbi.Bitmap, BitmapBounds);
            e.Graphics.DrawString(tbi.DisplayName, lbSender.Font, foregroundBrush, StringBounds);
        }
        finally
        {
            if (backgroundBrush != null)
            {
                backgroundBrush.Dispose();
            }

            if (foregroundBrush != null)
            {
                foregroundBrush.Dispose();
            }
        }
    }

    /// We measure each item by taking the combined width of the string and bitmap,
    /// and the greater height of the two.
    private void list_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
    {
        ListBox lbSender = sender as ListBox;
        ToolboxItem tbi = lbSender.Items[e.Index] as ToolboxItem;
        Size textSize = e.Graphics.MeasureString(tbi.DisplayName, lbSender.Font).ToSize();
        e.ItemWidth = tbi.Bitmap.Width + textSize.Width;
        if (tbi.Bitmap.Height > textSize.Height)
        {
            e.ItemHeight = tbi.Bitmap.Height;
        }
        else
        {
            e.ItemHeight = textSize.Height;
        }
    }

    /// This method handles a MouseDown event, which might be one of three things:
    ///		1) the start of a single click
    ///		2) the start of a drag
    ///		3) the start of a second of two consecutive clicks (double-click)
    private void list_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        // Regardless of which kind of click this is, we need to change the selection.
        // First we grab the bounds of the old selected tool so that we can de-higlight it.
        //
        ListBox lbSender = sender as ListBox;
        Rectangle lastSelectedBounds = lbSender.GetItemRectangle(selectedIndex);

        selectedIndex = lbSender.IndexFromPoint(e.X, e.Y); // change our selection
        lbSender.SelectedIndex = selectedIndex;

        lbSender.Invalidate(lastSelectedBounds); // clear highlight from last selection
        lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // highlight new one

        if (selectedIndex != 0)
        {
            ModelToolboxItem tbi = (ModelToolboxItem)lbSender.Items[selectedIndex];

            this.Host.IsComplexControl = tbi.IsComplexType;

            if (tbi.IsComplexType)
            {
                this.Host.PanelSet = tbi.PanelControlSet;
            }
            else if (tbi.IsExternal)
            {
                this.Host.IsExternalControl = true;
            }

            // only assign FieldName when entity field list item is selected so then when the user
            // selects an item from the common widgets it gets data-bound to the field selected in the
            // field list
            if (lbSender.Equals(listSpecial) && selectedIndex != 0)
            {
                ModelToolboxItem fieldItem = (ModelToolboxItem)lbSender.Items[selectedIndex];
                this.Host.IsFieldControl = fieldItem.IsFieldControl;
                this.Host.FieldName = fieldItem.FieldName;
            }

            // If this is a double-click, then the user wants to add the selected component
            // to the default location on the designer, with the default size. We call
            // ToolPicked on the current designer (as a IToolboxUser) to place the tool.
            // The IToolboxService calls SelectedToolboxItemUsed(), which calls this control's
            // SelectPointer() method.
            //
            if (e.Clicks == 2)
            {
                IToolboxUser tbu = host.GetDesigner(host.RootComponent) as IToolboxUser;
                if (tbu != null)
                {
                    tbu.ToolPicked(tbi);
                }
            }
            // Otherwise this is either a single click or a drag. Either way, we do the same
            // thing: start a drag--if this is just a single click, then the drag will
            // abort as soon as there's a MouseUp event.
            //
            else if (e.Clicks < 2)
            {
                IToolboxService tbs = host.GetService(typeof(IToolboxService)) as IToolboxService;

                // The IToolboxService serializes ToolboxItems by packaging them in DataObjects.
                DataObject d = tbs.SerializeToolboxItem(tbi) as DataObject;
                try
                {
                    DragAndDropControl = (
                        d.GetData(d.GetFormats()[0]) as ModelToolboxItem
                    ).ControlItem;
                    lbSender.DoDragDrop(d, DragDropEffects.Copy);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }
    }

    public static ControlItem DragAndDropControl;

    /// Go to the pointer whenever we change categories.
    private void tabControl_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        SelectPointer();
    }

    /// On our first paint, select the pointer.
    private void ToolboxPane_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
    {
        if (initialPaint)
        {
            SelectPointer();
        }
        initialPaint = false;
    }

    /// The toolbox can also be navigated using the keyboard commands Up, Down, and Enter.
    private void list_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        ListBox lbSender = sender as ListBox;
        Rectangle lastSelectedBounds = lbSender.GetItemRectangle(selectedIndex);
        switch (e.KeyCode)
        {
            case Keys.Up:
            {
                if (selectedIndex > 0)
                {
                    selectedIndex--; // change selection
                    lbSender.SelectedIndex = selectedIndex;
                    lbSender.Invalidate(lastSelectedBounds); // clear old highlight
                    lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // add new one
                }
                break;
            }

            case Keys.Down:
            {
                if (selectedIndex + 1 < lbSender.Items.Count)
                {
                    selectedIndex++; // change selection
                    lbSender.SelectedIndex = selectedIndex;
                    lbSender.Invalidate(lastSelectedBounds); // clear old highlight
                    lbSender.Invalidate(lbSender.GetItemRectangle(selectedIndex)); // add new one
                }
                break;
            }

            case Keys.Enter:
            {
                IToolboxUser tbu = host.GetDesigner(host.RootComponent) as IToolboxUser;
                if (tbu != null)
                {
                    // Enter means place the tool with default location and default size.
                    tbu.ToolPicked((ToolboxItem)(lbSender.Items[selectedIndex]));
                }
                break;
            }
        }
    }
}
