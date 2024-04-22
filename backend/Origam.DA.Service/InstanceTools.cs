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
using System.Reflection;
using System.Xml;

namespace Origam.DA.Service;

static class InstanceTools
{
    public static object GetCorrectlyTypedValue(MemberInfo memberInfo, object value)
    {
            Type memberType;
            if (memberInfo is PropertyInfo)
                memberType = (memberInfo as PropertyInfo).PropertyType;
            else
                memberType = (memberInfo as FieldInfo).FieldType;
            
            object correctlyTypedValue;
            // If member is enum, we have to convert
            if (memberType.IsEnum )
            {
                correctlyTypedValue = value == null ? null :
                    Enum.Parse(memberType, (string)value);
            }
            else if (memberType == typeof(int))
            {
                correctlyTypedValue = value == null ? 0 :
                    XmlConvert.ToInt32((string)value);
            }
            else if (memberType == typeof(bool))
            {
                correctlyTypedValue = value != null &&
                                      XmlConvert.ToBoolean((string)value);
            }
            else if (memberType == typeof(Guid))
            {
                if (value != null)
                {
                    correctlyTypedValue 
                        = value is Guid ? value : new Guid((string)value);
                }
                else
                {
                    correctlyTypedValue = null;
                }
            }
            else
            {
                correctlyTypedValue = value;
            }

            return correctlyTypedValue;
        }       
}