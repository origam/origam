using System;
using System.ComponentModel.DataAnnotations;

namespace Origam.ServerCore.Model.UIService
{
    public class RevertChangesInput
    {
        [RequiredNonDefault] public Guid SessionFormIdentifier { get; set; }
    }
}