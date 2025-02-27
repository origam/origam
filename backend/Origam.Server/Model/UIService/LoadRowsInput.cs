using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Origam.Server.Attributes;

namespace Origam.Server.Model.UIService;

public class LoadRowsInput
{
    [RequiredNonDefault]
    public Guid SessionFormIdentifier { get; set; }
    [Required]
    public string Entity { get; set; }
    public List<Guid> RowIds { get; set; }
}