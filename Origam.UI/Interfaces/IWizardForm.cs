using System.Collections;

namespace Origam.UI.Interfaces
{
    public interface IWizardForm
    {
        ArrayList listItemType { get; set; }
        Stack Pages { get; set; }
        string Description { get; set; }
    }
}
