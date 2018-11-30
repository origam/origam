using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityUpdateData
    {
        [Required]
        public Guid DataStructureEntityId { get; set; }
        [Required]
        public Guid RowId { get; set; }
        [Required]
        public Dictionary<string,string> NewValues { get; set; }
        public IEnumerable<string> ColumnNames => NewValues.Keys;
        [Required]
        public Guid MenuId { get; set; }
    }
}
