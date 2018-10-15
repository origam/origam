using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace AsMailService
{
	/// <summary>
	/// Summary description for Service1.
	/// </summary>
	/// 

	[WebService(Description="Advantage Solutions mailer service",Namespace="http://www.advantages.cz/")]
	public class AsMailer : System.Web.Services.WebService
	{
		public AsMailer()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		private Origam.Mail.MailAgent mailAgent = new Origam.Mail.MailAgent();

		[WebMethod]
		public int SendMail(XmlDocument mailDocument)
		{
			int retVal=0;
			try
			{
				retVal= mailAgent.SendMail(mailDocument);
			
			}
			catch(Exception ex)
			{
				retVal=-1;
				Console.WriteLine(ex.ToString());
			}
			return retVal;
				
		}
	}
}
