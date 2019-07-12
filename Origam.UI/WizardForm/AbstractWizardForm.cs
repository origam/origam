using Origam.UI.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Origam.UI.WizardForm
{
    public class AbstractWizardForm : IWizardForm
    {
        public ArrayList listItemType { get ; set ; }
        public  Stack Pages { get; set; }
        public  string Description { get ; set ; }
        public  List<string> ListDatastructure { get ; set ; }
        public  string NameOfEntity { get ; set ; }

        public bool IsExistsNameInDataStructure(string name)
        {
            return ListDatastructure.Contains(name);
        }
    }
}
