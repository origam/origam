using Origam.UI.Commands;
using Origam.UI.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.UI.WizardForm
{
    public class AbstractWizardForm : IWizardForm
    {
        public ArrayList ItemTypeList { get ; set ; }
        public  Stack Pages { get; set; }
        public  string Description { get ; set ; }
        public  List<string> StructureList { get ; set ; }
        public  string NameOfEntity { get ; set ; }
        public ImageList ImageList { get; set ; }
        public IRunCommand Command { get; set; }
        public string Title { get; set; }
        public string PageTitle { get; set; }

        public bool IsExistsNameInDataStructure(string name)
        {
            return StructureList.Contains(name);
        }

        public void ListView(ListView listView)
        {
            if (listView.Items.Count == 0)
            {
                foreach (ListViewItem item in ItemTypeList)
                {
                    item.ImageIndex = ImageList.ImageIndex(item.ImageKey);
                    listView.Items.Add(item);
                }
            }
        }
    }
    public enum PagesList
    {
        StartPage,
        StructureNamePage,
        ScreenForm,
        LookupForm,
        FieldLookup,
        FieldEntity,
        MenuPage,
        ChildEntity,
        ForeignForm,
        SummaryPage,
        Finish
    }
}
