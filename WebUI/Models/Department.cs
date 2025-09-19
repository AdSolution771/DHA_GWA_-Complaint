using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;  

public class Department
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
