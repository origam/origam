using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class LookupLabelsData
    {
        public Guid LookupId { get; set; }
        public Guid[] LabelIds { get; set; }
    }
}
