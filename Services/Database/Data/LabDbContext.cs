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
        public DbSet<LabOrderA> LabOrders { get; set; } = null!;
        public DbSet<LabOrderItem> LabOrderItems { get; set; } = null!;

        // إضافة جدول المستخدمين - هذا السطر مهم جداً
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تأكد من اسم جدول LabOrders
            modelBuilder.Entity<LabOrderA>().ToTable("LabOrders");

            // إضافة مستخدم افتراضي
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "0000", // في الواقع يجب تشفيرها، لكن للبساطة نتركها كما هي
                    Role = "Admin"
                }
            );
        }
    }
}