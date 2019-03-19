using System;
using System.Collections.Generic;

namespace Origam.ServerCore.Models
{
    public class NewRowData
    {
        public Guid SessionFormIdentifier { get; set; }
        public string Entity { get; set; }
        public IDictionary<string, object> Values { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public Guid  RequestingGridId { get; set; }
    }
}