namespace Origam.Architect.Server.Wrappers;

public class ConfigManager
{
    public OrigamSettings ActiveConfiguration
    {
        get => ConfigurationManager.GetActiveConfiguration();
        set => ConfigurationManager.SetActiveConfiguration(value);
    }
    
    public IEnumerable<string> Available => ConfigurationManager
        .GetAllConfigurations()
        .Cast<OrigamSettings>()
        .Select(x => x.Name);
    
    

}