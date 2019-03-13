using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityUpdateData
    {
        [RequireNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [RequireNonDefault]
        public Guid RowId { get; set; }
        [Required]
        public Dictionary<string,string> NewValues { get; set; }
        public IEnumerable<string> ColumnNames => NewValues.Keys;
        [RequireNonDefault]
        public Guid MenuId { get; set; }
    }
}
