using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Model.UIService
{
    public class CopyObjectInput
    {
        public Guid SessionFormIdentifier { get; set; }
        public string Entity { get; set; }
        public Guid OriginalId { get; set; }
        public Guid RequestingGridId { get; set; }
        public ArrayList Entities  { get; set; }
        public IDictionary<string, object> ForcedValues { get; set; }
    }
}
