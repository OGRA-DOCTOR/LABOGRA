using System.ComponentModel.DataAnnotations;

namespace LABOGRA.Models
{
    public class LabOrderItem
    {
        public int Id { get; set; }
        public int LabOrderId { get; set; }
        // السطر التالي هو الذي تم تعديله
        public LabOrderA LabOrder { get; set; } = null!; // كان LabOrder وأصبح LabOrderA
        public int TestId { get; set; }
        public Test Test { get; set; } = null!;
        public string? Result { get; set; }
        public string? ResultUnit { get; set; }
        public string? ReferenceRange { get; set; }
        public bool IsPrinted { get; set; } = false;
    }
}