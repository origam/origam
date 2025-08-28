#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.CodeDom.Compiler;
using System.Data;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Origam.Server;

[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
[ServiceContract(
    Namespace = "http://asapenginewebapi.advantages.cz/",
    ConfigurationName = "DataServiceSoap"
)]
public interface IDataServiceSoap
{
    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/LoadData",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> LoadDataAsync(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        Parameter[] parameters
    );

    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/LoadData0",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> LoadData0Async(
        string dataStructureId,
        string filterId,
        string sortSetId,
        string defaultSetId
    );

    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/LoadData1",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> LoadData1Async(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        string paramName1,
        string paramValue1
    );

    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/LoadData2",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> LoadData2Async(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        string paramName1,
        string paramValue1,
        string paramName2,
        string paramValue2
    );

    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/ExecuteProcedure",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> ExecuteProcedureAsync(string procedureName, Parameter[] parameters);

    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/StoreData",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> StoreDataAsync(
        string dataStructureId,
        [XmlElement(ElementName = "data")] DataSet data,
        bool loadActualValuesAfterUpdate
    );

    [OperationContract(
        Action = "http://asapenginewebapi.advantages.cz/StoreXml",
        ReplyAction = "*"
    )]
    [XmlSerializerFormat(SupportFaults = true)]
    [ServiceKnownType(typeof(Parameter[]))]
    Task<DataSet> StoreXmlAsync(
        string dataStructureId,
        XmlNode xml,
        bool loadActualValuesAfterUpdate
    );
}
