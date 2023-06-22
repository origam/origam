using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Origam.Server.Controller
{
    public class DeleteObjectInOrderedListInput
    {
        [Required]
        public string SessionFormIdentifier { set; get; }
        [Required]
        public string Entity { set; get; } 
        [Required]
        public object Id { set; get; } 
        [Required]
        public string OrderProperty { set; get; }
        public Hashtable UpdatedOrderValues { set; get; }
    }
}