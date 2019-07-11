using System.Collections;
using System.Collections.Generic;
using Origam.UI.Interfaces;

namespace Origam.UI.WizardForm
{
    public class DataStructureForm : IWizardForm
    {
        public ArrayList listItemType { get; set; }
        public Stack Pages { get; set; }
        public string Description { get; set; }
        public List<string> ListDatastructure { get; set; }
        public string NameOfEntity { get; set; }

        public bool IsExistsName(string name)
        {
            return ListDatastructure.Contains(name);
        }
    }
}
