using System.Collections.Generic; // يمكن استخدامه لاحقاً
using System.ComponentModel.DataAnnotations; // Not strictly needed for 'required' in .NET 8+

namespace LABOGRA.Models
{
    // هذا هو تعريف كلاس ReferringPhysician (الطبيب المحول أو جهة التحويل).
    public class ReferringPhysician
    {
        // المفتاح الأساسي (Primary Key)
        public int Id { get; set; }

        // لقب الطبيب (مثال: د., أستاذ.) - يمكن أن يكون Null.
        public string? Title { get; set; }

        // اسم الطبيب - مطلوب.
        public required string Name { get; set; }

        // ملاحظة: يمكن إضافة خصائص أخرى هنا لاحقاً (مثل العنوان، الهاتف).
        // يمكن إضافة خاصية ربط عكسية للمرضى الذين حولهم هذا الطبيب إذا لزم الأمر:
        // public ICollection<Patient> Patients { get; set; } = new List<Patient>();
        // public ICollection<LabOrder> LabOrders { get; set; } = new List<LabOrder>();
    }
}