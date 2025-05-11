using LABOGRA.Models;
using Microsoft.EntityFrameworkCore;

namespace LABOGRA.Services.Database.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions<LabDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<ReferringPhysician> ReferringPhysicians { get; set; } = null!;
        public DbSet<Test> Tests { get; set; } = null!;
        public DbSet<TestReferenceValue> TestReferenceValues { get; set; } = null!;
        // السطر التالي هو الذي تم تعديله
        public DbSet<LabOrderA> LabOrders { get; set; } = null!; // كان LabOrder وأصبح LabOrderA
        public DbSet<LabOrderItem> LabOrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // يمكنك إضافة أي تكوينات Fluent API هنا إذا لزم الأمر
            // مثال: تعريف العلاقات المعقدة أو المفاتيح المركبة

            // مثال لضمان أن اسم الجدول هو "LabOrders" حتى لو كان اسم الكلاس LabOrderA
            // modelBuilder.Entity<LabOrderA>().ToTable("LabOrders");
            // إذا لم تقم بذلك، قد يحاول EF Core تسمية الجدول "LabOrderAs"
            // تأكد من أن اسم الجدول في قاعدة البيانات الفعلية يتطابق.
        }
    }
}