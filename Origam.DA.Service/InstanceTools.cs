using System;
using System.Xml;
using Origam.DA.ObjectPersistence;

namespace Origam.DA.Service
{
    static class InstanceTools
    {
        public static object GetCorrectlyTypedValue(Type memberType, object value)
        {
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
}