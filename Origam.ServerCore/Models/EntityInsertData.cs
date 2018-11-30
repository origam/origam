using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityInsertData
    {
        [Required]
        public Guid DataStructureEntityId { get; set; }
        [Required]
        public Dictionary<string, string> NewValues { get; set; }
        [Required]
        public Guid MenuId { get; set; }
    }
}
