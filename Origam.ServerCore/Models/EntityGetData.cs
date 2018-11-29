using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class EntityGetData
    {
        [Required]
        public Guid MenuId { get; set; }
        [Required]
        public Guid DataStructureEntityId { get; set; }
        public string Filter { get; set; }
        public List<List<string>> Ordering { get; set; }
        [Required]
        public int RowLimit { get; set; }
        [Required]
        public List<string> ColumnNames { get; set; }

        public Guid MasterRowId { get; set; }

        public List<Tuple<string, string>> OrderingAsTuples =>
            Ordering
                .Where(x=> x.Count > 0)
                .Select(x => new Tuple<string, string>(x[0], x[1]))
                .ToList();
    }
}
