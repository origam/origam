using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class LookupLabelsData
    {
        [Required]
        public Guid LookupId { get; set; }
        [Required]
        public Guid[] LabelIds { get; set; }
        [Required]
        public Guid MenuId { get; set; }
    }
}
