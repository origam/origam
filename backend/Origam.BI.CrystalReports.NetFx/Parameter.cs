using System.Runtime.Serialization;

namespace Origam.CrystalReportsService.Models;
[DataContract(Namespace = "")]
public class Parameter
{
    [DataMember]
    public string Key { get; set; }
    [DataMember]
    public string Value { get; set; }
}
