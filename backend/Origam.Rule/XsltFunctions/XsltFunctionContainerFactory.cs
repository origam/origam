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
using System.Collections.Generic;
using System.Linq;
using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Rule.XsltFunctions;

public static class XsltFunctionContainerFactory
{
    public static IEnumerable<XsltFunctionsDefinition> Create(string transactionId = null)
    {
        var businessServicesService =
            ServiceManager.Services.GetService<IBusinessServicesService>();
        return Create(
            businessServicesService,
            ServiceManager
                .Services.GetService<SchemaService>()
                .GetProvider<XsltFunctionSchemaItemProvider>(),
            ServiceManager.Services.GetService<IPersistenceService>(),
            ServiceManager.Services.GetService<IDataLookupService>(),
            ServiceManager.Services.GetService<IParameterService>(),
            ServiceManager.Services.GetService<IStateMachineService>(),
            ServiceManager.Services.GetService<ITracingService>(),
            ServiceManager.Services.GetService<IDocumentationService>(),
            DataService.Instance,
            SecurityManager.GetAuthorizationProvider(),
            SecurityManager.CurrentUserProfile,
            XpathEvaluator.Instance,
            HttpTools.Instance,
            new ResourceTools(businessServicesService, SecurityManager.CurrentUserProfile),
            transactionId
        );
    }

    // the method is public because of tests
    public static IEnumerable<XsltFunctionsDefinition> Create(
        IBusinessServicesService businessService,
        IXsltFunctionSchemaItemProvider xsltFunctionSchemaItemProvider,
        IPersistenceService persistence,
        IDataLookupService lookupService,
        IParameterService parameterService,
        IStateMachineService stateMachineService,
        ITracingService tracingService,
        IDocumentationService documentationService,
        ICoreDataService dataService,
        IOrigamAuthorizationProvider authorizationProvider,
        Func<UserProfile> userProfileGetter,
        IXpathEvaluator xpathEvaluator,
        IHttpTools httpTools,
        IResourceTools resourceTools,
        string transactionId
    )
    {
        return xsltFunctionSchemaItemProvider
            .ChildItemsByType<XsltFunctionCollection>(XsltFunctionCollection.CategoryConst)
            .Select(collection =>
            {
                object container = Reflector.InvokeObject(
                    collection.FullClassName,
                    collection.AssemblyName
                );
                if (container is IOrigamDependentXsltFunctionContainer origamContainer)
                {
                    origamContainer.Persistence = persistence;
                    origamContainer.LookupService = lookupService;
                    origamContainer.ParameterService = parameterService;
                    origamContainer.StateMachineService = stateMachineService;
                    origamContainer.TracingService = tracingService;
                    origamContainer.DocumentationService = documentationService;
                    origamContainer.AuthorizationProvider = authorizationProvider;
                    origamContainer.UserProfileGetter = userProfileGetter;
                    origamContainer.BusinessService = businessService;
                    origamContainer.DataService = dataService;
                    origamContainer.XpathEvaluator = xpathEvaluator;
                    origamContainer.HttpTools = httpTools;
                    origamContainer.ResourceTools = resourceTools;
                    origamContainer.TransactionId = transactionId;
                }
                return new XsltFunctionsDefinition(
                    Container: container,
                    NameSpacePrefix: collection.XslNameSpacePrefix,
                    NameSpaceUri: collection.XslNameSpaceUri
                );
            });
    }
}

public record XsltFunctionsDefinition(
    object Container,
    string NameSpacePrefix,
    string NameSpaceUri
);
