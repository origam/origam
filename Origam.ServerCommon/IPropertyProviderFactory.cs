using System;
using System.Collections.Generic;
using System.Text;

namespace Origam.Server
{
    public interface IPropertyProviderFactory
    {
        IAdaptivePropertyProvider New(string propertyName, object propertyValue);
    }

    public class StandardPropertyProviderFactory: IPropertyProviderFactory
    {
        public IAdaptivePropertyProvider New(string propertyName, object propertyValue)
        {
            return new NullPropertyProvider();
        }
    }
}
