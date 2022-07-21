using System;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Rule
{
    public interface IXsltFunctionContainer
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

    public class LegacyXsltFunctionContainer : IXsltFunctionContainer
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

        public string Test()
        {
            return "test";
        }
    }
}