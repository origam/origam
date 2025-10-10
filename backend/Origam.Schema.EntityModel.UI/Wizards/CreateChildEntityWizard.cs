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

using System.Windows.Forms;

namespace Origam.Schema.EntityModel.Wizards;

/// <summary>
/// Summary description for CreateNtoNEntityWizard.
/// </summary>
public class CreateChildEntityWizard : System.Windows.Forms.Form
{
    private System.Windows.Forms.Label lblName;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.Label lblEntity1;
    private System.Windows.Forms.ComboBox cboEntity1;
    private System.Windows.Forms.Label lblEntity2;
    private System.Windows.Forms.ComboBox cboEntity2;
    private System.Windows.Forms.Label label1;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public CreateChildEntityWizard()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        //
        // TODO: Add any constructor code after InitializeComponent call
        //
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
        this.lblName = new System.Windows.Forms.Label();
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblEntity1 = new System.Windows.Forms.Label();
        this.cboEntity1 = new System.Windows.Forms.ComboBox();
        this.lblEntity2 = new System.Windows.Forms.Label();
        this.cboEntity2 = new System.Windows.Forms.ComboBox();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnOK = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.SuspendLayout();
        //
        // lblName
        //
        this.lblName.Location = new System.Drawing.Point(8, 16);
        this.lblName.Name = "lblName";
        this.lblName.Size = new System.Drawing.Size(104, 16);
        this.lblName.TabIndex = 3;
        this.lblName.Text = ResourceUtils.GetString("ChildEntityNameLabel");
        //
        // txtName
        //
        this.txtName.Location = new System.Drawing.Point(120, 16);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(248, 20);
        this.txtName.TabIndex = 3;
        this.txtName.Text = "";
        //
        // lblEntity1
        //
        this.lblEntity1.Location = new System.Drawing.Point(8, 40);
        this.lblEntity1.Name = "lblEntity1";
        this.lblEntity1.Size = new System.Drawing.Size(96, 16);
        this.lblEntity1.TabIndex = 7;
        this.lblEntity1.Text = ResourceUtils.GetString("MasterEntityLabel");
        //
        // cboEntity1
        //
        this.cboEntity1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboEntity1.Location = new System.Drawing.Point(120, 40);
        this.cboEntity1.Name = "cboEntity1";
        this.cboEntity1.Size = new System.Drawing.Size(248, 21);
        this.cboEntity1.Sorted = true;
        this.cboEntity1.TabIndex = 4;
        this.cboEntity1.SelectedIndexChanged += new System.EventHandler(
            this.cboEntity1_SelectedIndexChanged
        );
        //
        // lblEntity2
        //
        this.lblEntity2.Location = new System.Drawing.Point(8, 96);
        this.lblEntity2.Name = "lblEntity2";
        this.lblEntity2.Size = new System.Drawing.Size(96, 16);
        this.lblEntity2.TabIndex = 9;
        this.lblEntity2.Text = ResourceUtils.GetString("ForeignEntityLabel");
        //
        // cboEntity2
        //
        this.cboEntity2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboEntity2.Location = new System.Drawing.Point(120, 96);
        this.cboEntity2.Name = "cboEntity2";
        this.cboEntity2.Size = new System.Drawing.Size(248, 21);
        this.cboEntity2.Sorted = true;
        this.cboEntity2.TabIndex = 0;
        this.cboEntity2.SelectedIndexChanged += new System.EventHandler(
            this.cboEntity2_SelectedIndexChanged
        );
        //
        // btnCancel
        //
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnCancel.Location = new System.Drawing.Point(264, 136);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(96, 24);
        this.btnCancel.TabIndex = 2;
        this.btnCancel.Text = ResourceUtils.GetString("ButtonCancel");
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        //
        // btnOK
        //
        this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.btnOK.Location = new System.Drawing.Point(160, 136);
        this.btnOK.Name = "btnOK";
        this.btnOK.Size = new System.Drawing.Size(96, 24);
        this.btnOK.TabIndex = 1;
        this.btnOK.Text = ResourceUtils.GetString("ButtonOK");
        this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
        //
        // label1
        //
        this.label1.Location = new System.Drawing.Point(8, 72);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(360, 16);
        this.label1.TabIndex = 10;
        this.label1.Text = ResourceUtils.GetString("OptionalForeignEntity");
        //
        // CreateChildEntityWizard
        //
        this.AcceptButton = this.btnOK;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(376, 167);
        this.ControlBox = false;
        this.Controls.Add(this.label1);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnOK);
        this.Controls.Add(this.lblEntity2);
        this.Controls.Add(this.cboEntity2);
        this.Controls.Add(this.lblEntity1);
        this.Controls.Add(this.cboEntity1);
        this.Controls.Add(this.lblName);
        this.Controls.Add(this.txtName);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        this.Name = "CreateChildEntityWizard";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = ResourceUtils.GetString("CreateNNEntityWiz");
        this.ResumeLayout(false);
    }
    #endregion
    #region Event Handlers
    private void btnOK_Click(object sender, System.EventArgs e)
    {
        if (this.txtName.Text == "" | this.Entity1 == null)
        {
            MessageBox.Show(
                ResourceUtils.GetString("EnterAllInfo"),
                ResourceUtils.GetString("ChildEntityWiz"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Asterisk
            );
            return;
        }
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private void btnCancel_Click(object sender, System.EventArgs e)
    {
        this.Close();
    }

    private void cboEntity1_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        RefreshName();
    }

    private void cboEntity2_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        RefreshName();
    }
    #endregion
    #region Public Properties
    private IDataEntity _entity;
    public IDataEntity Entity1
    {
        get { return _entity; }
        set
        {
            _entity = value;
            SetUpForm();
        }
    }
    public IDataEntity Entity2
    {
        get { return cboEntity2.SelectedItem as IDataEntity; }
    }
    public string EntityName
    {
        get { return txtName.Text; }
    }
    #endregion

    #region Private Methods
    private void SetUpForm()
    {
        this.txtName.Text = "";
        cboEntity1.Items.Clear();
        cboEntity2.Items.Clear();
        if (this.Entity1 == null)
            return;
        object selectedItem = null;
        foreach (IDataEntity entity in this.Entity1.RootProvider.ChildItems)
        {
            cboEntity1.Items.Add(entity);
            cboEntity2.Items.Add(entity);
            if (entity.PrimaryKey.Equals(this.Entity1.PrimaryKey))
            {
                selectedItem = entity;
            }
        }
        cboEntity1.SelectedItem = selectedItem;
    }

    private void RefreshName()
    {
        if (cboEntity1.SelectedItem != null & cboEntity2.SelectedItem != null)
        {
            txtName.Text = this.Entity1.Name + this.Entity2.Name;
        }
    }
    #endregion
}
