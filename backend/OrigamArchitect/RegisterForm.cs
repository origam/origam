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
using System.Net;
using System.Windows.Forms;
using Origam.UI;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Origam;

namespace OrigamArchitect
{
	public partial class RegisterForm : Form
	{
		public RegisterForm()
		{
			InitializeComponent();
			this.BackColor = OrigamColorScheme.FormBackgroundColor;
			this.btnRegister.BackColor = OrigamColorScheme.ButtonBackColor;
			this.btnRegister.ForeColor = OrigamColorScheme.ButtonForeColor;
			this.btnCancel.BackColor = OrigamColorScheme.ButtonBackColor;
			this.btnCancel.ForeColor = OrigamColorScheme.ButtonForeColor;
		}

		public string Email
		{
			get
			{
				return txtEmail.Text.ToString();
			}
		}


		private void RegisterForm_Load(object sender, EventArgs e)
		{

		}

		private void btnRegister_Click_1(object sender, EventArgs e)
		{
			/*
			if (string.IsNullOrEmpty(txtEmail.Text.Trim()))
			{

				epEmail.SetError(txtEmail, "Email is a Mandatory Field.");
				lblErrorMessage.Text = "Email is a MandatoryField\nPlease fill it out.";
				lblErrorMessage.Visible = true;
				//Origam.UI.AsMessageBox.ShowError(null, "Email is a mandatory field.", "Email error", null);
				return;
			}
			*/

			Cursor.Current = Cursors.WaitCursor;
			JObject jobj = new JObject(
				new JProperty("BusinessPartner",
					new JObject(
						new JProperty("UserEmail", txtEmail.Text.Trim())
						, new JProperty("Name", txtSurname.Text.Trim())
						, new JProperty("FirstName", txtFirstName.Text.Trim())
						, new JProperty("UserPassword", txtPassword.Text.Trim())
						, new JProperty("ConfirmUserPassword", txtConfirmPassword.Text.Trim())
						, new JProperty("Phone", txtPhone.Text.Trim())
						, new JProperty("UserName", txtEmail.Text.Trim())
						, new JProperty("Id", Guid.NewGuid().ToString())
					)
				)
			);
			
			string output = null;
			try
			{				
				using (WebResponse webResponse = HttpTools.Instance.GetResponse(
					string.Format("{0}public/RegisterDownloadsUser", frmMain.ORIGAM_COM_API_BASEURL),
				"POST", jobj.ToString(), "application/json",
				new Hashtable()
				{ { "Accept-Encoding", "gzip,deflate"} }
				, null, null, null, 15000, null,
				frmMain.IgnoreHTTPSErrors))
				{
					HttpWebResponse httpWebResponse = webResponse as HttpWebResponse;
					output = HttpTools.Instance.ReadResponseTextRespectionContentEncoding(httpWebResponse);
					if (httpWebResponse.StatusCode == HttpStatusCode.OK)
					{
						MessageBox.Show(string.Format(
								strings.RegisterForm_Success_Message,
								txtEmail.Text.Trim()),
								strings.RegisterForm_Success_Label,
								MessageBoxButtons.OK,
								MessageBoxIcon.Information);
						
						// success
						this.DialogResult = DialogResult.OK;
						this.Close();
					}
				}
            }
			catch (System.Net.WebException wex)
			{
				if (wex.Status == WebExceptionStatus.Timeout
					|| wex.Status == WebExceptionStatus.ConnectFailure)
				{
					AsMessageBox.ShowError(null,
						string.Format(strings.RegisterAndLoginForm_TemporaryHttpError,
						wex.Message),
						strings.RegisterForm_Error_Label, wex);
					return;
				}
				string errorInfo = null;
				using (HttpWebResponse httpWebResponse = wex.Response as HttpWebResponse)
				{
					if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.BadRequest)
					{
						errorInfo = HttpTools.Instance.ReadResponseTextRespectionContentEncoding(httpWebResponse);
						JObject jResult = (JObject)JsonConvert.DeserializeObject(errorInfo);

						if (jResult["ClassName"] != null)
						{
							MessageBox.Show(jResult["Message"].ToString(),
								strings.RegisterForm_Warning_Label,
								MessageBoxButtons.OK,
								MessageBoxIcon.Exclamation);
							return;
						}
						else if (jResult["RuleResult"] != null)
						{
							if ((string)jResult["RuleResult"][0]["EntityName"]
								== "SendEmailConfirmationTokenError")
							{
								MessageBox.Show(string.Format(
									strings.RegisterForm_CouldNotSendConfirmationMail_Message,
									jResult["Message"].ToString()),
									strings.RegisterForm_Error_Label,
									MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								this.Close();
								return;
							}
							else
							{
								MessageBox.Show(jResult["Message"].ToString(),
								strings.RegisterForm_Error_Label,
								MessageBoxButtons.OK,
								MessageBoxIcon.Exclamation);
								return;
							}
						}
					}					
				}
				AsMessageBox.ShowError(null,
					strings.RegisterLoginForm_UnexpectedError_Message,
					strings.RegisterForm_Error_Label, wex);
			}			
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
