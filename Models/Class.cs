using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AQUA.Models;

public class Class
{
    [Key]
    public int ClassId { get; set; }

    [Required]
    [StringLength(100)]
    public string ClassName { get; set; }

    [Required]
    [StringLength(50)]
    public string Level { get; set; } // Beginner, Intermediate, Advanced

    [Required]
    [Range(1, 100)]
    public int MaxCapacity { get; set; }

    [Required]
    [Range(0, 1000)]
    [DataType(DataType.Currency)]
    public decimal Fee { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<Schedule> Schedules { get; set; }
    public Class()
    {
        Schedules = new HashSet<Schedule>(); // Initialize the collection
    }
}