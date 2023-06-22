using System.Data;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Origam.Server
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://asapenginewebapi.advantages.cz/", ConfigurationName="DataServiceSoap")]
    public interface IDataServiceSoap
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/LoadData", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> LoadDataAsync(string dataStructureId, string filterId, string defaultSetId, string sortSetId, Parameter[] parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/LoadData0", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> LoadData0Async(string dataStructureId, string filterId, string sortSetId, string defaultSetId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/LoadData1", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> LoadData1Async(string dataStructureId, string filterId, string defaultSetId, string sortSetId, string paramName1, string paramValue1);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/LoadData2", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> LoadData2Async(string dataStructureId, string filterId, string defaultSetId, string sortSetId, string paramName1, string paramValue1, string paramName2, string paramValue2);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/ExecuteProcedure", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> ExecuteProcedureAsync(string procedureName, Parameter[] parameters);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/StoreData", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> StoreDataAsync(string dataStructureId, [XmlElement(ElementName = "data")]DataSet data, bool loadActualValuesAfterUpdate);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/StoreXml", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
        System.Threading.Tasks.Task<DataSet> StoreXmlAsync(string dataStructureId, System.Xml.XmlNode xml, bool loadActualValuesAfterUpdate);
    }
}