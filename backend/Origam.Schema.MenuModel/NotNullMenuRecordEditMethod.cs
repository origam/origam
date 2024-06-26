using Origam.DA.ObjectPersistence;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Origam.Schema.MenuModel;
public class NotNullMenuRecordEditMethod : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }
    public override Exception CheckRule(object instance, string memberName)
    {
        if (memberName == String.Empty || memberName == null)
        {
            CheckRule(instance);
        }
        object value = Reflector.GetValue(instance.GetType(), instance, memberName);
        if (value is FormReferenceMenuItem formReferenceMenuItem)
        {
            if(formReferenceMenuItem.ListDataStructure != null &&
                formReferenceMenuItem.RecordEditMethod == null)
            {
                return new DataException("The target menu item has to have a property RecordEditMethod set.");
            }
        }
        return null;
    }
}
