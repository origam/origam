using Origam.UI.Commands;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.UI.Interfaces;
public interface IWizardForm
{
    List<ListViewItem> ItemTypeList { get; set; }
    Stack Pages { get; set; }
    string Description { get; set; }
    string Title { get; set; }
    string PageTitle { get; set; }
    List<string> StructureList { get; set; }
    string NameOfEntity { get; set; }
    ImageList ImageList { get; set; }
    IRunCommand Command { get; set; }
    bool IsExistsNameInDataStructure(string name);
    void ListView(ListView listView);
}
