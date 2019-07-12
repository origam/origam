using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.UI.Interfaces
{
    public interface IWizardForm
    {
        ArrayList listItemType { get; set; }
        Stack Pages { get; set; }
        string Description { get; set; }
        List<string> ListDatastructure { get; set; }
        string NameOfEntity { get; set; }
        ImageList imgList { get; set; }

        bool IsExistsNameInDataStructure(string name);
        void ListView(ListView listView);
    }
}
