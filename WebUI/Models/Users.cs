using System;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Password { get; set; }  

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }  

        [MaxLength(15)]
        public string? CNIC { get; set; }     

        [MaxLength(50)]
        public string? UserType { get; set; }  // e.g., Resident, Admin, etc.

        [MaxLength(50)]

        public string Status { get; set; } = "pending";


        public bool IsApproved { get; set; } 


        public int Type { get; set; } = 0;  // default value 0
        public string PlotNumber { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
