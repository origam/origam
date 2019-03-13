using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class UpdateRowData
    {
        public Guid SessionFormIdentifier { get; set; }
        public string Entity { get; set; }
        public Guid Id { get; set; }
        public string Property { get; set; }
        public string NewValue { get; set; }
    }
}
