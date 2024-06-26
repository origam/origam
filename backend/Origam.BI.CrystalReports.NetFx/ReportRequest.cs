using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Origam.CrystalReportsService.Models;
[DataContract(Namespace = "")]
public class ReportRequest
{
    [DataMember()]
    public string Data
    {
        get
        {
            return Dataset.GetXml();
        }
    }
    public DataSet Dataset { get; set; }
    [DataMember()]
    public List<Parameter> Parameters { get; set; } = new List<Parameter>();
}
