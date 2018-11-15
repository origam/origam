using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityInsertData
    {
        public Guid DataStructureEntityId { get; set; }
        public Dictionary<string, string> NewValues { get; set; }
    }
}
