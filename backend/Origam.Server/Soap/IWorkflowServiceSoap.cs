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
using System.ServiceModel;
using System.Threading.Tasks;

namespace Origam.Server
{
    [GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [ServiceContract(
        Namespace = "http://asapenginewebapi.advantages.cz/",
        ConfigurationName = "WorkflowServiceSoap"
    )]
    public interface IWorkflowServiceSoap
    {
        [OperationContract(
            Action = "http://asapenginewebapi.advantages.cz/ExecuteWorkflow0",
            ReplyAction = "*"
        )]
        [XmlSerializerFormat(SupportFaults = true)]
        Task<object> ExecuteWorkflow0Async(string workflowId);

        [OperationContract(
            Action = "http://asapenginewebapi.advantages.cz/ExecuteWorkflow",
            ReplyAction = "*"
        )]
        [XmlSerializerFormat(SupportFaults = true)]
        Task<object> ExecuteWorkflowAsync(string workflowId, Parameter[] parameters);

        [OperationContract(
            Action = "http://asapenginewebapi.advantages.cz/ExecuteWorkflow1",
            ReplyAction = "*"
        )]
        [XmlSerializerFormat(SupportFaults = true)]
        Task<object> ExecuteWorkflow1Async(string workflowId, string paramName, string paramValue);
    }
}
