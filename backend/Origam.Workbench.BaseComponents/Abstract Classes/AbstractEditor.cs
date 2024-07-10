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
using System.Linq;
using System.Windows.Forms;
using Origam.Extensions;
using Origam.Gui;
using Origam.Gui.UI;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Type = System.Type;

namespace Origam.Workbench.Editors;
public class AbstractEditor : AbstractViewContent, IToolStripContainer	
{
	public event EventHandler ContentLoaded;
	IDocumentationService _documentation = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
    private Panel toolPanel;
    private ToolStrip toolStrip1;
    private ToolStripLabel actionsLabel;
    private ToolStripLabel newElementsLabel;
    private Panel headerPanel;
    private Label lblName;
    private PictureBox elementPicture;
    private Label lblType;
    ToolStripMenuItem _saveCmd = new ToolStripMenuItem("Save", ImageRes.Save_16x);
    ToolStripMenuItem dockCmd = new ToolStripMenuItem("Dock", ImageRes.dock);
    private ISubmenuBuilder _actionsBuilder = null;
    private ISubmenuBuilder _newElementsBuilder = null;
    protected bool showMenusInAppToolStrip = false;
    public override object Content { get; set; }
	public AbstractSchemaItem ModelContent
    {
        get
        {
            return Content as AbstractSchemaItem;
        }
        set
        {
            Content = value;
        }
    }
	private void InitializeComponent()
	{
        this.toolPanel = new System.Windows.Forms.Panel();
        this.toolStrip1 = new System.Windows.Forms.ToolStrip();
        this.actionsLabel = new System.Windows.Forms.ToolStripLabel();
        this.newElementsLabel = new System.Windows.Forms.ToolStripLabel();
        this.headerPanel = new System.Windows.Forms.Panel();
        this.lblName = new System.Windows.Forms.Label();
        this.lblType = new System.Windows.Forms.Label();
        this.elementPicture = new System.Windows.Forms.PictureBox();
        this.toolPanel.SuspendLayout();
        this.toolStrip1.SuspendLayout();
        this.headerPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.elementPicture)).BeginInit();
        this.SuspendLayout();
        // 
        // toolPanel
        // 
        this.toolPanel.AutoSize = true;
        this.toolPanel.Controls.Add(this.toolStrip1);
        this.toolPanel.Dock = System.Windows.Forms.DockStyle.Right;
        this.toolPanel.Location = new System.Drawing.Point(635, 40);
        this.toolPanel.Name = "toolPanel";
        this.toolPanel.Size = new System.Drawing.Size(79, 521);
        this.toolPanel.TabIndex = 2;
        // 
        // toolStrip1
        // 
        this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
        this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
        this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.actionsLabel,
        this.newElementsLabel});
        this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
        this.toolStrip1.Location = new System.Drawing.Point(0, 0);
        this.toolStrip1.Name = "toolStrip1";
        this.toolStrip1.Padding = new System.Windows.Forms.Padding(10);
        this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
        this.toolStrip1.Size = new System.Drawing.Size(79, 90);
        this.toolStrip1.Stretch = true;
        this.toolStrip1.TabIndex = 2;
        this.toolStrip1.Text = "toolStrip1";
        // 
        // actionsLabel
        // 
        this.actionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.actionsLabel.Name = "actionsLabel";
        this.actionsLabel.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
        this.actionsLabel.Size = new System.Drawing.Size(59, 32);
        this.actionsLabel.Text = "Actions";
        // 
        // newElementsLabel
        // 
        this.newElementsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.newElementsLabel.Name = "newElementsLabel";
        this.newElementsLabel.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
        this.newElementsLabel.Size = new System.Drawing.Size(38, 32);
        this.newElementsLabel.Text = "New";
        // 
        // headerPanel
        // 
        this.headerPanel.Controls.Add(this.lblName);
        this.headerPanel.Controls.Add(this.lblType);
        this.headerPanel.Controls.Add(this.elementPicture);
        this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.headerPanel.Location = new System.Drawing.Point(0, 0);
        this.headerPanel.Name = "headerPanel";
        this.headerPanel.Size = new System.Drawing.Size(714, 40);
        this.headerPanel.TabIndex = 0;
        // 
        // lblName
        // 
        this.lblName.AutoSize = true;
        this.lblName.Dock = System.Windows.Forms.DockStyle.Left;
        this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.lblName.Location = new System.Drawing.Point(74, 0);
        this.lblName.Name = "lblName";
        this.lblName.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
        this.lblName.Size = new System.Drawing.Size(57, 30);
        this.lblName.TabIndex = 1;
        this.lblName.Text = "label1";
        this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lblType
        // 
        this.lblType.AutoSize = true;
        this.lblType.BackColor = System.Drawing.Color.Transparent;
        this.lblType.Dock = System.Windows.Forms.DockStyle.Left;
        this.lblType.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.lblType.Location = new System.Drawing.Point(16, 0);
        this.lblType.Name = "lblType";
        this.lblType.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
        this.lblType.Size = new System.Drawing.Size(58, 30);
        this.lblType.TabIndex = 1;
        this.lblType.Text = "lblType";
        this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // elementPicture
        // 
        this.elementPicture.Dock = System.Windows.Forms.DockStyle.Left;
        this.elementPicture.Location = new System.Drawing.Point(0, 0);
        this.elementPicture.Name = "elementPicture";
        this.elementPicture.Padding = new System.Windows.Forms.Padding(12, 12, 12, 16);
        this.elementPicture.Size = new System.Drawing.Size(16, 40);
        this.elementPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.elementPicture.TabIndex = 0;
        this.elementPicture.TabStop = false;
        // 
        // AbstractEditor
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
        this.ClientSize = new System.Drawing.Size(714, 561);
        this.Controls.Add(this.toolPanel);
        this.Controls.Add(this.headerPanel);
        this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.Name = "AbstractEditor";
        this.Closing += AbstractEditor_Closing;
        this.toolPanel.ResumeLayout(false);
        this.toolPanel.PerformLayout();
        this.toolStrip1.ResumeLayout(false);
        this.toolStrip1.PerformLayout();
        this.headerPanel.ResumeLayout(false);
        this.headerPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.elementPicture)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
	}

	public AbstractEditor()
	{
		InitializeComponent();
        // show only vertical scrollbar
        toolPanel.HorizontalScroll.Maximum = 0;
        toolPanel.VerticalScroll.Visible = false;
        toolPanel.AutoScroll = true;
        toolPanel.Hide();
        _saveCmd.Click += SaveCmd_Click;
	    dockCmd.Click += DockCmd_Click;
	    LoadSettings();
	}
	private void LoadSettings()
	{
        if (ConfigurationManager.GetActiveConfiguration() != null)
        {
            showMenusInAppToolStrip = ConfigurationManager
                .GetActiveConfiguration()
                .ShowEditorMenusInAppToolStrip;
        }
    }
	public override string TitleName
    {
        get
        {
            return base.TitleName;
        }
        set
        {
            base.TitleName = value;
            lblName.Text = TitleName;
        }
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (IsDialog())
        {
            if (keyData == (Keys.S | Keys.Control))
            {
                SaveCommand();
                return true;
            }
            else if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
    public ISubmenuBuilder ActionsBuilder
    {
        get
        {
            return _actionsBuilder;
        }
        set
        {
            _actionsBuilder = value;
        }
    }
    public ISubmenuBuilder NewElementsBuilder
    {
        get
        {
            return _newElementsBuilder;
        }
        set
        {
            _newElementsBuilder = value;
            RebuildActionsPane();
        }
    }
    private void RebuildActionsPane()
    {
        int width = toolPanel.Width;
        toolStrip1.AutoSize = false;
        toolPanel.AutoSize = false;
        toolPanel.Width = width;
        toolStrip1.Items.Clear();
        toolStrip1.Top = 0;
        // Actions
        toolStrip1.Items.Add(actionsLabel);
		if (!IsDialog() && (ActionsBuilder == null || NewElementsBuilder == null))
        {
            toolPanel.Hide();
            return;
        }
        else if (IsDirty && ! IsDialog())
        {
            toolPanel.Hide();
            return;
        }
        else if (IsDirty && IsDialog())
        {
            toolPanel.Show();
            toolStrip1.Items.Add(_saveCmd);
        }
        else if (!IsDialog() && !showMenusInAppToolStrip)
        {
            var actions = this.ActionsBuilder.BuildSubmenu(this.Content);           
            var newItems = this.NewElementsBuilder.BuildSubmenu(this.Content);
            if (actions.Length != 0 || newItems.Length != 0)
            {
                toolPanel.Show();
            }
            if (actions.Length == 0)
            {
                toolStrip1.Items.Remove(actionsLabel);
            }
            else
            {
                toolStrip1.Items.AddRange(actions);
            }
            // New
            if (newItems.Length != 0)
            {
                toolStrip1.Items.Add(newElementsLabel);
                toolStrip1.Items.AddRange(newItems);
            }
            // refresh menus
            foreach (var item in toolStrip1.Items)
            {
                AsMenuCommand cmd = item as AsMenuCommand;
                if (cmd != null)
                {
                    cmd.UpdateItemsToDisplay();
                }
            }
            toolPanel.AutoSize = true;
            toolStrip1.AutoSize = true;
        }
		if (IsDialog()){
			toolStrip1.Items.Add(dockCmd);
			toolPanel.Show();
		}
		if (!IsDialog() && 
		    toolStrip1.Items.Count == 1 &&
		    toolStrip1.Items.Contains(actionsLabel))
		{
			toolPanel.Hide();
		}
		toolPanel.BackColor = toolStrip1.BackColor;
    }
    private void DockCmd_Click(object sender, EventArgs e)
    {
        AbstractEditor newEditor = (AbstractEditor)Activator.CreateInstance(this.GetType(), new object[] { false });
        newEditor.LoadObject(ModelContent);
        newEditor.TitleName = ModelContent.Name;
        WorkbenchSingleton.Workbench.ShowView(newEditor);
        this.Closing -= AbstractEditor_Closing;
        Close();
    }
    private void SaveCmd_Click(object sender, EventArgs e)
    {
        SaveCommand();
    }
    private void SaveCommand()
    {
        SaveObject();
        IsDirty = false;
        if (IsDialog())
        {
            this.Close();
        }
    }
    public bool IsDialog()
    {
        return ParentForm == null;
    }
	protected virtual void OnContentLoaded(EventArgs e)
	{
		if (ContentLoaded != null) 
		{
			ContentLoaded(this, e);
		}
        RebuildActionsPane();
	}
    protected override void OnDirtyChanged(EventArgs e)
    {
        base.OnDirtyChanged(e);
        RebuildActionsPane();
        ToolStripsNeedUpdate?.Invoke(this, EventArgs.Empty);
    }
    protected override void ViewSpecificLoad(object objectToLoad)
	{
        AbstractSchemaItem schemaItem = objectToLoad as AbstractSchemaItem;
        if (schemaItem != null)
		{
            SchemaItemDescriptionAttribute attr = schemaItem.GetType().SchemaItemDescription();
            string type = schemaItem.GetType().Name;
            if (attr != null)
            {
                type = attr.Name;
            }
            lblType.Text = type + ":";
            lblName.Text = schemaItem.Name;
            var schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(IBrowserPad)) as IBrowserPad;
            var imageList = schemaBrowser.ImageList;
            elementPicture.Image = imageList.Images[schemaBrowser.ImageIndex(schemaItem.Icon)];
            this.ModelContent = schemaItem;
			OnContentLoaded(EventArgs.Empty);
		}
		else
		{
			throw new InvalidCastException(Origam.Workbench.BaseComponents.ResourceUtils.GetString("ErrorISchemaItemOnly"));
		}
	}
    public override void SaveObject()
    {
        try
        {
            ModelContent.PersistenceProvider.BeginTransaction();
            if (ModelContent.OldPrimaryKey == null)
            {
                ModelContent.Persist();
            }
            else
            {
                // this is a copy
                // first we copy all original-new key pairs
                Hashtable oldKeys = new Hashtable();
                foreach (AbstractSchemaItem child in ModelContent.ChildItemsRecursive)
                {
                    if (child.OldPrimaryKey != null)
                    {
                        oldKeys.Add(child.PrimaryKey, child.OldPrimaryKey);
                    }
                }
                // we persist with "wrong" references
                // to the original model elements
                ModelContent.ThrowEventOnPersist = false;
                ModelContent.Persist();
                // then we return the old keys to the persisted elements
                // since they lost them while being persisted
                foreach (AbstractSchemaItem child in ModelContent.ChildItemsRecursive)
                {
                    if (oldKeys.Contains(child.PrimaryKey))
                    {
                        child.OldPrimaryKey = (ModelElementKey)oldKeys[child.PrimaryKey];
                    }
                }
                // then we update the references to the new elements
                ModelContent.UpdateReferences();
                // and persist again
                ModelContent.ThrowEventOnPersist = true;
                ModelContent.Persist();
                ArrayList items = ModelContent.ChildItemsRecursive;
                items.Add(ModelContent);
                _documentation.CloneDocumentation(items.ToList<ISchemaItem>());
                ModelContent.OldPrimaryKey = null;
            }
        }
        finally
        {
            ModelContent.PersistenceProvider.EndTransaction();
        }
        this.DialogResult = DialogResult.OK;
    }
    public override string HelpTopic
    {
        get
        {
            if (ModelContent != null)
            {
                HelpTopicAttribute topic = Help(ModelContent.GetType());
                if(topic != null)
                {
                    return topic.Topic;
                }
            }
            return "";
        }
    }
    private HelpTopicAttribute Help(Type type)
    {
        object[] attributes = type.GetCustomAttributes(typeof(HelpTopicAttribute), true);
        if (attributes != null && attributes.Length > 0)
            return attributes[0] as HelpTopicAttribute;
        else
            return null;
    }
	private void AbstractEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
	{
		if(IsDirty)
		{
			DialogResult result = MessageBox.Show(
				Origam.Workbench.BaseComponents.ResourceUtils.GetString("DoYouWantSave", this.TitleName), 
				Origam.Workbench.BaseComponents.ResourceUtils.GetString("SaveTitle"), 
				MessageBoxButtons.YesNoCancel, 
				MessageBoxIcon.Question);
		
			switch(result)
			{
				case DialogResult.Yes:
					try
					{
						SaveObject();
					}
					catch(Exception ex)
					{
						Origam.UI.AsMessageBox.ShowError(WorkbenchSingleton.Workbench as Form, ex.Message, "Error", ex);
						e.Cancel = true;
					}
					break;
				case DialogResult.No:
					if(ModelContent.IsPersisted)
					// existing item
					{
						// in case we edited some of the children we invalidate their cache
						ModelContent.InvalidateChildrenPersistenceCache();
						ModelContent.ClearCache();
					}
					else
					// new item
					{
						// we have to remove ours object from the colection
						ISchemaItemProvider provider = ModelContent.ParentItem == null ? ModelContent.RootProvider : ModelContent.ParentItem as ISchemaItemProvider;
						if(provider == null)
						{
							System.Diagnostics.Debug.Fail("Both ParentItem and RootProvider not specified");
						}
						else
						{
							if(provider.ChildItems.Contains(ModelContent))
							{
								provider.ChildItems.Remove(ModelContent);
							}
						}								
					}
					break;
		
				case DialogResult.Cancel:
					e.Cancel = true;
					break;
			}
		}
	}
    public virtual List<ToolStrip> GetToolStrips(int maxWidth =-1) {
	    if (!showMenusInAppToolStrip) return new List<ToolStrip>();
		var actions = ActionsBuilder.BuildSubmenu(Content);
		var actionToolStrip = MakeLabeledToolStrip(actions, "Actions",maxWidth/2);
        var newItems = NewElementsBuilder.BuildSubmenu(Content);
		var newToolStrip = MakeLabeledToolStrip(newItems, "New", maxWidth/2);
		return new List<ToolStrip> {actionToolStrip, newToolStrip};			        
    }
    protected ToolStrip MakeLabeledToolStrip(ToolStripMenuItem[] items,
        string toolStripName, int maxWidth)
    {
	    BigToolStripButton[] toolStripButtons = items
            .Select(item =>
            {
                BigToolStripButton button = new BigToolStripButton();
                button.Text = item.Text;
                button.Image = item.Image ?? ImageRes.UnknownIcon;
                button.Click += (sender, args) => item.PerformClick();
                button.Enabled = !IsDirty;
	            item.Enabled = !IsDirty;
                return button;
            }).ToArray();
        LabeledToolStrip toolStrip = new LabeledToolStrip(this);
        toolStrip.Text = toolStripName;
        toolStrip.Items.AddRange(toolStripButtons);
        HideItemsToFitToMaxWidth(toolStrip, items, maxWidth);
        return toolStrip;
    }
    private static void HideItemsToFitToMaxWidth(ToolStrip toolStrip,
        ToolStripMenuItem[] items, int maxWidth)
    {
        var itemsToHide = new List<ToolStripItem>();
        var dropDownButton = new BigArrowToolStripDropDownButton();
        for (int i = 0; i < 200; i++)
        {
            if (itemsToHide.Count == items.Length - 1) break;
            int totalToolTipWidth = itemsToHide.Count == 0
                ? toolStrip.PreferredSize.Width
                : toolStrip.PreferredSize.Width + dropDownButton.Width;
            if (totalToolTipWidth < maxWidth) break;
            int indexToMove = toolStrip.Items.Count - 1;
            if (indexToMove > -1)
            {
                var lastItem = toolStrip.Items[indexToMove];
                toolStrip.Items.Remove(lastItem);
                itemsToHide.Add(items[indexToMove]);
            }
        }
        toolStrip.Width = 10000;
        itemsToHide.Reverse();
        dropDownButton.DropDownItems.AddRange(itemsToHide.ToArray());
        if (dropDownButton.HasDropDownItems)
        {
            toolStrip.Items.Add(dropDownButton);
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
    public event EventHandler ToolStripsNeedUpdate;
}
