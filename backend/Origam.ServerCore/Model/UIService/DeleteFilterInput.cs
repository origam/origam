using System;

namespace Origam.ServerCore.Model.UIService
{
    public class DeleteFilterInput
    {
        [RequiredNonDefault]
        public Guid FilterId { get; set; }
    }
}