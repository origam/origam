using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class LookupListData 
    {
        public Guid DataStructureEntityId { get; set; }
        public string Property { get; set; }
        public Guid Id { get; set; }
        public Guid LookupId { get; set; }
        //public IDictionary<string, object> Parameters { get; set; }
        public bool ShowUniqueValues { get; set; }
        public string SearchText { get; set; }
        public int PageSize { get; set; } = -1;
        public int PageNumber { get; set; } = -1;
        
    }
}
