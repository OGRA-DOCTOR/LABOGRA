using Microsoft.EntityFrameworkCore;
using LABOGRA.Models;
using System.Collections.Generic;
using System.Linq; // Required for LINQ in seeding if needed, though not directly used here

namespace LABOGRA.Services.Database.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions<LabDbContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<ReferringPhysician> ReferringPhysicians { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<LabOrderItem> LabOrderItems { get; set; }
        public DbSet<TestReferenceValue> TestReferenceValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Test>()
                .HasMany(t => t.ReferenceValues)
                .WithOne(rv => rv.Test)
                .HasForeignKey(rv => rv.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ... (بقية تعريف العلاقات كما كانت) ...
            modelBuilder.Entity<Patient>()
               .HasMany(p => p.LabOrders)
               .WithOne(o => o.Patient)
               .HasForeignKey(o => o.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LabOrder>()
                .HasMany(o => o.Items)
                .WithOne(item => item.LabOrder)
                .HasForeignKey(item => item.LabOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LabOrderItem>()
                .HasOne(item => item.Test)
                .WithMany()
                .HasForeignKey(item => item.TestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
               .HasOne(p => p.ReferringPhysician)
               .WithMany()
               .HasForeignKey(p => p.ReferringPhysicianId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LabOrder>()
               .HasOne(o => o.ReferringPhysician)
               .WithMany()
               .HasForeignKey(o => o.ReferringPhysicianId)
               .OnDelete(DeleteBehavior.SetNull);

            // --- بداية البيانات الأولية للتحاليل والقيم المرجعية (نسخة مصححة) ---

            // أولاً: تعريف التحاليل الأساسية
            modelBuilder.Entity<Test>().HasData(
                // سنستخدم الـ ID مباشرة هنا
                new Test { Id = 1, Name = "Hemoglobin", Abbreviation = "Hb" },
                new Test { Id = 2, Name = "Fasting Blood Glucose", Abbreviation = "FBG" },
                new Test { Id = 3, Name = "Two-Hour Postprandial Glucose", Abbreviation = "2hPPG" },
                new Test { Id = 4, Name = "Random Blood Glucose", Abbreviation = "RBG" },
                new Test { Id = 5, Name = "Glycated Hemoglobin", Abbreviation = "HbA1c" },
                new Test { Id = 6, Name = "Serum Creatinine", Abbreviation = "Cr" },
                new Test { Id = 7, Name = "Blood Urea Nitrogen", Abbreviation = "Urea" },
                new Test { Id = 8, Name = "Uric Acid", Abbreviation = "UA" },
                new Test { Id = 9, Name = "Sodium", Abbreviation = "Na" },
                new Test { Id = 10, Name = "Potassium", Abbreviation = "K" },
                new Test { Id = 11, Name = "Chloride", Abbreviation = "Cl" },
                new Test { Id = 12, Name = "Ionized Calcium", Abbreviation = "Ca²⁺" },
                new Test { Id = 13, Name = "Total Calcium", Abbreviation = "Ca" },
                new Test { Id = 14, Name = "Total Protein", Abbreviation = "TP" },
                new Test { Id = 15, Name = "Albumin", Abbreviation = "Alb" },
                new Test { Id = 16, Name = "Total Bilirubin", Abbreviation = "TBil" },
                new Test { Id = 17, Name = "Direct Bilirubin", Abbreviation = "DBil" },
                new Test { Id = 18, Name = "Alanine Transaminase", Abbreviation = "ALT" },
                new Test { Id = 19, Name = "Aspartate Transaminase", Abbreviation = "AST" },
                new Test { Id = 20, Name = "Alkaline Phosphatase", Abbreviation = "ALP" },
                new Test { Id = 21, Name = "Total Cholesterol", Abbreviation = "TC" },
                new Test { Id = 22, Name = "Triglycerides", Abbreviation = "TG" },
                new Test { Id = 23, Name = "High-Density Lipoprotein Cholesterol", Abbreviation = "HDL" },
                new Test { Id = 24, Name = "Low-Density Lipoprotein Cholesterol", Abbreviation = "LDL" },
                new Test { Id = 25, Name = "Thyroid Stimulating Hormone", Abbreviation = "TSH" },
                new Test { Id = 26, Name = "Free Triiodothyronine", Abbreviation = "Free T3" },
                new Test { Id = 27, Name = "Free Thyroxine", Abbreviation = "Free T4" },
                new Test { Id = 28, Name = "Prolactin", Abbreviation = "PRL" },
                new Test { Id = 29, Name = "Total Testosterone", Abbreviation = "TT" },
                new Test { Id = 30, Name = "Estradiol", Abbreviation = "E2" },
                new Test { Id = 31, Name = "Luteinizing Hormone", Abbreviation = "LH" },
                new Test { Id = 32, Name = "Follicle-Stimulating Hormone", Abbreviation = "FSH" },
                new Test { Id = 33, Name = "Progesterone", Abbreviation = "Prog" },
                new Test { Id = 34, Name = "Erythrocyte Sedimentation Rate", Abbreviation = "ESR" },
                new Test { Id = 35, Name = "C-Reactive Protein", Abbreviation = "CRP" },
                new Test { Id = 36, Name = "Vitamin D (25-OH)", Abbreviation = "Vit D" },
                new Test { Id = 37, Name = "Vitamin B12", Abbreviation = "Vit B12" },
                new Test { Id = 38, Name = "Ferritin", Abbreviation = "Ferritin" },
                new Test { Id = 39, Name = "Serum Iron", Abbreviation = "Iron" },
                new Test { Id = 40, Name = "Total Iron-Binding Capacity", Abbreviation = "TIBC" },
                new Test { Id = 41, Name = "Lactate Dehydrogenase", Abbreviation = "LDH" },
                new Test { Id = 42, Name = "Creatine Kinase", Abbreviation = "CK" },
                new Test { Id = 43, Name = "Amylase", Abbreviation = "AMY" },
                new Test { Id = 44, Name = "Lipase", Abbreviation = "LIP" },
                new Test { Id = 45, Name = "Troponin I", Abbreviation = "TnI" },
                new Test { Id = 46, Name = "D-Dimer", Abbreviation = "D-Dimer" },
                new Test { Id = 47, Name = "Prostate-Specific Antigen", Abbreviation = "PSA" },
                new Test { Id = 48, Name = "Anti-Cyclic Citrullinated Peptide Antibody", Abbreviation = "Anti-CCP" },
                new Test { Id = 49, Name = "Antinuclear Antibody", Abbreviation = "ANA" },
                new Test { Id = 50, Name = "Rheumatoid Factor", Abbreviation = "RF" }
            );

            // ثانياً: تعريف القيم المرجعية لهذه التحاليل (مع استخدام الـ ID الصحيح لكل Test)
            // ملاحظة: سنحتاج إلى عداد منفصل لـ TestReferenceValue.Id
            int refValueIdCounter = 1;
            modelBuilder.Entity<TestReferenceValue>().HasData(
                // Hemoglobin (TestId = 1)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 1, GenderSpecific = "Male", ReferenceText = "13.5 – 17.5", Unit = "g/dL" },
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 1, GenderSpecific = "Female", ReferenceText = "12.0 – 15.5", Unit = "g/dL" },
                // Fasting Blood Glucose (TestId = 2)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 2, GenderSpecific = "Any", ReferenceText = "70 – 100", Unit = "mg/dL" },
                // 2hPPG (TestId = 3)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 3, GenderSpecific = "Any", ReferenceText = "< 140", Unit = "mg/dL" },
                // RBG (TestId = 4)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 4, GenderSpecific = "Any", ReferenceText = "< 200", Unit = "mg/dL" },
                // HbA1c (TestId = 5)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 5, GenderSpecific = "Any", ReferenceText = "4.0 – 5.6", Unit = "%" },
                // Serum Creatinine (TestId = 6)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 6, GenderSpecific = "Male", ReferenceText = "0.7 – 1.3", Unit = "mg/dL" },
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 6, GenderSpecific = "Female", ReferenceText = "0.5 – 1.1", Unit = "mg/dL" },
                // Urea (TestId = 7)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 7, GenderSpecific = "Any", ReferenceText = "7 – 20", Unit = "mg/dL" },
                // Uric Acid (TestId = 8)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 8, GenderSpecific = "Any", ReferenceText = "2.4 – 7.0", Unit = "mg/dL" },
                // Sodium (TestId = 9)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 9, GenderSpecific = "Any", ReferenceText = "135 – 145", Unit = "mmol/L" },
                // Potassium (TestId = 10)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 10, GenderSpecific = "Any", ReferenceText = "3.5 – 5.0", Unit = "mmol/L" },
                // Chloride (TestId = 11)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 11, GenderSpecific = "Any", ReferenceText = "98 – 106", Unit = "mmol/L" },
                // Ionized Calcium (TestId = 12)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 12, GenderSpecific = "Any", ReferenceText = "1.12 – 1.32", Unit = "mmol/L" },
                // Total Calcium (TestId = 13)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 13, GenderSpecific = "Any", ReferenceText = "8.5 – 10.2", Unit = "mg/dL" },
                // Total Protein (TestId = 14)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 14, GenderSpecific = "Any", ReferenceText = "6.0 – 8.3", Unit = "g/dL" },
                // Albumin (TestId = 15)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 15, GenderSpecific = "Any", ReferenceText = "3.5 – 5.0", Unit = "g/dL" },
                // Total Bilirubin (TestId = 16)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 16, GenderSpecific = "Any", ReferenceText = "0.1 – 1.2", Unit = "mg/dL" },
                // Direct Bilirubin (TestId = 17)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 17, GenderSpecific = "Any", ReferenceText = "0.0 – 0.3", Unit = "mg/dL" },
                // ALT (TestId = 18)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 18, GenderSpecific = "Any", ReferenceText = "7 – 56", Unit = "U/L" },
                // AST (TestId = 19)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 19, GenderSpecific = "Any", ReferenceText = "10 – 40", Unit = "U/L" },
                // ALP (TestId = 20)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 20, GenderSpecific = "Any", ReferenceText = "44 – 147", Unit = "U/L" },
                // TC (TestId = 21)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 21, GenderSpecific = "Any", ReferenceText = "< 200", Unit = "mg/dL" },
                // TG (TestId = 22)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 22, GenderSpecific = "Any", ReferenceText = "< 150", Unit = "mg/dL" },
                // HDL (TestId = 23)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 23, GenderSpecific = "Any", ReferenceText = "> 60", Unit = "mg/dL" },
                // LDL (TestId = 24)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 24, GenderSpecific = "Any", ReferenceText = "< 100", Unit = "mg/dL" },
                // TSH (TestId = 25)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 25, GenderSpecific = "Any", ReferenceText = "0.5 – 4.0", Unit = "mIU/L" },
                // Free T3 (TestId = 26)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 26, GenderSpecific = "Any", ReferenceText = "2.3 – 4.2", Unit = "pg/mL" },
                // Free T4 (TestId = 27)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 27, GenderSpecific = "Any", ReferenceText = "0.8 – 1.8", Unit = "ng/dL" },
                // PRL (TestId = 28)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 28, GenderSpecific = "Any", ReferenceText = "4.0 – 23.0", Unit = "ng/mL" },
                // TT (TestId = 29)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 29, GenderSpecific = "Any", ReferenceText = "300 – 1,000", Unit = "ng/dL" }, // Consider adding female range?
                                                                                                                                                         // E2 (TestId = 30)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 30, GenderSpecific = "Any", ReferenceText = "20 – 350", Unit = "pg/mL" }, // Consider adding context (follicular, luteal etc)?
                                                                                                                                                      // LH (TestId = 31)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 31, GenderSpecific = "Any", ReferenceText = "1.24 – 7.8", Unit = "IU/L" }, // Consider adding context?
                                                                                                                                                       // FSH (TestId = 32)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 32, GenderSpecific = "Any", ReferenceText = "1.5 – 12.4", Unit = "IU/L" }, // Consider adding context?
                                                                                                                                                       // Prog (TestId = 33)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 33, GenderSpecific = "Any", ReferenceText = "0.1 – 25.0", Unit = "ng/mL" }, // Consider adding context?
                                                                                                                                                        // ESR (TestId = 34)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 34, GenderSpecific = "Male", ReferenceText = "< 15", Unit = "mm/hr" },
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 34, GenderSpecific = "Female", ReferenceText = "< 20", Unit = "mm/hr" },
                // CRP (TestId = 35)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 35, GenderSpecific = "Any", ReferenceText = "< 3.0", Unit = "mg/L" },
                // Vit D (TestId = 36)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 36, GenderSpecific = "Any", ReferenceText = "20 – 50", Unit = "ng/mL" },
                // Vit B12 (TestId = 37)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 37, GenderSpecific = "Any", ReferenceText = "200 – 900", Unit = "pg/mL" },
                // Ferritin (TestId = 38)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 38, GenderSpecific = "Male", ReferenceText = "24 – 336", Unit = "ng/mL" },
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 38, GenderSpecific = "Female", ReferenceText = "11 – 307", Unit = "ng/mL" },
                // Iron (TestId = 39)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 39, GenderSpecific = "Any", ReferenceText = "60 – 170", Unit = "µg/dL" },
                // TIBC (TestId = 40)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 40, GenderSpecific = "Any", ReferenceText = "240 – 450", Unit = "µg/dL" },
                // LDH (TestId = 41)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 41, GenderSpecific = "Any", ReferenceText = "140 – 280", Unit = "U/L" },
                // CK (TestId = 42)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 42, GenderSpecific = "Male", ReferenceText = "52 – 336", Unit = "U/L" },
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 42, GenderSpecific = "Female", ReferenceText = "38 – 176", Unit = "U/L" },
                // Amylase (TestId = 43)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 43, GenderSpecific = "Any", ReferenceText = "23 – 85", Unit = "U/L" },
                // Lipase (TestId = 44)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 44, GenderSpecific = "Any", ReferenceText = "0 – 160", Unit = "U/L" },
                // Troponin I (TestId = 45)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 45, GenderSpecific = "Any", ReferenceText = "< 0.04", Unit = "ng/mL" },
                // D-Dimer (TestId = 46)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 46, GenderSpecific = "Any", ReferenceText = "< 0.5", Unit = "µg/mL FEU" },
                // PSA (TestId = 47)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 47, GenderSpecific = "Any", ReferenceText = "< 4.0", Unit = "ng/mL" }, // Typically for Males
                                                                                                                                                   // Anti-CCP (TestId = 48)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 48, GenderSpecific = "Any", ReferenceText = "< 20", Unit = "U/mL" },
                // ANA (TestId = 49)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 49, GenderSpecific = "Any", ReferenceText = "Negative at < 1:40", Unit = "Titer" },
                // RF (TestId = 50)
                new TestReferenceValue { Id = refValueIdCounter++, TestId = 50, GenderSpecific = "Any", ReferenceText = "< 15", Unit = "IU/mL" }
            );
            // --- نهاية البيانات الأولية ---
        }
    }
}