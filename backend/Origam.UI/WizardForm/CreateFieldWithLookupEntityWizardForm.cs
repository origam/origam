using System.Collections.Generic;
using System.ComponentModel;

namespace Origam.UI.WizardForm;

public class CreateFieldWithLookupEntityWizardForm : AbstractWizardForm
{
    public bool AllowNulls { get; set; }
    public string NameFieldName { get; set; }
    public string NameFieldCaption { get; set; }
    public string KeyFieldName { get; set; }
    public string KeyFieldCaption { get; set; }
    public bool TwoColumns { get; set; }

    private BindingList<InitialValue> _initialValues = new BindingList<InitialValue>();
    public IList<InitialValue> InitialValues
    {
        get
        {
                return _initialValues;
            }
    }
    public string LookupCaption { get; set; }
    public string LookupName { get; set; }
    public InitialValue DefaultInitialValue
    {
        get
        {
                foreach (var item in InitialValues)
                {
                    if (item.IsDefault)
                    {
                        return item;
                    }
                }
                return null;
            }
    }

    public bool ForceTwoColumns { get; set; }
    public string EnterAllInfo { get; set; }
    public string LookupWiz { get; set; }
    public string DefaultValueNotSet { get; set; }

    public class InitialValue
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
}