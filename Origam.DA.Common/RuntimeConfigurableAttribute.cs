using System;

namespace Origam.DA
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RuntimeConfigurableAttribute: Attribute
    {
        public string Name { get; }
        public RuntimeConfigurableAttribute(string name)
        {
            Name = name;
        }
    }
}