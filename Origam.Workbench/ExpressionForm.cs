using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;
using Origam.Query;
using Origam.Query.UI.Designer;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for ExpressionForm.
	/// </summary>
	internal class ExpressionForm : DockContent
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ExpressionForm()
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
			// 
			// ExpressionForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(336, 283);
			this.DockableAreas = WeifenLuo.WinFormsUI.DockAreas.Document;
			this.Name = "ExpressionForm";
			this.Text = "ExpressionForm";

		}
		#endregion

		IExpressionDictionary mExpressionDictionary = null;
		public IExpressionDictionary ExpressionDictionary
		{
			get
			{
				return mExpressionDictionary;
			}
			set
			{
				mExpressionDictionary = value;
			}
		}

		ArrayList mExtensions = null;
	
		public ArrayList Extensions
		{
			get
			{
				return mExtensions;
			}
			set
			{
				mExtensions = value;
			}
		}

		IExpression mExpression = null;
		public IExpression ExpressionEdited
		{
			get
			{
				return mExpression;
			}
			set
			{
				mExpression = value;

				ExpressionEditor ee = GetExpressionEditor(mExpression);
				ee.Expression = mExpression;

				this.AutoScroll = true;
				this.Text = mExpression.ToString();
				//f.Icon
				this.Controls.Add(ee);
			}
		}

		public void ChangeExpressionType(ExpressionType TargetType)
		{
			ExpressionFactory ef = new ExpressionFactory(this.ExpressionDictionary);
			ExpressionEditor oldEd = (ExpressionEditor)this.Controls[0];
			IExpression e = oldEd.Expression;
			
			ExpressionEditor newEd = GetExpressionEditor(ef.ChangeExpressionType(e, TargetType));

			//remove old editor
			this.Controls.RemoveAt(0);
			//put new editor on tab page
			this.Controls.Add(newEd);
			//f.Icon
			//mark the page changed
			//MarkPageChanged(tabEditors.SelectedTab, newEd.Expression.Name);
		}

		private ExpressionEditor GetExpressionEditor(IExpression e)
		{
			ExpressionEditorFactory eeFact = new ExpressionEditorFactory();
			ExpressionEditor ee = eeFact.GetEditor(e.Type, mExpressionDictionary, mExtensions);
			ee.Expression = e;
			ee.Dock = DockStyle.Fill;
			//ee.ExpressionChange +=new EventHandler(ee_ExpressionChange);

			return ee;
		}

	}
}
