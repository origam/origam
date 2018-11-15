using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityDeleteData
    {
        public Guid DataStructureEntityId { get; set; }
        public Guid RowIdToDelete { get; set; }
    }
}
