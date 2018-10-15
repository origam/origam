using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using Origam.Workbench.Services;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema.EntityModel;

using Microsoft.Office.Core;
using ppt=Microsoft.Office.Interop.PowerPoint;

namespace Origam.Workflow.Star21Service
{
	/// <summary>
	/// Summary description for DataService.
	/// </summary>
	public class Star21ServiceAgent : AbstractServiceAgent
	{
		public Star21ServiceAgent()
		{
		}

		#region PowerPoint Service

		private object ExportDocumentation(XmlDataDocument data, string templatePath, string destinationPath)
		{
			PptDocumentation doc = new PptDocumentation();
			doc.Merge(data.DataSet);

			Microsoft.Office.Interop.PowerPoint.Application app = new Microsoft.Office.Interop.PowerPoint.ApplicationClass();
			try
			{
				Microsoft.Office.Interop.PowerPoint.Presentation pres = app.Presentations.OpenOld(templatePath, Microsoft.Office.Core.MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);

				//object tb =  pres.Slides["Slide4"].Shapes["LocMesto"]; 
				
				// master slide (background for all slides)
				foreach(ppt.Shape shape in FindSlideByName(pres.Slides, "Slide4").Master.Shapes)
				{
					ProcessField(shape, doc);
				}
				
				// try to add a bitmap of the location, but if it fails, nothing happens
				try
				{
					string picturePath = (this.RuleEngine as Rule.RuleEngine).GetConstant("PptDocumentation_LocationPicturePath");
					string pictureName = doc.Location[0].ReferenceCode + "_map.jpg";

					string destination = Path.Combine(Path.Combine(picturePath, doc.Location[0].ReferenceCode.Substring(0, 3) + @"\"), pictureName);

					ppt.Shape pic = FindSlideByName(pres.Slides, "Slide4").Shapes.AddPicture(
						destination,
						MsoTriState.msoFalse, 
						MsoTriState.msoTrue, 
						Convert.ToSingle(59.24), 
						Convert.ToSingle(79.37), 
						Convert.ToSingle(309.83), 
						Convert.ToSingle(423.78)
						);

					pic.ScaleHeight(1, MsoTriState.msoTrue, MsoScaleFrom.msoScaleFromTopLeft);
					pic.ScaleWidth(1, MsoTriState.msoTrue, MsoScaleFrom.msoScaleFromTopLeft);
				} 
				catch(Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}

				// slide
				foreach(ppt.Shape shape in FindSlideByName(pres.Slides, "Slide4").Shapes)
				{
					ProcessField(shape, doc);
				}

				string fileName = doc.Location[0].ReferenceCode 
					+ "_" 
					+ doc.TelcoService[0].ReferenceCode
					+ ".ppt";

				fileName = Regex.Replace(fileName, @"[\\/:\*\?""<>\|]", "_");
				
				pres.SaveAs(Path.Combine(destinationPath, fileName), ppt.PpSaveAsFileType.ppSaveAsPresentation, MsoTriState.msoFalse);
			}
			catch(Exception ex)
			{
				System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
				throw new Exception(ResourceUtils.GetString("ErrorWhenPowerPointExport", Environment.NewLine + ex.Message), ex);
			}

			System.Runtime.InteropServices.Marshal.ReleaseComObject(app);

			return null;
		}

		private ppt.Slide FindSlideByName(ppt.Slides slides, string name)
		{
			foreach(ppt.Slide slide in slides)
			{
				if(slide.Name == name)
				{
					return slide;
				}
			}

			throw new ArgumentOutOfRangeException("name", name, ResourceUtils.GetString("ErrorSlideNotFound"));
		}

		private void ProcessField(ppt.Shape shape, PptDocumentation doc)
		{
			if(shape.Type == MsoShapeType.msoTable)
			{ 
				ppt.Table t = shape.Table;
	
				foreach(ppt.Row row in t.Rows)
				{
					foreach(ppt.Cell cell in row.Cells)
					{
						try
						{
							switch(cell.Shape.TextFrame.TextRange.Text)
							{
								case "<KEYACCOUNT>":
									UpdatePptField(cell.Shape.TextFrame, doc.KeyAccountManager[0].Name1 + " " + doc.KeyAccountManager[0].Name2);
									break;
									
								case "<CODE>":
									UpdatePptField(cell.Shape.TextFrame, doc.Project[0].ReferenceCode);
									break;

								case "<LOCATION>":
									UpdatePptField(cell.Shape.TextFrame, doc.Location[0].ReferenceCode);
									break;
								
								case "<REGION>":
									UpdatePptField(cell.Shape.TextFrame, doc.Location[0].City);
									break;
									
								case "<ADDRESS>":
									UpdatePptField(cell.Shape.TextFrame, doc.Street[0].Name + "/" + doc.Location[0].HouseNumber);
									break;
									
								case "<CONTACT1>":
								case "<CONTACT2>":
								case "<CONTACT3>":
									PptDocumentation.TelcoServiceNetworkElementBusinessPartnerDataRow locBPcontact = doc.TelcoServiceNetworkElementBusinessPartnerData[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(8,1))-1];
									UpdatePptField(
										cell.Shape.TextFrame, 
										locBPcontact.Name +
										(locBPcontact.IsFirstNameNull() ? "" : " " + locBPcontact.FirstName)
										);
									break;

								case "<PHONE1>":
								case "<PHONE2>":
								case "<PHONE3>":
									PptDocumentation.TelcoServiceNetworkElementBusinessPartnerDataRow locBPphone = doc.TelcoServiceNetworkElementBusinessPartnerData[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(6,1))-1];
									string phone1 = locBPphone.IsCommunicationPhoneWorkNull() ? "---" : locBPphone.CommunicationPhoneWork;
									string phone2 = locBPphone.IsCommunicationPhoneWork2Null() ? "---" : locBPphone.CommunicationPhoneWork2;
									string mobile = locBPphone.IsCommunicationPhoneMobileNull() ? "---" : locBPphone.CommunicationPhoneMobile;

									UpdatePptField(
										cell.Shape.TextFrame, 
										phone1 + ", " + phone2 + ", " + mobile
										);
									break;

								case "<CLIENT>":
									UpdatePptField(cell.Shape.TextFrame, doc.PartnerBusinessPartner[0].Name);
									break;
								
								case "<SERVICE>":
									UpdatePptField(cell.Shape.TextFrame, doc.TelcoService[0].ReferenceCode);
									break;

								case "<BUSINESSCASE>":
									UpdatePptField(cell.Shape.TextFrame, doc.BusinessCase[0].ReferenceCode + ", " + doc.BusinessCase[0].Name);
									break;

								case "<INTERFACE>":
									UpdatePptField(cell.Shape.TextFrame, doc.TelcoServiceNetworkElement[0].TelcoInterfaceType_Text);
									break;

								case "<SERVICEPROTOCOL>":
									UpdatePptField(cell.Shape.TextFrame, doc.TelcoService[0].TelcoProtocol_Text);
									break;

								case "<BANDWIDTHMIR>":
									UpdatePptField(cell.Shape.TextFrame, doc.TelcoService[0].BandwidthKbpsMir.ToString());
									break;

								case "<BS1>":
								case "<BS2>":
								case "<BS3>":
								case "<BS4>":
									UpdatePptField(
										cell.Shape.TextFrame, 
										doc.TelcoNetworkElementsInRange[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(3,1))-1].GetNetworkElementInRangeRows()[0].Description
										);
									break;

								case "<CODE1>":
								case "<CODE2>":
								case "<CODE3>":
								case "<CODE4>":
									UpdatePptField(
										cell.Shape.TextFrame, 
										doc.TelcoNetworkElementsInRange[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(5,1))-1].GetNetworkElementInRangeRows()[0].ReferenceCode
										);
									break;

								case "<AZIMUTH1>":
								case "<AZIMUTH2>":
								case "<AZIMUTH3>":
								case "<AZIMUTH4>":
									UpdatePptField(
										cell.Shape.TextFrame, 
										doc.TelcoNetworkElementsInRange[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(8,1))-1].Azimuth.ToString()
										);
									break;

								case "<DISTANCE1>":
								case "<DISTANCE2>":
								case "<DISTANCE3>":
								case "<DISTANCE4>":
									UpdatePptField(
										cell.Shape.TextFrame, 
										doc.TelcoNetworkElementsInRange[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(9,1))-1].DistanceKm.ToString()
										);
									break;

								case "<SECTOR1>":
								case "<SECTOR2>":
								case "<SECTOR3>":
								case "<SECTOR4>":
									UpdatePptField(
										cell.Shape.TextFrame, 
										doc.TelcoNetworkElementsInRange[Convert.ToInt32(cell.Shape.TextFrame.TextRange.Text.Substring(7,1))-1].GetSubNetworkElementInRangeRows()[0].ReferenceCode
										);
									break;
							}
						}
						catch(Exception ex)
						{
							System.Diagnostics.Debug.WriteLine(cell.Shape.TextFrame.TextRange.Text.Substring(3, 1));
							cell.Shape.TextFrame.TextRange.Text = "";
							System.Diagnostics.Debug.WriteLine(ex.Message);
						}
					}
				}
			}
		}
		private void UpdatePptField(ppt.TextFrame frame, string text)
		{
			frame.TextRange.Text = text;
		}
		#endregion

		#region IServiceAgent Members

		private object _result;
		public override object Result
		{
			get
			{
					object temp = _result;
				_result = null;
				
				return temp;
			}
		}

		public override void Run()
		{
			switch(this.MethodName)
			{
				case "ExportDocumentation":
					// Check input parameters
					if(! (this.Parameters["Data"] is XmlDataDocument))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDataDocument"));

					if(! (this.Parameters["DestinationPath"] is string))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorDestinationPathNotString"));

					if(! (this.Parameters["TemplatePath"] is string))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorTemplatePathNotString"));

					_result = this.ExportDocumentation(this.Parameters["Data"] as XmlDataDocument, this.Parameters["TemplatePath"] as String, this.Parameters["DestinationPath"] as String);
					break;
			}
		}

		#endregion
	}
}
