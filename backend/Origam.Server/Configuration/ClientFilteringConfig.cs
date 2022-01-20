using Origam.Server;

namespace Origam.Server.Configuration
{
    public class ClientFilteringConfig: IFilteringConfig
    {
        public bool CaseSensitive { get; set; } = false;
        public bool AccentSensitive { get; set; } = true;
    }
}