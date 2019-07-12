using Origam.UI.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.UI.WizardForm
{
    public class AbstractWizardForm : IWizardForm
    {
        public ArrayList listItemType { get ; set ; }
        public  Stack Pages { get; set; }
        public  string Description { get ; set ; }
        public  List<string> ListDatastructure { get ; set ; }
        public  string NameOfEntity { get ; set ; }
        public ImageList imgList { get; set ; }

        public bool IsExistsNameInDataStructure(string name)
        {
            return ListDatastructure.Contains(name);
        }

        public void ListView(ListView listView)
        {
            foreach (ListViewItem item in listItemType)
            {
                listView.Items.Add(item);
            }
        }
    }
    public enum PagesList
    {
        startPage,
        DatastructureNamePage,
        ScreenForm,
        LookupForm,
        finish
    }
}
