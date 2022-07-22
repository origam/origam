using System;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Rule;

public abstract class AbstractOrigamDependentXsltFunctionContainer: AbstractXsltFunctionContainer, IOrigamDependentXsltFunctionContainer
{
    public IPersistenceService Persistence { get; set; }
    public IDataLookupService LookupService { get; set; }
    public IParameterService ParameterService { get; set; }
    public IBusinessServicesService BusinessService { get; set; }
    public IStateMachineService StateMachineService { get; set; }
    public ITracingService TracingService { get; set; }
    public IDocumentationService DocumentationService { get; set; }
    public IOrigamAuthorizationProvider AuthorizationProvider { get; set; }
    public Func<UserProfile> UserProfileGetter { get; set; }
    public string XslNameSpacePrefix { get; set; }
    public string XslNameSpaceUri { get; set; }
}