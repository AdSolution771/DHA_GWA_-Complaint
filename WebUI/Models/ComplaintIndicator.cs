using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Models;
public class ComplaintIndicator
{
    public int Id { get; set; }

    public int ComplaintId { get; set; }
    public Complaint Complaint { get; set; }

    [MaxLength(100)]
    public string IndicatorType { get; set; }  // Overdue, Escalated, SLA Breach

    public DateTime TriggerDate { get; set; }

    public int? NotifiedToId { get; set; }
    public User? NotifiedTo { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";
}
