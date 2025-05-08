using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LABOGRA.Models
{
    public class TestReferenceValue
    {
        public int Id { get; set; } // المفتاح الأساسي

        public int TestId { get; set; } // مفتاح خارجي لربطه بجدول Test
        [ForeignKey("TestId")]
        public virtual Test Test { get; set; } = null!; // خاصية الربط

        // لتحديد ما إذا كانت القيمة خاصة بجنس معين أو عامة (مثل: "Male", "Female", "Any")
        public string? GenderSpecific { get; set; }

        // يمكنك إضافة خصائص للعمر لاحقاً إذا أردت قيم مرجعية حسب العمر
        // public int? MinAge { get; set; }
        // public int? MaxAge { get; set; }
        // public string? AgeUnit { get; set; } // "Day", "Month", "Year"

        // النص الذي سيعرض للمستخدم كقيمة مرجعية
        // مثال: "13.5 – 17.5" أو "< 140" أو "Negative at < 1:40"
        public required string ReferenceText { get; set; }

        // وحدة القياس الخاصة بهذه القيمة المرجعية (قد تكون مفيدة إذا اختلفت عن وحدة التحليل الرئيسية)
        public string? Unit { get; set; }

        // ملاحظات إضافية على هذه القيمة المرجعية (اختياري)
        public string? Notes { get; set; }
    }
}