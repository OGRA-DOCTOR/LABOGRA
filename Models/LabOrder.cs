using System; // Needed for DateTime
using System.Collections.Generic; // Needed for ICollection
using System.ComponentModel.DataAnnotations; // Not strictly needed for 'required' in .NET 8+

namespace LABOGRA.Models
{
    // هذا هو تعريف كلاس LabOrder (طلب التحاليل المحدد لزيارة معينة لمريض).
    // يربط المريض بمجموعة من التحاليل التي تم طلبها له في وقت معين.
    public class LabOrder
    {
        // المفتاح الأساسي (Primary Key)
        public int Id { get; set; }

        // مفتاح خارجي (Foreign Key) لربط هذا الطلب بالمريض الذي يخصه.
        public int PatientId { get; set; }

        // خاصية الربط (Navigation Property) للمريض صاحب الطلب.
        public Patient Patient { get; set; } = null!; // 'null!' تخبر الكومبايلر أنه لن يكون Null عند الاستخدام في EF Core

        // تاريخ ووقت تسجيل الطلب (يمكن أن يكون هو نفسه تاريخ تسجيل المريض للطلب الأول).
        public required DateTime RegistrationDateTime { get; set; }

        // مفتاح خارجي للطبيب المحول لهذا الطلب المحدد (قد يكون مختلفاً عن الطبيب المحول للمريض نفسه في بياناته الأساسية).
        // يمكن أن يكون Null.
        public int? ReferringPhysicianId { get; set; }

        // خاصية الربط للطبيب المحول لهذا الطلب. يمكن أن يكون Null.
        public ReferringPhysician? ReferringPhysician { get; set; }

        // خاصية الربط لمجموعة عناصر الطلب (التحاليل الفردية ضمن هذا الطلب).
        public ICollection<LabOrderItem> Items { get; set; } = new List<LabOrderItem>();

        // ملاحظة: يمكن إضافة خصائص أخرى هنا لاحقاً تتعلق بالطلب ككل (خصومات، حالة الدفع، إلخ).
    }
}