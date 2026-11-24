using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AQUA.Models
{
    public class Coach : UserBase
    {
        [StringLength(50)]
        public string Specialization { get; set; }

        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public int? CoachId { get; internal set; }
    }
}