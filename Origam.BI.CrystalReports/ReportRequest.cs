using Origam.BI.CrystalReports;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Origam.CrystalReportsService.Models
{
    [DataContract(Namespace = "")]
    public class ReportRequest
    {
        [DataMember()]
        public string Data
        {
            get
            {
                var stringBuilder = new StringBuilder();
                using (var stringWriter = new EncodingStringWriter(stringBuilder, Encoding.UTF8))
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter,
                    new XmlWriterSettings { Encoding = UTF8Encoding.UTF8 }))
                    {
                        Dataset.WriteXml(xmlWriter, XmlWriteMode.WriteSchema);
                    }
                }
                return stringBuilder.ToString();
            }
            set
            {

            }
        }

        public DataSet Dataset { get; set; }

        [DataMember()]
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }
}