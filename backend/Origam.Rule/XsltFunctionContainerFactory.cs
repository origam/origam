using System;
using System.Collections.Generic;
using System.Linq;
using Origam.DA;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Rule;

public static class XsltFunctionContainerFactory
{
    public static IEnumerable<IXsltFunctionContainer> Create()
    {
        return Create(
            ServiceManager.Services.GetService<IBusinessServicesService>(),
            ServiceManager.Services.GetService<SchemaService>()
                .GetProvider<XsltFunctionSchemaItemProvider>(),
            ServiceManager.Services.GetService<IPersistenceService>(),
            ServiceManager.Services.GetService<IDataLookupService>(),
            ServiceManager.Services.GetService<IParameterService>(),
            ServiceManager.Services.GetService<IStateMachineService>(),
            ServiceManager.Services.GetService<ITracingService>(),
            ServiceManager.Services.GetService<IDocumentationService>(),
            SecurityManager.GetAuthorizationProvider(),
            SecurityManager.CurrentUserProfile);
    }

    public static IEnumerable<IXsltFunctionContainer> Create (
        IBusinessServicesService businessService,
        IXsltFunctionSchemaItemProvider xsltFunctionSchemaItemProvider,
        IPersistenceService persistence, IDataLookupService lookupService,
        IParameterService parameterService, IStateMachineService stateMachineService ,
        ITracingService tracingService, IDocumentationService documentationService,
        IOrigamAuthorizationProvider authorizationProvider, Func<UserProfile> userProfileGetter)
    {
        return xsltFunctionSchemaItemProvider
            .ChildItemsByType(XsltFunctionCollection.CategoryConst)
            .Cast<XsltFunctionCollection>()
            .Select(collection =>
            {
                object instantiatedObject = Reflector.InvokeObject(
                    collection.FullClassName,
                    collection.AssemblyName);
                if (!(instantiatedObject is IXsltFunctionContainer container))
                {
                    throw new Exception(
                        $"Referenced class {collection.FullClassName} from {collection.AssemblyName} does not implement interface {nameof(IXsltFunctionContainer)}");
                }

                container.Persistence = persistence;
                container.LookupService = lookupService;
                container.ParameterService = parameterService;
                container.StateMachineService = stateMachineService;
                container.TracingService = tracingService;
                container.DocumentationService = documentationService;
                container.AuthorizationProvider = authorizationProvider;
                container.UserProfileGetter = userProfileGetter;
                container.BusinessService = businessService;
                if (!string.IsNullOrWhiteSpace(collection.XslNameSpacePrefix))
                {
                    container.XslNameSpacePrefix = collection.XslNameSpacePrefix;
                }
                if (!string.IsNullOrWhiteSpace(collection.XslNameSpaceUri))
                {
                    container.XslNameSpaceUri = collection.XslNameSpaceUri;
                }
                return container;
            });
    }
}