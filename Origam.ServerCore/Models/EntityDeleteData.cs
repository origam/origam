using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityDeleteData
    {
        [Required]
        public Guid DataStructureEntityId { get; set; }
        [Required]
        public Guid RowIdToDelete { get; set; }
    }
}
