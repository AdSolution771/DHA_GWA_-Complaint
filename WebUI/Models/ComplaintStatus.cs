using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;

public class ComplaintStatus
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}
