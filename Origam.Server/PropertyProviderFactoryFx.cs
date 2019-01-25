using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origam.Server
{
    class PropertyProviderFactoryFx: IPropertyProviderFactory
    {
        public IAdaptivePropertyProvider New(string propertyName, object propertyValue)
        {
            return new AdaptivePropertyProvider(propertyName, propertyValue);
        }
    }
}
