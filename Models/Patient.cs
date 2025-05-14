// الإصدار: 2 (لهذا الملف - إضافة GeneratedId)
// اسم الملف: Patient.cs
// الوصف: نموذج بيانات المريض مع إضافة حقل ID مولد تلقائيًا.
using System;
using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations; // ليس ضرورياً لـ required إذا كان نوع المشروع يدعمه

namespace LABOGRA.Models
{
    public class Patient
    {
        public int Id { get; set; } // هذا هو الـ ID التلقائي من قاعدة البيانات (Primary Key)

        // *** بداية الإضافة هنا ***
        // هذا هو الـ ID المولد تلقائيًا الذي طلبته (مثال: 2025051401)
        // سنجعله string لأنه سيحتوي على التاريخ والرقم التسلسلي.
        // يمكن أن يكون null في البداية قبل أن يتم توليده.
        public string? GeneratedId { get; set; }
        // *** نهاية الإضافة هنا ***

        public string? Title { get; set; }
        public required string Name { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

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