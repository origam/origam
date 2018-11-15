using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityUpdateData
    {
        public Guid DataStructureEntityId { get; set; }
        public Guid RowId { get; set; }
        public Dictionary<string,string> NewValues { get; set; }
        public IEnumerable<string> ColumnNames => NewValues.Keys;
    }
}
