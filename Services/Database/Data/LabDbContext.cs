// بداية الكود لملف Services/Database/Data/LabDbContext.cs
using LABOGRA.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        public DbSet<User> Users { get; set; } = null!;

        private string HashPasswordForSeed(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LabOrderA>().ToTable("LabOrders");
            string adminPasswordHash = HashPasswordForSeed("0000");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = adminPasswordHash,
                    Role = "Admin"
                }
            );
        }
    }
}
// نهاية الكود لملف Services/Database/Data/LabDbContext.cs