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

using System.Data;
using System.Xml.Serialization;

namespace Origam.Server;

[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
[System.ServiceModel.ServiceContractAttribute(
    Namespace = "http://asapenginewebapi.advantages.cz/",
    ConfigurationName = "DataServiceSoap"
)]
public interface IDataServiceSoap
{
    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/LoadData",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> LoadDataAsync(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        Parameter[] parameters
    );

    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/LoadData0",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> LoadData0Async(
        string dataStructureId,
        string filterId,
        string sortSetId,
        string defaultSetId
    );

    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/LoadData1",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> LoadData1Async(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        string paramName1,
        string paramValue1
    );

    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/LoadData2",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> LoadData2Async(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        string paramName1,
        string paramValue1,
        string paramName2,
        string paramValue2
    );

    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/ExecuteProcedure",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> ExecuteProcedureAsync(
        string procedureName,
        Parameter[] parameters
    );

    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/StoreData",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> StoreDataAsync(
        string dataStructureId,
        [XmlElement(ElementName = "data")] DataSet data,
        bool loadActualValuesAfterUpdate
    );

    [System.ServiceModel.OperationContractAttribute(
        Action = "http://asapenginewebapi.advantages.cz/StoreXml",
        ReplyAction = "*"
    )]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(Parameter[]))]
    System.Threading.Tasks.Task<DataSet> StoreXmlAsync(
        string dataStructureId,
        System.Xml.XmlNode xml,
        bool loadActualValuesAfterUpdate
    );
}
