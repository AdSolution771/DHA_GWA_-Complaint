using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models
{
    public class ComplaintCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
