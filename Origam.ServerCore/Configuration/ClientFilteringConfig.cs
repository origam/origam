using Origam.Server;

namespace Origam.ServerCore.Configuration
{
    public class ClientFilteringConfig: IFilteringConfig
    {
        public bool CaseSensitive { get; set; } = false;
        public bool AccentSensitive { get; set; } = true;
    }
}