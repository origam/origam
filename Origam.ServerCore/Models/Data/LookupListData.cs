using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class LookupListData 
    {
        [RequireNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [Required]
        public string[] ColumnNames { get; set; }
        [Required]
        public string Property { get; set; }
        [RequireNonDefault]
        public Guid Id { get; set; }
        [RequireNonDefault]
        public Guid LookupId { get; set; }
        //public IDictionary<string, object> Parameters { get; set; }
        public bool ShowUniqueValues { get; set; }
        public string SearchText { get; set; }
        [Range(-1, 10_000)]
        public int PageSize { get; set; } = -1;
        [Range(1, 10_000)]
        public int PageNumber { get; set; } = -1;
        [RequireNonDefault]
        public Guid MenuId { get; set; }
    }
}
