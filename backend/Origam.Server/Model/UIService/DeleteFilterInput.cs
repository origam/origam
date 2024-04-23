using System;
using Origam.Server.Attributes;

namespace Origam.Server.Model.UIService;

public class DeleteFilterInput
{
    [RequiredNonDefault]
    public Guid FilterId { get; set; }
}