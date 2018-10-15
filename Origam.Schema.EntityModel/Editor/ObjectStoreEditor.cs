using System;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Workbench;
using Origam.Workbench.Editors;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for XslTransformationEditor.
	/// </summary>
	public class XslTransformationEditor : AbstractEditor
	{
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label lblText;
		private Netron.Neon.TextEditor.TextEditorControl txtText;
//		private XslTransformation _XslTransformation;

		private bool _isEditing = false;

		public XslTransformationEditor()
		{
			InitializeComponent();

			this.ContentLoaded += new EventHandler(XslTransformationEditor_ContentLoaded);
		}

		#region Overriden AbstractViewContent Members
		public override void SaveObject()
		{
			(Content as XslTransformation).TextStore = txtText.Text;
			
			base.SaveObject();
		}
		#endregion

		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(XslTransformationEditor));
			this.txtName = new System.Windows.Forms.TextBox();
			this.lblName = new System.Windows.Forms.Label();
			this.lblText = new System.Windows.Forms.Label();
			this.txtText = new Netron.Neon.TextEditor.TextEditorControl();
			this.SuspendLayout();
			// 
			// txtName
			// 
			this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtName.Location = new System.Drawing.Point(128, 8);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(528, 20);
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
			this.lblText.Location = new System.Drawing.Point(8, 32);
			this.lblText.Name = "lblText";
			this.lblText.Size = new System.Drawing.Size(120, 16);
			this.lblText.TabIndex = 3;
			this.lblText.Text = "Text object";
			// 
			// txtText
			// 
			this.txtText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtText.Encoding = ((System.Text.Encoding)(resources.GetObject("txtText.Encoding")));
			this.txtText.LineViewerStyle = Netron.Neon.TextEditor.Document.LineViewerStyle.FullRow;
			this.txtText.Location = new System.Drawing.Point(0, 56);
			this.txtText.Name = "txtText";
			this.txtText.Root = null;
			this.txtText.ShowEOLMarkers = true;
			this.txtText.ShowSpaces = true;
			this.txtText.ShowTabs = true;
			this.txtText.ShowVRuler = true;
			this.txtText.Size = new System.Drawing.Size(664, 288);
			this.txtText.Tab = null;
			this.txtText.TabIndex = 5;
			this.txtText.ActiveTextAreaControl.TextArea.KeyDown += new KeyEventHandler(TextArea_KeyDown);
			// 
			// XslTransformationEditor
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(664, 341);
			this.Controls.Add(this.txtText);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.lblText);
			this.Controls.Add(this.lblName);
			this.Name = "XslTransformationEditor";
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

		private void XslTransformationEditor_ContentLoaded(object sender, EventArgs e)
		{
			txtText.SetHighlighting("XML");

			if(Content is XslTransformation)
			{
				XslTransformation _XslTransformation = Content as XslTransformation;

				_isEditing = true;
				txtName.Text = _XslTransformation.Name;
				txtText.Text = _XslTransformation.TextStore;
				_isEditing = false;
			}
			else
			{
				throw new InvalidCastException("Only XslTransformation objects are supported.");
			}
		}
	}
}
