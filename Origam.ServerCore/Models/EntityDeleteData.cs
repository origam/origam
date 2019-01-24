using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityDeleteData
    {
        [RequireNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [RequireNonDefault]
        public Guid RowIdToDelete { get; set; }
        [RequireNonDefault]
        public Guid MenuId { get; set; }
    }
}
