using Origam.Workbench.Services;

namespace Origam.Server.Common;

public class FeatureTools
{
    public static bool IsFeatureOn(string featureCode)
    {
        return ServiceManager.Services
            .GetService<IParameterService>()
            .IsFeatureOn(featureCode);
    }
}