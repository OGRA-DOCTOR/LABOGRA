using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LABOGRA.Migrations
{
    /// <inheritdoc />
    public partial class ApplyRefValueStructureAndData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceRange",
                table: "Tests");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Tests",
                newName: "Abbreviation");

            migrationBuilder.CreateTable(
                name: "TestReferenceValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TestId = table.Column<int>(type: "INTEGER", nullable: false),
                    GenderSpecific = table.Column<string>(type: "TEXT", nullable: true),
                    ReferenceText = table.Column<string>(type: "TEXT", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestReferenceValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestReferenceValues_Tests_TestId",
                        column: x => x.TestId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tests",
                columns: new[] { "Id", "Abbreviation", "Name" },
                values: new object[,]
                {
                    { 1, "Hb", "Hemoglobin" },
                    { 2, "FBG", "Fasting Blood Glucose" },
                    { 3, "2hPPG", "Two-Hour Postprandial Glucose" },
                    { 4, "RBG", "Random Blood Glucose" },
                    { 5, "HbA1c", "Glycated Hemoglobin" },
                    { 6, "Cr", "Serum Creatinine" },
                    { 7, "Urea", "Blood Urea Nitrogen" },
                    { 8, "UA", "Uric Acid" },
                    { 9, "Na", "Sodium" },
                    { 10, "K", "Potassium" },
                    { 11, "Cl", "Chloride" },
                    { 12, "Ca²⁺", "Ionized Calcium" },
                    { 13, "Ca", "Total Calcium" },
                    { 14, "TP", "Total Protein" },
                    { 15, "Alb", "Albumin" },
                    { 16, "TBil", "Total Bilirubin" },
                    { 17, "DBil", "Direct Bilirubin" },
                    { 18, "ALT", "Alanine Transaminase" },
                    { 19, "AST", "Aspartate Transaminase" },
                    { 20, "ALP", "Alkaline Phosphatase" },
                    { 21, "TC", "Total Cholesterol" },
                    { 22, "TG", "Triglycerides" },
                    { 23, "HDL", "High-Density Lipoprotein Cholesterol" },
                    { 24, "LDL", "Low-Density Lipoprotein Cholesterol" },
                    { 25, "TSH", "Thyroid Stimulating Hormone" },
                    { 26, "Free T3", "Free Triiodothyronine" },
                    { 27, "Free T4", "Free Thyroxine" },
                    { 28, "PRL", "Prolactin" },
                    { 29, "TT", "Total Testosterone" },
                    { 30, "E2", "Estradiol" },
                    { 31, "LH", "Luteinizing Hormone" },
                    { 32, "FSH", "Follicle-Stimulating Hormone" },
                    { 33, "Prog", "Progesterone" },
                    { 34, "ESR", "Erythrocyte Sedimentation Rate" },
                    { 35, "CRP", "C-Reactive Protein" },
                    { 36, "Vit D", "Vitamin D (25-OH)" },
                    { 37, "Vit B12", "Vitamin B12" },
                    { 38, "Ferritin", "Ferritin" },
                    { 39, "Iron", "Serum Iron" },
                    { 40, "TIBC", "Total Iron-Binding Capacity" },
                    { 41, "LDH", "Lactate Dehydrogenase" },
                    { 42, "CK", "Creatine Kinase" },
                    { 43, "AMY", "Amylase" },
                    { 44, "LIP", "Lipase" },
                    { 45, "TnI", "Troponin I" },
                    { 46, "D-Dimer", "D-Dimer" },
                    { 47, "PSA", "Prostate-Specific Antigen" },
                    { 48, "Anti-CCP", "Anti-Cyclic Citrullinated Peptide Antibody" },
                    { 49, "ANA", "Antinuclear Antibody" },
                    { 50, "RF", "Rheumatoid Factor" }
                });

            migrationBuilder.InsertData(
                table: "TestReferenceValues",
                columns: new[] { "Id", "GenderSpecific", "Notes", "ReferenceText", "TestId", "Unit" },
                values: new object[,]
                {
                    { 1, "Male", null, "13.5 – 17.5", 1, "g/dL" },
                    { 2, "Female", null, "12.0 – 15.5", 1, "g/dL" },
                    { 3, "Any", null, "70 – 100", 2, "mg/dL" },
                    { 4, "Any", null, "< 140", 3, "mg/dL" },
                    { 5, "Any", null, "< 200", 4, "mg/dL" },
                    { 6, "Any", null, "4.0 – 5.6", 5, "%" },
                    { 7, "Male", null, "0.7 – 1.3", 6, "mg/dL" },
                    { 8, "Female", null, "0.5 – 1.1", 6, "mg/dL" },
                    { 9, "Any", null, "7 – 20", 7, "mg/dL" },
                    { 10, "Any", null, "2.4 – 7.0", 8, "mg/dL" },
                    { 11, "Any", null, "135 – 145", 9, "mmol/L" },
                    { 12, "Any", null, "3.5 – 5.0", 10, "mmol/L" },
                    { 13, "Any", null, "98 – 106", 11, "mmol/L" },
                    { 14, "Any", null, "1.12 – 1.32", 12, "mmol/L" },
                    { 15, "Any", null, "8.5 – 10.2", 13, "mg/dL" },
                    { 16, "Any", null, "6.0 – 8.3", 14, "g/dL" },
                    { 17, "Any", null, "3.5 – 5.0", 15, "g/dL" },
                    { 18, "Any", null, "0.1 – 1.2", 16, "mg/dL" },
                    { 19, "Any", null, "0.0 – 0.3", 17, "mg/dL" },
                    { 20, "Any", null, "7 – 56", 18, "U/L" },
                    { 21, "Any", null, "10 – 40", 19, "U/L" },
                    { 22, "Any", null, "44 – 147", 20, "U/L" },
                    { 23, "Any", null, "< 200", 21, "mg/dL" },
                    { 24, "Any", null, "< 150", 22, "mg/dL" },
                    { 25, "Any", null, "> 60", 23, "mg/dL" },
                    { 26, "Any", null, "< 100", 24, "mg/dL" },
                    { 27, "Any", null, "0.5 – 4.0", 25, "mIU/L" },
                    { 28, "Any", null, "2.3 – 4.2", 26, "pg/mL" },
                    { 29, "Any", null, "0.8 – 1.8", 27, "ng/dL" },
                    { 30, "Any", null, "4.0 – 23.0", 28, "ng/mL" },
                    { 31, "Any", null, "300 – 1,000", 29, "ng/dL" },
                    { 32, "Any", null, "20 – 350", 30, "pg/mL" },
                    { 33, "Any", null, "1.24 – 7.8", 31, "IU/L" },
                    { 34, "Any", null, "1.5 – 12.4", 32, "IU/L" },
                    { 35, "Any", null, "0.1 – 25.0", 33, "ng/mL" },
                    { 36, "Male", null, "< 15", 34, "mm/hr" },
                    { 37, "Female", null, "< 20", 34, "mm/hr" },
                    { 38, "Any", null, "< 3.0", 35, "mg/L" },
                    { 39, "Any", null, "20 – 50", 36, "ng/mL" },
                    { 40, "Any", null, "200 – 900", 37, "pg/mL" },
                    { 41, "Male", null, "24 – 336", 38, "ng/mL" },
                    { 42, "Female", null, "11 – 307", 38, "ng/mL" },
                    { 43, "Any", null, "60 – 170", 39, "µg/dL" },
                    { 44, "Any", null, "240 – 450", 40, "µg/dL" },
                    { 45, "Any", null, "140 – 280", 41, "U/L" },
                    { 46, "Male", null, "52 – 336", 42, "U/L" },
                    { 47, "Female", null, "38 – 176", 42, "U/L" },
                    { 48, "Any", null, "23 – 85", 43, "U/L" },
                    { 49, "Any", null, "0 – 160", 44, "U/L" },
                    { 50, "Any", null, "< 0.04", 45, "ng/mL" },
                    { 51, "Any", null, "< 0.5", 46, "µg/mL FEU" },
                    { 52, "Any", null, "< 4.0", 47, "ng/mL" },
                    { 53, "Any", null, "< 20", 48, "U/mL" },
                    { 54, "Any", null, "Negative at < 1:40", 49, "Titer" },
                    { 55, "Any", null, "< 15", 50, "IU/mL" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestReferenceValues_TestId",
                table: "TestReferenceValues",
                column: "TestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestReferenceValues");

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Tests",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.RenameColumn(
                name: "Abbreviation",
                table: "Tests",
                newName: "Unit");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceRange",
                table: "Tests",
                type: "TEXT",
                nullable: true);
        }
    }
}
