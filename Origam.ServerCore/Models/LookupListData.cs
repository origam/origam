using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class LookupListData 
    {
        [Required]
        public Guid DataStructureEntityId { get; set; }
        [Required]
        public string Property { get; set; }
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid LookupId { get; set; }
        //public IDictionary<string, object> Parameters { get; set; }
        [Required]
        public bool ShowUniqueValues { get; set; }
        public string SearchText { get; set; }
        [Required]
        [Range(0, 10_000)]
        public int PageSize { get; set; } = -1;
        [Required]
        [Range(0, 10_000)]
        public int PageNumber { get; set; } = -1;
    }
}
