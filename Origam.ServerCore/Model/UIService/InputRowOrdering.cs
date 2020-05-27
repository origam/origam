using System;
using System.ComponentModel.DataAnnotations;

public class InputRowOrdering
{
    [Required] public string ColumnId { get; set; }

    [Required] public string Direction { get; set; }

    public Guid LookupId { get; set; }
}