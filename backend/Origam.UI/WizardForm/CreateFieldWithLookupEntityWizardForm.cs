#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

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
