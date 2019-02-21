using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using Noris.WS.ServiceGate;
using Noris.Clients.ServiceGate;
using System.Xml;

namespace Origam.Workflow.LcsHeliosService
{
    /// <summary>
    /// Summary description for LcsHeliosAgent.
    /// </summary>
    public class LcsHeliosAgent : AbstractServiceAgent
    {
        public LcsHeliosAgent()
        {
        }

        private XmlDocument InsertUpdate(string serverName, string dbProfile, string userName, string password, XmlDocument data)
        {
            ServiceGateConnector connector = ServiceGateConnector.Create(serverName);
            LogOnInfo li = new LogOnInfo(dbProfile, userName ?? "", password ?? "");
            using (LogOnScope ls = new LogOnScope(connector, li))
            {
                XmlReader reader = new XmlNodeReader(data);
                reader.Read();
                DataRecordList records = new DataRecordList(reader);
                InsertUpdateRequest request = new InsertUpdateRequest(records);
                InsertUpdateResponse response = request.Process(connector);
                XmlDocument result = new XmlDocument();
                result.LoadXml(response.RawXml);
                return result;
            }
        }

        private IXmlContainer Browse(string serverName, string dbProfile, string userName, string password,
            int folderId, int templateNumber, string skipArguments)
        {
            ServiceGateConnector connector = ServiceGateConnector.Create(serverName);
            LogOnInfo li = new LogOnInfo(dbProfile, userName ?? "", password ?? "");
            using (LogOnScope ls = new LogOnScope(connector, li))
            {
                BrowseRequest request = new BrowseRequest(folderId, new BrowseId(BrowseType.Template, templateNumber));
                request.BrowseLookup = BrowseLookupType.IdDefined;
                if (skipArguments != null)
                {
                    request.BaseFilterArguments = new FilterArgumentList();
                    foreach (string skipArgument in skipArguments.Split(",".ToCharArray()))
                    {
                        request.BaseFilterArguments.Add(skipArgument, true);
                    }
                }
                request.Bounds.LowerBound = 1;
                request.Bounds.UpperBound = -1;
                BrowseResponse response = request.Process(connector);
                if (response.Data == null || response.Data.MainTable.Rows.Count == 0)
                {
                    return  new XmlContainer("<ROOT/>");
                }
                else
                {
                    DataSet ds = new DataSet();
                    ds.DataSetName = "ROOT";
                    DataTable table = response.Data.MainTable;
                    ds.Tables.Add(table);
                    return DataDocumentFactory.New(ds);
                }
            } 
       }

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
            switch (this.MethodName)
            {
                case "BrowseTemplate":
                    // Check input parameters
                    if (!(this.Parameters["TemplateNumber"] is int))
                        throw new InvalidCastException("TemplateNumber has to be a int data type.");

                    if (!(this.Parameters["FolderId"] is int))
                        throw new InvalidCastException("FolderId has to be a int data type.");

                    if (!(this.Parameters["ServerName"] is string))
                        throw new InvalidCastException("ServerName has to be string.");

                    if (!(this.Parameters["DbProfile"] is string))
                        throw new InvalidCastException("DbProfile has to be string.");

                    // user name and a password can be empty - Integrated Authentication is used then

                    _result = this.Browse(
                        (string)this.Parameters["ServerName"],
                        (string)this.Parameters["DbProfile"],
                        (string)this.Parameters["UserName"],
                        (string)this.Parameters["Password"],
                        (int)this.Parameters["FolderId"],
                        (int)this.Parameters["TemplateNumber"],
                        (string)this.Parameters["SkipArguments"]
                        );
                    break;

                case "InsertUpdate":
                    // Check input parameters
                    if (!(this.Parameters["Data"] is XmlDocument))
                        throw new InvalidCastException("Data has to be an XmlDocument.");

                    if (!(this.Parameters["ServerName"] is string))
                        throw new InvalidCastException("ServerName has to be string.");

                    if (!(this.Parameters["DbProfile"] is string))
                        throw new InvalidCastException("DbProfile has to be string.");

                    // user name and a password can be empty - Integrated Authentication is used then

                    _result = this.InsertUpdate(
                        (string)this.Parameters["ServerName"],
                        (string)this.Parameters["DbProfile"],
                        (string)this.Parameters["UserName"],
                        (string)this.Parameters["Password"],
                        (XmlDocument)this.Parameters["Data"]
                        );
                    break;

                default:
                    throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
            }
        }

        #endregion
    }
}
