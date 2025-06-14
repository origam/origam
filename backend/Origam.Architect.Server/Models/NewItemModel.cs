﻿using System.ComponentModel.DataAnnotations;

namespace Origam.Architect.Server.Models;

public class NewItemModel
{
    [Required]
    public string NodeId { get; set; }   
    
    [Required]
    public string NewTypeName { get; set; }
}