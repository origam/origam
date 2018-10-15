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
using System.Data;
using System.Xml;
using System.Text;
using System.Net;
using System.Xml.XPath;

using Origam.Workflow;
using Origam.DA.Service;
using Origam.Schema.EntityModel;

namespace Origam.Sharepoint
{
	/// <summary>
	/// Summary description for SharepointListAgent.
	/// </summary>
	public class SharepointListAgent : AbstractServiceAgent
	{
		ListsWebService.Lists _lists = new ListsWebService.Lists();
		DatasetGenerator _datasetGenerator = new DatasetGenerator(true);

		public SharepointListAgent()
		{
			CredentialCache credentials = new System.Net.CredentialCache();
			NetworkCredential cred = new NetworkCredential("advantages", "solutions", "forbo");
			credentials.Add(new Uri(_lists.Url), "NTLM", cred); // "http://82.208.17.241:880"
			_lists.Credentials = credentials; //System.Net.CredentialCache.DefaultCredentials;
		}

		public XmlDataDocument GetListItems(string listName, string viewName)
		{
			XmlNode viewFieldsNode = GetListFieldSchema(listName, _lists);

			XmlNode node = _lists.GetListItems(listName, viewName, null, viewFieldsNode, "0", null);
			
			XmlNodeReader nodereader = new XmlNodeReader(node);

			DataSet data = _datasetGenerator.CreateDataSet(this.OutputStructure as DataStructure);
			
			DataSet tempData = new DataSet("ROOT");
			tempData.ReadXml(nodereader);
			
			if(tempData.Tables.Contains("row"))
			{
				tempData.Tables["row"].AcceptChanges();

				// We get rid of all the "ows_" column name prefixes
				foreach(DataColumn column in tempData.Tables["row"].Columns)
				{
					column.ColumnName = column.ColumnName.Replace("ows_", "");
				}

				// And finally we rename the only table returned by SharePoint to the same name as list
				tempData.Tables["row"].TableName = listName;


				data.Merge(tempData, false, MissingSchemaAction.Ignore);
			}

			return new XmlDataDocument(data);
		}

		public void UpdateListItems(string listName, string viewName, XmlDataDocument data)
		{
			if(! data.DataSet.HasChanges()) return;

			DataTable changes = data.DataSet.Tables[listName].GetChanges();
			if(changes == null) return;

			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			System.Xml.XmlElement batchElement = doc.CreateElement("Batch");
			batchElement.SetAttribute("OnError","Return");
			batchElement.SetAttribute("ListVersion","1");
			batchElement.SetAttribute("ViewName", viewName);

			/* Specify methods for the batch post using CAML. In each method include 
			the ID of the item to update and the value to place in the specified column.*/
			
			int rowNum = 0;
			foreach(DataRow row in changes.Rows)
			{
				XmlElement methodElement = doc.CreateElement("Method");
				methodElement.SetAttribute("ID", rowNum.ToString());
				string command = "";

				switch(row.RowState)
				{
					case DataRowState.Added:
						command = "New";
						break;
					case DataRowState.Deleted:
						command = "Delete";
						break;
					case DataRowState.Modified:
						command = "Update";
						break;
				}
				
				if(command == "") break;
				methodElement.SetAttribute("Cmd", command);

				foreach(DataColumn column in changes.Columns)
				{
					if(!(command == "New" & column.ColumnName.ToUpper() == "ID"))
					{
						if(row[column] != DBNull.Value)
						{
							XmlElement fieldElement = doc.CreateElement("Field");
							fieldElement.SetAttribute("Name", column.ColumnName);
					
							string elementValue;
							
							if(column.DataType == typeof(DateTime))
							{
								elementValue = XmlConvert.ToString((DateTime)row[column]);
							}
							else if(column.DataType == typeof(System.Decimal))
							{
								elementValue = XmlConvert.ToString((System.Decimal)row[column]);
							}
							else if(column.DataType == typeof(int))
							{
								elementValue = XmlConvert.ToString((int)row[column]);
							}
							else if(column.DataType == typeof(float))
							{
								elementValue = XmlConvert.ToString((float)row[column]);
							}
							else
							{
								elementValue = row[column].ToString();
							}


							fieldElement.InnerText = elementValue;

							methodElement.AppendChild(fieldElement);
						}
					}
				}

				batchElement.AppendChild(methodElement);
				rowNum++;
			}

			XmlNode result;
			// Update list items.
			try
			{
				result = _lists.UpdateListItems(listName, batchElement);
			}
			catch(System.Web.Services.Protocols.SoapException soapEx)
			{
				throw new Exception(ResourceUtils.GetString("ErrorWhenSendingList", listName, 
					Environment.NewLine + soapEx.Detail.InnerText), soapEx);
			}

			XPathNavigator nav = result.CreateNavigator();
			
			XPathNodeIterator iter = nav.Select("//ErrorText");
			
			string errors = "";

			while (iter.MoveNext())
			{
				errors += iter.Current.Value + ";";
			};

			if(errors != "")
			{
				throw new InvalidOperationException(errors);
			}
		}

		// ***
		/// <summary>
		/// Takes the Xml data returned from GetList and gets the CAML
		/// representation of the Lists schema.
		/// </summary>
		/// <param name="listName">Name of the list.</param>
		/// <param name="webService">WebService to use for the connection.</param>
		/// <returns>XmlNode object with the transformed Xml Data.</returns>
		private XmlNode GetListFieldSchema(string listName, ListsWebService.Lists webService)
		{
			XmlDocument xmlDoc = new XmlDocument();

			XmlNode listSchemaNode = webService.GetList(listName);
			XmlNode fieldsNode = listSchemaNode.FirstChild;
			XmlNode viewFieldsNode = xmlDoc.CreateNode(XmlNodeType.Element, "ViewFields", "");
			StringBuilder innerXml = new StringBuilder();
            
			foreach (XmlNode field in fieldsNode.ChildNodes)
			{
				bool fromBaseType = false;
				try
				{
					if(field.Attributes["ReadOnly"] != null)
					{
						fromBaseType = Boolean.Parse(field.Attributes["ReadOnly"].Value);
					}
				}
				catch
				{
				}

				if (!fromBaseType)
				{
					innerXml.AppendFormat("<FieldRef Name='{0}' />", field.Attributes["Name"].Value);
				}
			}

			viewFieldsNode.InnerXml = innerXml.ToString();
			return viewFieldsNode;
		}

		#region IServiceAgent Members
		private object _result;
		public override object Result
		{
			get
			{
				return _result;
			}
		}

		public override void Run()
		{
			switch(this.MethodName)
			{
				case "GetListItems":
					// Check input parameters
					if(! (this.Parameters["ListName"] is string))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorListNameNotString"));

					if(! (this.Parameters["ViewName"] is string | this.Parameters["ViewName"] == null ))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorViewNameNotString"));

					_result = 
						this.GetListItems(this.Parameters["ListName"] as String,
						this.Parameters["ViewName"] as String);

					break;

				case "UpdateListItems":
					// Check input parameters
					if(! (this.Parameters["ListName"] is string))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorListNameNotString"));

					if(! (this.Parameters["ViewName"] is string | this.Parameters["ViewName"] == null ))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorViewNameNotString"));

					if(! (this.Parameters["Data"] is XmlDataDocument))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorDataNotXml"));

					this.UpdateListItems(this.Parameters["ListName"] as String,
						this.Parameters["ViewName"] as String,
						this.Parameters["Data"] as XmlDataDocument);

					_result = null;

					break;
			}
		}

		#endregion
	}
}
