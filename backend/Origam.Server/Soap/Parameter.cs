using System.Linq;
using System.Xml.Serialization;
using Origam.DA;
using Origam.DA.Common.Extensiosn;

namespace Origam.Server;
/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[XmlType(TypeName = "parameter",Namespace = "http://asapenginewebapi.advantages.cz/")]
public partial class Parameter
{

    private object valueField;

    private string nameField;

    /// <remarks/>
    [XmlElement(Order=0)]
    public object value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

public static class ParameterUtils
{
    public static QueryParameterCollection ToQueryParameterCollection(Parameter[] parameters)
    {
        return (parameters ?? new Parameter[0])
            .Select(x => new QueryParameter(x.name, x.value))
            .ToQueryParameterCollection();
    }
}
