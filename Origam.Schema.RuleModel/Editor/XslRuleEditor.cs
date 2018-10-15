using System;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Workbench;
using Origam.Workbench.Editors;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;

namespace Origam.Schema.RuleModel
{
	/// <summary>
	/// Summary description for XslTransformationEditor.
	/// </summary>
	public class XslRuleEditor : AbstractEditor
	{
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblText;
		private Netron.Neon.TextEditor.TextEditorControl txtText;
		private System.Windows.Forms.ComboBox cboDataStructure;
		private System.Windows.Forms.Label lblDataStructure;

		private bool _isEditing = false;

		public XslRuleEditor()
		{
			InitializeComponent();
			this.ContentLoaded += new EventHandler(XslRuleEditor_ContentLoaded);
		}

		#region Overriden AbstractViewContent Members

		public override void SaveObject()
		{
			(Content as XslRule).Xsl = txtText.Text;

			base.SaveObject();
		}
		#endregion

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(XslRuleEditor));
			this.txtName = new System.Windows.Forms.TextBox();
			this.lblName = new System.Windows.Forms.Label();
			this.lblText = new System.Windows.Forms.Label();
			this.txtText = new Netron.Neon.TextEditor.TextEditorControl();
			this.cboDataStructure = new System.Windows.Forms.ComboBox();
			this.lblDataStructure = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// txtName
			// 
			this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtName.Location = new System.Drawing.Point(128, 8);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(480, 20);
			this.txtName.TabIndex = 0;
			this.txtName.Text = "";
			this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
			// 
			// lblName
			// 
			this.lblName.Location = new System.Drawing.Point(8, 8);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(112, 16);
			this.lblName.TabIndex = 2;
			this.lblName.Text = "Name";
			// 
			// lblText
			// 
			this.lblText.Location = new System.Drawing.Point(8, 56);
			this.lblText.Name = "lblText";
			this.lblText.Size = new System.Drawing.Size(120, 16);
			this.lblText.TabIndex = 3;
			this.lblText.Text = "XSL";
			// 
			// txtText
			// 
			this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtText.Encoding = ((System.Text.Encoding)(resources.GetObject("txtText.Encoding")));
			this.txtText.LineViewerStyle = Netron.Neon.TextEditor.Document.LineViewerStyle.FullRow;
			this.txtText.Location = new System.Drawing.Point(0, 80);
			this.txtText.Name = "txtText";
			this.txtText.Root = null;
			this.txtText.ShowEOLMarkers = true;
			this.txtText.ShowSpaces = true;
			this.txtText.ShowTabs = true;
			this.txtText.ShowVRuler = true;
			this.txtText.Size = new System.Drawing.Size(616, 240);
			this.txtText.Tab = null;
			this.txtText.TabIndex = 5;
			this.txtText.ActiveTextAreaControl.TextArea.KeyDown += new KeyEventHandler(TextArea_KeyDown);
			// 
			// cboDataStructure
			// 
			this.cboDataStructure.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cboDataStructure.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDataStructure.Location = new System.Drawing.Point(128, 32);
			this.cboDataStructure.Name = "cboDataStructure";
			this.cboDataStructure.Size = new System.Drawing.Size(480, 21);
			this.cboDataStructure.Sorted = true;
			this.cboDataStructure.TabIndex = 6;
			this.cboDataStructure.SelectedValueChanged += new System.EventHandler(this.cboDataStructure_SelectedValueChanged);
			this.cboDataStructure.Click += new System.EventHandler(this.cboDataStructure_Click);
			// 
			// lblDataStructure
			// 
			this.lblDataStructure.Location = new System.Drawing.Point(8, 32);
			this.lblDataStructure.Name = "lblDataStructure";
			this.lblDataStructure.Size = new System.Drawing.Size(120, 16);
			this.lblDataStructure.TabIndex = 7;
			this.lblDataStructure.Text = "Data structure:";
			// 
			// XslRuleEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 317);
			this.Controls.Add(this.lblDataStructure);
			this.Controls.Add(this.cboDataStructure);
			this.Controls.Add(this.txtText);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.lblText);
			this.Controls.Add(this.lblName);
			this.Name = "XslRuleEditor";
			this.ResumeLayout(false);

		}

		
		private void txtName_TextChanged(object sender, System.EventArgs e)
		{
			if(Content != null)
			{
				if(! _isEditing)
				{
					Content.Name = txtName.Text;
					this.IsDirty = true;
					this.TitleName = Content.Name;
				}
			}
		}

		private void cboDataStructure_Click(object sender, System.EventArgs e)
		{
			_isEditing = true;

			IDataStructure oldValue = cboDataStructure.SelectedItem as IDataStructure;

			SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
			DataStructureSchemaItemProvider structures = schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;

			cboDataStructure.BeginUpdate();
			cboDataStructure.Items.Clear();

			foreach(IDataStructure structure in structures.ChildItems)
			{
				cboDataStructure.Items.Add(structure);
			}
			
			cboDataStructure.EndUpdate();

			if(oldValue != null)
			{
				if(cboDataStructure.Items.Contains(oldValue))
				{
					cboDataStructure.SelectedItem = oldValue;
				}
			}

			_isEditing = false;
		}

		private void cboDataStructure_SelectedValueChanged(object sender, System.EventArgs e)
		{
			if(! _isEditing)
			{
				(Content as XslRule).Structure = cboDataStructure.SelectedItem as IDataStructure;
				this.IsDirty = true;
			}
		}

		private void TextArea_KeyDown(object sender, KeyEventArgs e)
		{
			if(Content != null)
			{
				if(! _isEditing)
				{
					this.IsDirty = true;
				}
			}
		}

		private void XslRuleEditor_ContentLoaded(object sender, EventArgs e)
		{
			txtText.SetHighlighting("XML");

			if(Content is XslRule)
			{
				XslRule _xslRule = Content as XslRule;

				_isEditing = true;
				txtName.Text = _xslRule.Name;
				txtText.Text = _xslRule.Xsl;

				cboDataStructure.Items.Clear();

				if(_xslRule.Structure != null)
				{
					cboDataStructure.Items.Add(_xslRule.Structure);
					cboDataStructure.SelectedItem = (_xslRule.Structure);
				}

				_isEditing = false;
			}
			else
			{
				throw new InvalidCastException("Only XslRule objects are supported.");
			}
		}
	}
}
