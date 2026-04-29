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
using Origam.Windows.Editor;
using Origam.Workbench.Editors;

namespace Origam.Schema.EntityModel.UI;

/// <summary>
/// Summary description for XslTransformationEditor.
/// </summary>
public class XsdEditor : AbstractEditor
{
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Label lblXsdName;
    private System.Windows.Forms.Label lblText;
    private Origam.Windows.Editor.XmlEditor txtText;
    private Panel panel1;
    private bool _isEditing = false;

    public XsdEditor()
    {
        InitializeComponent();

        this.ContentLoaded += new EventHandler(XsdEditor_ContentLoaded);
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
    }

    #region Overriden AbstractViewContent Members
    public override void SaveObject()
    {
        (Content as XsdDataStructure).Xsd = txtText.Text;
        base.SaveObject();
    }
    #endregion
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(t: typeof(XsdEditor));
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblXsdName = new System.Windows.Forms.Label();
        this.lblText = new System.Windows.Forms.Label();
        this.txtText = new XmlEditor();
        this.panel1 = new System.Windows.Forms.Panel();
        this.panel1.SuspendLayout();
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
        this.txtName.Location = new System.Drawing.Point(x: 136, y: 10);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(width: 586, height: 20);
        this.txtName.TabIndex = 0;
        this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
        //
        // lblXsdName
        //
        this.lblXsdName.Location = new System.Drawing.Point(x: 16, y: 10);
        this.lblXsdName.Name = "lblXsdName";
        this.lblXsdName.Size = new System.Drawing.Size(width: 112, height: 16);
        this.lblXsdName.TabIndex = 2;
        this.lblXsdName.Text = "Name";
        //
        // lblText
        //
        this.lblText.Location = new System.Drawing.Point(x: 16, y: 34);
        this.lblText.Name = "lblText";
        this.lblText.Size = new System.Drawing.Size(width: 120, height: 16);
        this.lblText.TabIndex = 3;
        this.lblText.Text = "Xsd:";
        //
        // txtText
        //
        this.txtText.Anchor = (
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
        this.txtText.Location = new System.Drawing.Point(x: 0, y: 53);
        this.txtText.Name = "txtText";
        this.txtText.Size = new System.Drawing.Size(width: 722, height: 449);
        this.txtText.TabIndex = 5;
        //
        // panel1
        //
        this.panel1.Controls.Add(value: this.lblXsdName);
        this.panel1.Controls.Add(value: this.txtText);
        this.panel1.Controls.Add(value: this.lblText);
        this.panel1.Controls.Add(value: this.txtName);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel1.Location = new System.Drawing.Point(x: 0, y: 40);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(width: 728, height: 502);
        this.panel1.TabIndex = 6;
        //
        // XsdEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(width: 5, height: 13);
        this.ClientSize = new System.Drawing.Size(width: 807, height: 542);
        this.Controls.Add(value: this.panel1);
        this.Font = new System.Drawing.Font(
            familyName: "Microsoft Sans Serif",
            emSize: 8.25F,
            style: System.Drawing.FontStyle.Regular,
            unit: System.Drawing.GraphicsUnit.Point,
            gdiCharSet: ((byte)(238))
        );
        this.Name = "XsdEditor";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Controls.SetChildIndex(child: this.panel1, newIndex: 0);
        this.panel1.ResumeLayout(performLayout: false);
        this.panel1.PerformLayout();
        this.ResumeLayout(performLayout: false);
        this.PerformLayout();
    }

    private void txtName_TextChanged(object sender, System.EventArgs e)
    {
        if (ModelContent != null & !this.IsViewOnly)
        {
            if (!_isEditing)
            {
                ModelContent.Name = txtName.Text;
                this.IsDirty = true;
                this.TitleName = ModelContent.Name;
            }
        }
    }

    private void TextArea_KeyDown(object sender, KeyEventArgs e)
    {
        if (!this.IsViewOnly)
        {
            this.IsDirty = true;
        }
    }

    private void XsdEditor_ContentLoaded(object sender, EventArgs e)
    {
        if (this.IsViewOnly)
        {
            txtText.Enabled = false;
            txtName.Enabled = false;
        }

        if (ModelContent is XsdDataStructure)
        {
            XsdDataStructure _xsdDs = ModelContent as XsdDataStructure;
            _isEditing = true;
            txtName.Text = _xsdDs.Name;
            txtText.Text = _xsdDs.Xsd;
            _isEditing = false;
        }
        else
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorXsdDataStructureOnly")
            );
        }
    }
}
