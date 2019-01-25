using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Origam.ServerCore.Models
{
    public class NewSessionData
    {
        [RequireNonDefault]
        public Guid MenuId { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
