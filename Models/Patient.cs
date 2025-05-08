using System; // Needed for DateTime
using System.Collections.Generic; // Needed for ICollection
using System.ComponentModel.DataAnnotations; // Not strictly needed for 'required' in .NET 8+, but good practice

namespace LABOGRA.Models
{
    // هذا هو تعريف كلاس Patient (المريض) مع الحقول الجديدة المطلوبة.
    public class Patient
    {
        // المفتاح الأساسي (Primary Key)
        public int Id { get; set; }

        // اللقب (مثال: السيد، دكتور) - يمكن إدخاله يدوياً أو اختياره.
        // نستخدم 'required' لأن اللقب مطلوب عند الإدخال الأولي.
        public required string Title { get; set; }

        // الاسم - مطلوب.
        public required string Name { get; set; }

        // النوع (Male/Female) - مطلوب.
        public required string Gender { get; set; }

        // قيمة العمر (الرقم) - مطلوب.
        public required int AgeValue { get; set; }

        // وحدة العمر (يوم/شهر/سنة) - مطلوب.
        public required string AgeUnit { get; set; }

        // الرقم الطبي - ليس مطلوباً دائماً، لذا يمكن أن يكون Null.
        public string? MedicalRecordNumber { get; set; }

        // رقم المحمول - ليس مطلوباً دائماً، لذا يمكن أن يكون Null.
        public string? PhoneNumber { get; set; }

        // البريد الإلكتروني - ليس مطلوباً دائماً، لذا يمكن أن يكون Null.
        public string? Email { get; set; }

        // تاريخ ووقت تسجيل المريض في البرنامج - يتم حفظه تلقائياً، مطلوب.
        public required DateTime RegistrationDateTime { get; set; }

        // مفتاح خارجي (Foreign Key) لربط المريض بجهة التحويل (الطبيب).
        // 'int?' تعني أنه يمكن أن يكون Null إذا لم يتم تحديد طبيب محول.
        public int? ReferringPhysicianId { get; set; }

        // خاصية الربط (Navigation Property) للطبيب المحول.
        // 'ReferringPhysician?' تعني أنه يمكن أن يكون Null.
        public ReferringPhysician? ReferringPhysician { get; set; }

        // خاصية الربط (Navigation Property) لمجموعة طلبات التحاليل لهذا المريض.
        // ICollection تسمح بوجود أكثر من طلب تحليل لنفس المريض مع مرور الوقت.
        // '= new List<LabOrder>();' هو ممارسة جيدة لتهيئة المجموعة لتجنب Null Reference Exceptions.
        public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
    }
}