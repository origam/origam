using Origam.UI.Interfaces;
using System;
using System.Collections;


namespace Origam.UI.WizardForm
{
    public class ScreenWizardForm : IWizardForm
    {
        public ArrayList listItemType { get  ; set  ; }
        public Stack Pages { get  ; set  ; }
        public string Description { get  ; set  ; }


    }
}
