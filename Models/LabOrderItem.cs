using System.ComponentModel.DataAnnotations; // Not strictly needed for 'required' in .NET 8+

namespace LABOGRA.Models
{
    // هذا هو تعريف كلاس LabOrderItem (عنصر تحليل فردي ضمن طلب معين).
    // يمثل تحليلاً محدداً تم طلبه ضمن LabOrder، وسيحتوي لاحقاً على نتيجته.
    public class LabOrderItem
    {
        // المفتاح الأساسي (Primary Key)
        public int Id { get; set; }

        // مفتاح خارجي لربط هذا العنصر بطلب التحاليل الأب.
        public int LabOrderId { get; set; }

        // خاصية الربط لطلب التحاليل الأب.
        public LabOrder LabOrder { get; set; } = null!; // 'null!' تخبر الكومبايلر أنه لن يكون Null

        // مفتاح خارجي لربط هذا العنصر بنوع التحليل من قائمة التحاليل المتاحة.
        public int TestId { get; set; }

        // خاصية الربط لنوع التحليل من قائمة التحاليل المتاحة.
        public Test Test { get; set; } = null!; // 'null!' تخبر الكومبايلر أنه لن يكون Null

        // قيمة النتيجة لهذا التحليل. يمكن أن تكون Null في البداية قبل إدخال النتيجة.
        public string? Result { get; set; }

        // وحدة قياس النتيجة لهذا التحليل. يمكن أن تكون Null في البداية.
        public string? ResultUnit { get; set; }

        // القيم المرجعية لهذا التحليل بناءً على بيانات المريض. يمكن أن تكون Null في البداية.
        public string? ReferenceRange { get; set; }

        // مؤشر لتتبع ما إذا كانت نتيجة هذا العنصر قد تم طباعتها أم لا.
        public bool IsPrinted { get; set; } = false; // القيمة الافتراضية هي False

        // ملاحظة: يمكن إضافة خصائص أخرى هنا لاحقاً (مثل الملاحظات، تاريخ إدخال النتيجة، إلخ).
    }
}