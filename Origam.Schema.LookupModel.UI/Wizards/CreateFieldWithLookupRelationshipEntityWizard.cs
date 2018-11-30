#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using Origam.Schema.EntityModel;

namespace Origam.Schema.LookupModel.Wizards
{
	/// <summary>
	/// Summary description for CreateLookupFromEntityWizard.
	/// </summary>
	public class CreateFieldWithLookupRelationshipEntityWizard : System.Windows.Forms.Form
	{
        public class InitialValue
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public bool IsDefault { get; set; }
        }

		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblCaption;
        private BindingList<InitialValue> _initialValues = new BindingList<InitialValue>();
        private DataGridViewTextBoxColumn colCode;
       // private DataGridViewTextBoxColumn colName;
        private CheckedListBox checkedRMisc;
        private GroupBox groupBoxKey;
        private GroupBox groupBox2;
        private Label label3;
        private Label label2;
        private TextBox txtKeyName;
        private Label label1;
        private ComboBox RelatedEntityFiled;
        private ComboBox BaseEntityField;
        private ComboBox tableRelation;
        private Label label4;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

		public CreateFieldWithLookupRelationshipEntityWizard()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            //grdInitialValues.DataSource = _initialValues;
            //UpdateScreen();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.lblCaption = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkedRMisc = new System.Windows.Forms.CheckedListBox();
            this.groupBoxKey = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RelatedEntityFiled = new System.Windows.Forms.ComboBox();
            this.BaseEntityField = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtKeyName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableRelation = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBoxKey.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(140, 16);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(292, 20);
            this.txtName.TabIndex = 1;
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(8, 19);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(116, 20);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Relation Name";
            // 
            // lblCaption
            // 
            this.lblCaption.Location = new System.Drawing.Point(8, 63);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(116, 20);
            this.lblCaption.TabIndex = 2;
            this.lblCaption.Text = "Misc";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(234, 361);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(96, 24);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "&OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(336, 361);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(96, 24);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // colCode
            // 
            this.colCode.DataPropertyName = "Code";
            this.colCode.HeaderText = "Code";
            this.colCode.Name = "colCode";
            // 
            // checkedRMisc
            // 
            this.checkedRMisc.FormattingEnabled = true;
            this.checkedRMisc.Items.AddRange(new object[] {
            "isOR",
            "IsParentChild",
            "IsSelfJoin"});
            this.checkedRMisc.Location = new System.Drawing.Point(140, 53);
            this.checkedRMisc.Name = "checkedRMisc";
            this.checkedRMisc.Size = new System.Drawing.Size(101, 49);
            this.checkedRMisc.TabIndex = 20;
            // 
            // groupBoxKey
            // 
            this.groupBoxKey.AutoSize = true;
            this.groupBoxKey.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxKey.Controls.Add(this.groupBox2);
            this.groupBoxKey.Controls.Add(this.txtKeyName);
            this.groupBoxKey.Controls.Add(this.label1);
            this.groupBoxKey.Enabled = false;
            this.groupBoxKey.Location = new System.Drawing.Point(11, 165);
            this.groupBoxKey.Name = "groupBoxKey";
            this.groupBoxKey.Size = new System.Drawing.Size(426, 176);
            this.groupBoxKey.TabIndex = 21;
            this.groupBoxKey.TabStop = false;
            this.groupBoxKey.Text = "Key";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RelatedEntityFiled);
            this.groupBox2.Controls.Add(this.BaseEntityField);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(21, 65);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(399, 92);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Misc";
            // 
            // RelatedEntityFiled
            // 
            this.RelatedEntityFiled.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RelatedEntityFiled.Location = new System.Drawing.Point(107, 43);
            this.RelatedEntityFiled.Name = "RelatedEntityFiled";
            this.RelatedEntityFiled.Size = new System.Drawing.Size(292, 21);
            this.RelatedEntityFiled.Sorted = true;
            this.RelatedEntityFiled.TabIndex = 5;
            // 
            // BaseEntityField
            // 
            this.BaseEntityField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BaseEntityField.Location = new System.Drawing.Point(107, 15);
            this.BaseEntityField.Name = "BaseEntityField";
            this.BaseEntityField.Size = new System.Drawing.Size(292, 21);
            this.BaseEntityField.Sorted = true;
            this.BaseEntityField.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "RelatedEntityFiled";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "BaseEntityField";
            // 
            // txtKeyName
            // 
            this.txtKeyName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeyName.Location = new System.Drawing.Point(128, 28);
            this.txtKeyName.Name = "txtKeyName";
            this.txtKeyName.Size = new System.Drawing.Size(292, 20);
            this.txtKeyName.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(25, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 20);
            this.label1.TabIndex = 22;
            this.label1.Text = "Name";
            // 
            // tableRelation
            // 
            this.tableRelation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tableRelation.Location = new System.Drawing.Point(139, 123);
            this.tableRelation.Name = "tableRelation";
            this.tableRelation.Size = new System.Drawing.Size(298, 21);
            this.tableRelation.Sorted = true;
            this.tableRelation.TabIndex = 6;
            this.tableRelation.SelectedIndexChanged += new System.EventHandler(this.TableRelation_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Table";
            // 
            // CreateFieldWithLookupRelationshipEntityWizard
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(447, 397);
            this.ControlBox = false;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tableRelation);
            this.Controls.Add(this.groupBoxKey);
            this.Controls.Add(this.checkedRMisc);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblCaption);
            this.Controls.Add(this.lblName);
            this.Name = "CreateFieldWithLookupRelationshipEntityWizard";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Lookup Relation Wizard";
            this.groupBoxKey.ResumeLayout(false);
            this.groupBoxKey.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        
        #endregion

        #region Event Handlers
        private void btnOK_Click(object sender, System.EventArgs e)
		{
			if(LookupName == "" 
				|| (txtKeyName.Visible && NameKeyFieldName == ""))
			{
				MessageBox.Show(ResourceUtils.GetString("EnterAllInfo"), 
                    ResourceUtils.GetString("LookupWiz"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
            this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
		#endregion

		#region Public Properties
		public string LookupName
		{
			get
			{
				return txtName.Text;
			}
		}

        public string NameKeyFieldName
        {
            get
            {
                return txtKeyName.Text;
            }
            set
            {
                txtKeyName.Text = value;
            }
        }

        private IDataEntity _entity;
        public IDataEntity Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
                SetUpForm();
            }
        }

        private AbstractSchemaItem _abstractSchemaItem = null;
        public AbstractSchemaItem abstSchemaItem
        {
            get
            {
                return _abstractSchemaItem;
            }
            set
            {
                _abstractSchemaItem = value;
            }
        }

        private IDataEntityColumn _idColumn = null;
        public IDataEntityColumn IdColumn
        {
            get
            {
                return _idColumn;
            }
        }

        public List<KeyValuePair<string,bool>> CheckedListmisc
        {
            get
            {
                var list = new List<KeyValuePair<string, bool>>();
                
                for (var i = 0; i <= (checkedRMisc.Items.Count - 1); i++)
                {
                   list.Add(new KeyValuePair<string, bool>(checkedRMisc.Items[i].ToString(), checkedRMisc.GetItemChecked(i)));
                }
                return list;
            }
        }

       

        public IList<InitialValue> InitialValues
        {
            get
            {
                return _initialValues;
            }
        }

        public InitialValue DefaultInitialValue
        {
            get
            {
                foreach (var item in InitialValues)
                {
                    if (item.IsDefault)
                    {
                        return item;
                    }
                }
                return null;
            }
        }
		#endregion

		#region Private Methods
		private void SetUpForm()
		{
            tableRelation.Items.Clear();

            if (this.Entity == null) return;
            txtName.Text = "Transaction " + this.Entity.Name;
            foreach (AbstractSchemaItem abstractSchemaIttem in this.Entity.RootProvider.ChildItems)
            {
                tableRelation.Items.Add(abstractSchemaIttem);
            }
        }

        private void SetUpFormKey()
        {
            BaseEntityField.Items.Clear();
            RelatedEntityFiled.Items.Clear();
            IDataEntityColumn nameColumn = null;

            if (this.Entity == null) return;
            
            txtKeyName.Text = abstSchemaItem.NodeText + " TransactionKey";
            foreach (AbstractSchemaItem filter in abstSchemaItem.ChildItemsByType("DataEntityColumn"))
            {
                RelatedEntityFiled.Items.Add(filter);
            }
            foreach (IDataEntityColumn column in this.Entity.EntityColumns)
            {
                if (column.Name == "Name") nameColumn = column;
                if (column.IsPrimaryKey && !column.ExcludeFromAllFields) _idColumn = column;

                BaseEntityField.Items.Add(column);
            }
            RelatedEntityFiled.SelectedItem = nameColumn;
            if (_idColumn == null) throw new Exception("Entity has no primary key defined. Cannot create lookup.");

        }

        #endregion

        private void TableRelation_SelectedIndexChanged(object sender, EventArgs e)
        {
            abstSchemaItem = (AbstractSchemaItem)tableRelation.SelectedItem;
            if (this.tableRelation.Name != "")
            {
                this.groupBoxKey.Enabled = true;
                SetUpFormKey();
            }
        }
    }
}
