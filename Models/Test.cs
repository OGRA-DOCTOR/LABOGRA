using System.Collections.Generic; // Required for ICollection
using System.ComponentModel.DataAnnotations;

namespace LABOGRA.Models
{
    // هذا هو تعريف كلاس Test (نوع التحليل).
    // تم تحديثه ليرتبط بجدول القيم المرجعية الجديد TestReferenceValue.
    public class Test
    {
        // هذا هو المعرف الفريد لكل نوع تحليل.
        public int Id { get; set; }

        // هذا هو اسم نوع التحليل (مثال: Glucose, Creatinine). مطلوب.
        public required string Name { get; set; }

        // هذا هو الاختصار الشائع للتحليل (مثال: FBG, Cr). يمكن أن يكون Null.
        public string? Abbreviation { get; set; }

        // --- بداية الحقول التي تم إزالتها أو تعديلها ---
        // تم إزالة خاصية Unit من هنا، ستكون جزءاً من TestReferenceValue أو مشتركة بطريقة أخرى
        // public string? Unit { get; set; } 

        // تم إزالة خاصية ReferenceRange من هنا، ستدار عبر جدول TestReferenceValue
        // public string? ReferenceRange { get; set; }
        // --- نهاية الحقول التي تم إزالتها أو تعديلها ---

        // خاصية الربط (Navigation Property) لمجموعة القيم المرجعية لهذا التحليل.
        // كل تحليل يمكن أن يكون له عدة قيم مرجعية (مثلاً للذكور، للإناث، لأعمار مختلفة).
        public virtual ICollection<TestReferenceValue> ReferenceValues { get; set; } = new List<TestReferenceValue>();

        // ملاحظة: يمكن إضافة خصائص أخرى هنا لاحقاً مثل السعر، أو فئة التحليل.
        // public decimal? Price { get; set; }
        // public string? Category { get; set; } // مثل: Hematology, Chemistry, Hormones
    }
}