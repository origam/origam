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
using Origam.Gui;
using Origam.Gui.UI;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Services;
using Type = System.Type;

namespace Origam.Workbench.Editors;

public class AbstractEditor : AbstractViewContent, IToolStripContainer
{
    public event EventHandler ContentLoaded;
    IDocumentationService _documentation =
        ServiceManager.Services.GetService(serviceType: typeof(IDocumentationService))
        as IDocumentationService;
    private Panel toolPanel;
    private ToolStrip toolStrip1;
    private ToolStripLabel actionsLabel;
    private ToolStripLabel newElementsLabel;
    private Panel headerPanel;
    private Label lblName;
    private PictureBox elementPicture;
    private Label lblType;
    ToolStripMenuItem _saveCmd = new ToolStripMenuItem(text: "Save", image: ImageRes.Save_16x);
    ToolStripMenuItem dockCmd = new ToolStripMenuItem(text: "Dock", image: ImageRes.dock);
    private ISubmenuBuilder _actionsBuilder = null;
    private ISubmenuBuilder _newElementsBuilder = null;
    protected bool showMenusInAppToolStrip = false;
    public override object Content { get; set; }
    public ISchemaItem ModelContent
    {
        get { return Content as ISchemaItem; }
        set { Content = value; }
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
        this.toolPanel.Controls.Add(value: this.toolStrip1);
        this.toolPanel.Dock = System.Windows.Forms.DockStyle.Right;
        this.toolPanel.Location = new System.Drawing.Point(x: 635, y: 40);
        this.toolPanel.Name = "toolPanel";
        this.toolPanel.Size = new System.Drawing.Size(width: 79, height: 521);
        this.toolPanel.TabIndex = 2;
        //
        // toolStrip1
        //
        this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
        this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
        this.toolStrip1.Items.AddRange(
            toolStripItems: new System.Windows.Forms.ToolStripItem[]
            {
                this.actionsLabel,
                this.newElementsLabel,
            }
        );
        this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
        this.toolStrip1.Location = new System.Drawing.Point(x: 0, y: 0);
        this.toolStrip1.Name = "toolStrip1";
        this.toolStrip1.Padding = new System.Windows.Forms.Padding(all: 10);
        this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
        this.toolStrip1.Size = new System.Drawing.Size(width: 79, height: 90);
        this.toolStrip1.Stretch = true;
        this.toolStrip1.TabIndex = 2;
        this.toolStrip1.Text = "toolStrip1";
        //
        // actionsLabel
        //
        this.actionsLabel.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 9.75F,
            style: System.Drawing.FontStyle.Bold,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.actionsLabel.Name = "actionsLabel";
        this.actionsLabel.Padding = new System.Windows.Forms.Padding(
            left: 0,
            top: 16,
            right: 0,
            bottom: 0
        );
        this.actionsLabel.Size = new System.Drawing.Size(width: 59, height: 32);
        this.actionsLabel.Text = "Actions";
        //
        // newElementsLabel
        //
        this.newElementsLabel.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 9.75F,
            style: System.Drawing.FontStyle.Bold,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.newElementsLabel.Name = "newElementsLabel";
        this.newElementsLabel.Padding = new System.Windows.Forms.Padding(
            left: 0,
            top: 16,
            right: 0,
            bottom: 0
        );
        this.newElementsLabel.Size = new System.Drawing.Size(width: 38, height: 32);
        this.newElementsLabel.Text = "New";
        //
        // headerPanel
        //
        this.headerPanel.Controls.Add(value: this.lblName);
        this.headerPanel.Controls.Add(value: this.lblType);
        this.headerPanel.Controls.Add(value: this.elementPicture);
        this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.headerPanel.Location = new System.Drawing.Point(x: 0, y: 0);
        this.headerPanel.Name = "headerPanel";
        this.headerPanel.Size = new System.Drawing.Size(width: 714, height: 40);
        this.headerPanel.TabIndex = 0;
        //
        // lblName
        //
        this.lblName.AutoSize = true;
        this.lblName.Dock = System.Windows.Forms.DockStyle.Left;
        this.lblName.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 12F,
            style: System.Drawing.FontStyle.Bold,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.lblName.Location = new System.Drawing.Point(x: 74, y: 0);
        this.lblName.Name = "lblName";
        this.lblName.Padding = new System.Windows.Forms.Padding(
            left: 0,
            top: 10,
            right: 0,
            bottom: 0
        );
        this.lblName.Size = new System.Drawing.Size(width: 57, height: 30);
        this.lblName.TabIndex = 1;
        this.lblName.Text = "label1";
        this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblType
        //
        this.lblType.AutoSize = true;
        this.lblType.BackColor = System.Drawing.Color.Transparent;
        this.lblType.Dock = System.Windows.Forms.DockStyle.Left;
        this.lblType.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 12F,
            style: System.Drawing.FontStyle.Regular,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.lblType.Location = new System.Drawing.Point(x: 16, y: 0);
        this.lblType.Name = "lblType";
        this.lblType.Padding = new System.Windows.Forms.Padding(
            left: 0,
            top: 10,
            right: 0,
            bottom: 0
        );
        this.lblType.Size = new System.Drawing.Size(width: 58, height: 30);
        this.lblType.TabIndex = 1;
        this.lblType.Text = "lblType";
        this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // elementPicture
        //
        this.elementPicture.Dock = System.Windows.Forms.DockStyle.Left;
        this.elementPicture.Location = new System.Drawing.Point(x: 0, y: 0);
        this.elementPicture.Name = "elementPicture";
        this.elementPicture.Padding = new System.Windows.Forms.Padding(
            left: 12,
            top: 12,
            right: 12,
            bottom: 16
        );
        this.elementPicture.Size = new System.Drawing.Size(width: 16, height: 40);
        this.elementPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
        this.elementPicture.TabIndex = 0;
        this.elementPicture.TabStop = false;
        //
        // AbstractEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(width: 5, height: 12);
        this.ClientSize = new System.Drawing.Size(width: 714, height: 561);
        this.Controls.Add(value: this.toolPanel);
        this.Controls.Add(value: this.headerPanel);
        this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
        this.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 7.875F,
            style: System.Drawing.FontStyle.Regular,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.Name = "AbstractEditor";
        this.Closing += AbstractEditor_Closing;
        this.toolPanel.ResumeLayout(performLayout: false);
        this.toolPanel.PerformLayout();
        this.toolStrip1.ResumeLayout(performLayout: false);
        this.toolStrip1.PerformLayout();
        this.headerPanel.ResumeLayout(performLayout: false);
        this.headerPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.elementPicture)).EndInit();
        this.ResumeLayout(performLayout: false);
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
        get { return base.TitleName; }
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

            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }

    public ISubmenuBuilder ActionsBuilder
    {
        get { return _actionsBuilder; }
        set { _actionsBuilder = value; }
    }
    public ISubmenuBuilder NewElementsBuilder
    {
        get { return _newElementsBuilder; }
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
        toolStrip1.Items.Add(value: actionsLabel);
        if (!IsDialog() && (ActionsBuilder == null || NewElementsBuilder == null))
        {
            toolPanel.Hide();
            return;
        }

        if (IsDirty && !IsDialog())
        {
            toolPanel.Hide();
            return;
        }

        if (IsDirty && IsDialog())
        {
            toolPanel.Show();
            toolStrip1.Items.Add(value: _saveCmd);
        }
        else if (!IsDialog() && !showMenusInAppToolStrip)
        {
            var actions = this.ActionsBuilder.BuildSubmenu(owner: this.Content);
            var newItems = this.NewElementsBuilder.BuildSubmenu(owner: this.Content);
            if (actions.Length != 0 || newItems.Length != 0)
            {
                toolPanel.Show();
            }
            if (actions.Length == 0)
            {
                toolStrip1.Items.Remove(value: actionsLabel);
            }
            else
            {
                toolStrip1.Items.AddRange(toolStripItems: actions);
            }
            // New
            if (newItems.Length != 0)
            {
                toolStrip1.Items.Add(value: newElementsLabel);
                toolStrip1.Items.AddRange(toolStripItems: newItems);
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
        if (IsDialog())
        {
            toolStrip1.Items.Add(value: dockCmd);
            toolPanel.Show();
        }
        if (
            !IsDialog()
            && toolStrip1.Items.Count == 1
            && toolStrip1.Items.Contains(value: actionsLabel)
        )
        {
            toolPanel.Hide();
        }
        toolPanel.BackColor = toolStrip1.BackColor;
    }

    private void DockCmd_Click(object sender, EventArgs e)
    {
        AbstractEditor newEditor = (AbstractEditor)
            Activator.CreateInstance(type: this.GetType(), args: new object[] { false });
        newEditor.LoadObject(objectToLoad: ModelContent);
        newEditor.TitleName = ModelContent.Name;
        WorkbenchSingleton.Workbench.ShowView(content: newEditor);
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
            ContentLoaded(sender: this, e: e);
        }
        RebuildActionsPane();
    }

    protected override void OnDirtyChanged(EventArgs e)
    {
        base.OnDirtyChanged(e: e);
        RebuildActionsPane();
        ToolStripsNeedUpdate?.Invoke(sender: this, e: EventArgs.Empty);
    }

    protected override void ViewSpecificLoad(object objectToLoad)
    {
        ISchemaItem schemaItem = objectToLoad as ISchemaItem;
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
            var schemaBrowser =
                WorkbenchSingleton.Workbench.GetPad(type: typeof(IBrowserPad)) as IBrowserPad;
            var imageList = schemaBrowser.ImageList;
            elementPicture.Image = imageList.Images[
                index: schemaBrowser.ImageIndex(icon: schemaItem.Icon)
            ];
            this.ModelContent = schemaItem;
            OnContentLoaded(e: EventArgs.Empty);
        }
        else
        {
            throw new InvalidCastException(
                message: Origam.Workbench.BaseComponents.ResourceUtils.GetString(
                    key: "ErrorISchemaItemOnly"
                )
            );
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
                foreach (ISchemaItem child in ModelContent.ChildItemsRecursive)
                {
                    if (child.OldPrimaryKey != null)
                    {
                        oldKeys.Add(key: child.PrimaryKey, value: child.OldPrimaryKey);
                    }
                }
                // we persist with "wrong" references
                // to the original model elements
                ModelContent.ThrowEventOnPersist = false;
                ModelContent.Persist();
                // then we return the old keys to the persisted elements
                // since they lost them while being persisted
                foreach (ISchemaItem child in ModelContent.ChildItemsRecursive)
                {
                    if (oldKeys.Contains(key: child.PrimaryKey))
                    {
                        child.OldPrimaryKey = (ModelElementKey)oldKeys[key: child.PrimaryKey];
                    }
                }
                // then we update the references to the new elements
                ModelContent.UpdateReferences();
                // and persist again
                ModelContent.ThrowEventOnPersist = true;
                ModelContent.Persist();
                List<ISchemaItem> items = ModelContent.ChildItemsRecursive;
                items.Add(item: ModelContent);
                _documentation.CloneDocumentation(clonedSchemaItems: items);
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
                HelpTopicAttribute topic = Help(type: ModelContent.GetType());
                if (topic != null)
                {
                    return topic.Topic;
                }
            }
            return "";
        }
    }

    private HelpTopicAttribute Help(Type type)
    {
        object[] attributes = type.GetCustomAttributes(
            attributeType: typeof(HelpTopicAttribute),
            inherit: true
        );
        if (attributes != null && attributes.Length > 0)
        {
            return attributes[0] as HelpTopicAttribute;
        }

        return null;
    }

    private void AbstractEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (IsDirty)
        {
            DialogResult result = MessageBox.Show(
                text: Origam.Workbench.BaseComponents.ResourceUtils.GetString(
                    key: "DoYouWantSave",
                    args: this.TitleName
                ),
                caption: Origam.Workbench.BaseComponents.ResourceUtils.GetString(key: "SaveTitle"),
                buttons: MessageBoxButtons.YesNoCancel,
                icon: MessageBoxIcon.Question
            );

            switch (result)
            {
                case DialogResult.Yes:
                {
                    try
                    {
                        SaveObject();
                    }
                    catch (Exception ex)
                    {
                        Origam.UI.AsMessageBox.ShowError(
                            owner: WorkbenchSingleton.Workbench as Form,
                            text: ex.Message,
                            caption: "Error",
                            exception: ex
                        );
                        e.Cancel = true;
                    }
                    break;
                }

                case DialogResult.No:
                {
                    if (ModelContent.IsPersisted)
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
                        ISchemaItemProvider provider =
                            ModelContent.ParentItem == null
                                ? ModelContent.RootProvider
                                : ModelContent.ParentItem as ISchemaItemProvider;
                        if (provider == null)
                        {
                            System.Diagnostics.Debug.Fail(
                                message: "Both ParentItem and RootProvider not specified"
                            );
                        }
                        else
                        {
                            if (provider.ChildItems.Contains(item: ModelContent))
                            {
                                provider.ChildItems.Remove(item: ModelContent);
                            }
                        }
                    }
                    break;
                }

                case DialogResult.Cancel:
                {
                    e.Cancel = true;
                    break;
                }
            }
        }
    }

    public virtual List<ToolStrip> GetToolStrips(int maxWidth = -1)
    {
        if (!showMenusInAppToolStrip)
        {
            return new List<ToolStrip>();
        }

        var actions = ActionsBuilder.BuildSubmenu(owner: Content);
        var actionToolStrip = MakeLabeledToolStrip(
            items: actions,
            toolStripName: "Actions",
            maxWidth: maxWidth / 2
        );
        var newItems = NewElementsBuilder.BuildSubmenu(owner: Content);
        var newToolStrip = MakeLabeledToolStrip(
            items: newItems,
            toolStripName: "New",
            maxWidth: maxWidth / 2
        );
        return new List<ToolStrip> { actionToolStrip, newToolStrip };
    }

    protected ToolStrip MakeLabeledToolStrip(
        ToolStripMenuItem[] items,
        string toolStripName,
        int maxWidth
    )
    {
        BigToolStripButton[] toolStripButtons = items
            .Select(selector: item =>
            {
                BigToolStripButton button = new BigToolStripButton();
                button.Text = item.Text;
                button.Image = item.Image ?? ImageRes.UnknownIcon;
                button.Click += (sender, args) => item.PerformClick();
                button.Enabled = !IsDirty;
                item.Enabled = !IsDirty;
                return button;
            })
            .ToArray();
        LabeledToolStrip toolStrip = new LabeledToolStrip(owner: this);
        toolStrip.Text = toolStripName;
        toolStrip.Items.AddRange(toolStripItems: toolStripButtons);
        HideItemsToFitToMaxWidth(toolStrip: toolStrip, items: items, maxWidth: maxWidth);
        return toolStrip;
    }

    private static void HideItemsToFitToMaxWidth(
        ToolStrip toolStrip,
        ToolStripMenuItem[] items,
        int maxWidth
    )
    {
        var itemsToHide = new List<ToolStripItem>();
        var dropDownButton = new BigArrowToolStripDropDownButton();
        for (int i = 0; i < 200; i++)
        {
            if (itemsToHide.Count == items.Length - 1)
            {
                break;
            }

            int totalToolTipWidth =
                itemsToHide.Count == 0
                    ? toolStrip.PreferredSize.Width
                    : toolStrip.PreferredSize.Width + dropDownButton.Width;
            if (totalToolTipWidth < maxWidth)
            {
                break;
            }

            int indexToMove = toolStrip.Items.Count - 1;
            if (indexToMove > -1)
            {
                var lastItem = toolStrip.Items[index: indexToMove];
                toolStrip.Items.Remove(value: lastItem);
                itemsToHide.Add(item: items[indexToMove]);
            }
        }
        toolStrip.Width = 10000;
        itemsToHide.Reverse();
        dropDownButton.DropDownItems.AddRange(toolStripItems: itemsToHide.ToArray());
        if (dropDownButton.HasDropDownItems)
        {
            toolStrip.Items.Add(value: dropDownButton);
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
