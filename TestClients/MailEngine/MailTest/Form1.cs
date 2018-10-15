using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using GotDotNet.Exslt;
using Microsoft.ApplicationBlocks.ConfigurationManagement;
using Origam.DA;

namespace Origam.MailEngine
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnSend;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label lblStatus;
		private DataService _da = new DataService();
		private System.Windows.Forms.Label label1;

		public Form1()
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
				if (components != null) 
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
			this.btnSend = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(96, 48);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(96, 24);
			this.btnSend.TabIndex = 0;
			this.btnSend.Text = "Send";
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// lblStatus
			// 
			this.lblStatus.Location = new System.Drawing.Point(16, 200);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(264, 16);
			this.lblStatus.TabIndex = 1;
			this.lblStatus.Text = "Ready...";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(240, 24);
			this.label1.TabIndex = 2;
			this.label1.Text = "Press the button to send an e-mail:";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 77);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.btnSend);
			this.Name = "Form1";
			this.Text = "Mail Test Client";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			try
			{
				Application.Run(new Form1());
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.GetBaseException().Message);
			}
		}

		private void btnSend_Click(object sender, System.EventArgs e)
		{
			this.Cursor = System.Windows.Forms.Cursors.WaitCursor;

			//ConfigurationManager.Read("DaVinciMailQueryConfig");
			Origam.Mail.MailAgent ma = new Origam.Mail.MailAgent();
            
			ExsltTransform xslt = new ExsltTransform();
			System.IO.MemoryStream msTransform = new MemoryStream();
			System.IO.MemoryStream msData = new MemoryStream();

			lblStatus.Text = "Reading data...";
			//nacteme query z configu

			//nacteme data pomoci query
			DataSet ds = _da.LoadDataSet(new System.Guid("f7289945-3bf9-435e-9b69-a8dcecc90b80"), "user profile");
			ds.DataSetName = "ROOT";
			ds.WriteXml(msData);
			ds.WriteXml(@"DebugOutput\mailsource.xml", XmlWriteMode.WriteSchema);

			lblStatus.Text = "Transforming data...";

			//XPathDocument mailSource = new XPathDocument(@"c:\testmailsource.xml");
			msData.Position = 0;
			//PrintStream(msData);
			XPathDocument mailSource = new XPathDocument(msData);
			
			// provedeme transform, ktery provede insert do log tabulky
			xslt.Load(@"Stylesheets\testinsert.xsl");
			xslt.Transform(mailSource, null, msTransform);
			msTransform.Position = 0;
			DataSet dsInsert = new DataSet();
			dsInsert.ReadXml(msTransform, XmlReadMode.ReadSchema);
			
			_da.UpdateData(new System.Guid("fbb87b44-fe9a-4b44-b991-ccb62f7dae5b") , "user profile", dsInsert);

			//nacteme mailovaci xsl z configu
			xslt.Load(@"Stylesheets\testmail.xsl");
			msTransform = new MemoryStream();

			//ztransformujeme data do mailu
			xslt.Transform(mailSource, null, msTransform);
			msTransform.Position = 0;
			XmlDocument mailDoc = new XmlDocument();
			mailDoc.Load(msTransform);

			// a posleme maily - pokud mozno jeden po druhem a asynchronne
			lblStatus.Text = "Sending mails...";
			
			ma.SendMail(mailDoc);
			
			lblStatus.Text = "Ready...";
			this.Cursor = System.Windows.Forms.Cursors.Default;
		}

		private void PrintStream(System.IO.Stream s)
		{
			//set position to beginning of the stream
			s.Position = 0;
			using (StreamReader sr = new StreamReader(s)) 
			{
				char[] buf = new char[s.Length];
				sr.ReadBlock(buf, 0, (int)s.Length-1);
				MessageBox.Show(new string(buf));
			}
		}
	}
}
