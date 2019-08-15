using System;

namespace Origam.ServerCore.Model.UIService
{
    public class NewEmptyRowInput
    {
        [RequiredNonDefault]
        public Guid DataStructureEntityId { get; set; }
        [RequiredNonDefault]
        public Guid MenuId { get; set; }
    }
}