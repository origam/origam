using System;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Rule;

public interface IOrigamDependentXsltFunctionContainer: IXsltFunctionContainer
{
    IPersistenceService Persistence { get; set; }
    IDataLookupService LookupService { get; set; }
    IParameterService ParameterService { get; set; }
    IBusinessServicesService BusinessService { get; set; }
    IStateMachineService StateMachineService { get; set; }
    ITracingService TracingService { get; set; }
    IDocumentationService DocumentationService { get; set; }
    IOrigamAuthorizationProvider AuthorizationProvider { get; set; }
    Func<UserProfile> UserProfileGetter { get; set; }

}