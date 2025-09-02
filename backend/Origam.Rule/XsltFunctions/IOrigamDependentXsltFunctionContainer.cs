#region license

/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using Origam.DA;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Rule.XsltFunctions;

public interface IOrigamDependentXsltFunctionContainer
{
    IPersistenceService Persistence { get; set; }
    IDataLookupService LookupService { get; set; }
    ICoreDataService DataService { get; set; }
    IParameterService ParameterService { get; set; }
    IBusinessServicesService BusinessService { get; set; }
    IStateMachineService StateMachineService { get; set; }
    ITracingService TracingService { get; set; }
    IDocumentationService DocumentationService { get; set; }
    IOrigamAuthorizationProvider AuthorizationProvider { get; set; }
    Func<UserProfile> UserProfileGetter { get; set; }
    IXpathEvaluator XpathEvaluator { get; set; }
    IHttpTools HttpTools { get; set; }
    IResourceTools ResourceTools { get; set; }
    string TransactionId { set; get; }
}
