#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using System;

namespace Origam.DA.ObjectPersistence;

/// <summary>
/// Summary description for NotNullModelElementRuleAttribute.
/// </summary>
[AttributeUsage(
    validOn: AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false,
    Inherited = true
)]
public class NotNullModelElementRuleAttribute : AbstractModelElementRuleAttribute
{
    private string _conditionField = null;
    private string _conditionField2 = null;
    private object _conditionValue = null;
    private object _conditionValue2 = null;

    public NotNullModelElementRuleAttribute() { }

    /// <summary>
    /// Will raise an exception when conditionField is not empty and the checked field is empty.
    /// </summary>
    /// <param name="conditionField">Field that will be checked. If empty, no checking will be done. If this field is filled, the checked field will be checked if it is empty.</param>
    public NotNullModelElementRuleAttribute(string conditionField)
    {
        _conditionField = conditionField;
    }

    public NotNullModelElementRuleAttribute(string conditionField1, string conditionField2)
    {
        _conditionField = conditionField1;
        _conditionField2 = conditionField2;
    }

    public NotNullModelElementRuleAttribute(
        string conditionField1,
        string conditionField2,
        object conditionValue1,
        object conditionValue2
    )
    {
        _conditionField = conditionField1;
        _conditionField2 = conditionField2;
        _conditionValue = conditionValue1;
        _conditionValue2 = conditionValue2;
    }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(
            message: ResourceUtils.GetString(key: "MemberNameRequired")
        );
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (memberName == String.Empty || memberName == null)
        {
            CheckRule(instance: instance);
        }

        object value = Reflector.GetValue(
            type: instance.GetType(),
            instance: instance,
            memberName: memberName
        );
        object field1Value = null;
        object field2Value = null;
        if (_conditionField != null)
        {
            field1Value = Reflector.GetValue(
                type: instance.GetType(),
                instance: instance,
                memberName: _conditionField
            );
        }
        if (_conditionField2 != null)
        {
            field2Value = Reflector.GetValue(
                type: instance.GetType(),
                instance: instance,
                memberName: _conditionField2
            );
        }
        if (value == null || (value as string) == string.Empty)
        {
            if (
                _conditionField == null
                || (_conditionField != null && _conditionValue == null && field1Value != null)
                || (_conditionField2 != null && _conditionValue2 == null && field2Value != null)
                || (
                    _conditionField != null
                    && _conditionValue != null
                    && field1Value.Equals(obj: _conditionValue)
                )
                || (
                    _conditionField2 != null
                    && _conditionValue2 != null
                    && field2Value.Equals(obj: _conditionValue2)
                )
            )
            {
                return new NullReferenceException(
                    message: ResourceUtils.GetString(key: "CantBeNull", args: memberName)
                );
            }
        }
        return null;
    }
}
