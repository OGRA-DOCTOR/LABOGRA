using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations; // ليس ضرورياً لـ required إذا كان نوع المشروع يدعمه

namespace LABOGRA.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string? Title { get; set; } // يمكن أن يكون null
        public required string Name { get; set; }
        public string? MedicalRecordNumber { get; set; } // يمكن أن يكون null
        public string? Email { get; set; } // يمكن أن يكون null
        public string? Address { get; set; } // يمكن أن يكون null

        public required string Gender { get; set; }
        public int AgeValue { get; set; }
        public required string AgeUnit { get; set; }
        public string? PhoneNumber { get; set; }
        public required DateTime RegistrationDateTime { get; set; }

        public int? ReferringPhysicianId { get; set; }
        public ReferringPhysician? ReferringPhysician { get; set; }

        public virtual ICollection<LabOrderA> LabOrders { get; set; } = new List<LabOrderA>();
    }
}