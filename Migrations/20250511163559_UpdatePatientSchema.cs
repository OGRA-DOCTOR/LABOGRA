using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LABOGRA.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePatientSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // هذه الأوامر مهمة لإعادة بناء العلاقات بشكل صحيح بعد تعديل الجداول
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrderItems_LabOrders_LabOrderId",
                table: "LabOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrderItems_Tests_TestId",
                table: "LabOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrders_Patients_PatientId",
                table: "LabOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrders_ReferringPhysicians_ReferringPhysicianId",
                table: "LabOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_ReferringPhysicians_ReferringPhysicianId",
                table: "Patients");

            // !!!!!!!!!! بداية الجزء الذي تم حذفه !!!!!!!!!!
            // تم حذف جميع أسطر migrationBuilder.DeleteData(...) من هنا
            // !!!!!!!!!! نهاية الجزء الذي تم حذفه !!!!!!!!!!

            // هذه الأوامر هي لتعديل جدول Patients وهي صحيحة ومطلوبة
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Patients",
                type: "TEXT",
                nullable: true, // كان false، وتم تعديله ليكون nullable
                oldClrType: typeof(string),
                oldType: "TEXT");
            // ملاحظة: إذا كان عمود Title موجوداً بالفعل في قاعدة البيانات كـ NOT NULL
            // وكان هناك بيانات فيه، قد تحتاج أولاً لجعل البيانات الموجودة تقبل null
            // أو توفير قيمة افتراضية. لكن بما أننا عدلنا النموذج ليكون nullable،
            // يجب أن يكون هذا مقبولاً.

            // هذا الأمر لإضافة عمود Address، وهو صحيح ومطلوب
            // الأعمدة الأخرى مثل MedicalRecordNumber و Email يجب أن تكون قد أضيفت
            // في Migration سابقة (20250506195822_AddPatientDetailsAndLabOrderModels.cs)
            // إذا لم تكن موجودة لسبب ما، هذا الـ Migration لن يضيفها، وسنحتاج لـ Migration أخرى لها.
            // لكن بناءً على الـ Migrations التي رأيتها، يجب أن تكون موجودة.
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            // إعادة بناء المفاتيح الخارجية بعد التعديلات
            migrationBuilder.AddForeignKey(
                name: "FK_LabOrderItems_LabOrders_LabOrderId",
                table: "LabOrderItems",
                column: "LabOrderId",
                principalTable: "LabOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade); // كان Restrict في Down، Cascade جيد هنا

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrderItems_Tests_TestId",
                table: "LabOrderItems",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade); // كان Restrict في Down، Cascade جيد هنا

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrders_Patients_PatientId",
                table: "LabOrders",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade); // كان Restrict في Down، Cascade جيد هنا

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrders_ReferringPhysicians_ReferringPhysicianId",
                table: "LabOrders",
                column: "ReferringPhysicianId",
                principalTable: "ReferringPhysicians",
                principalColumn: "Id"); // كان SetNull في Down، الإبقاء عليه أو تغييره لـ SetNull إذا أردت

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_ReferringPhysicians_ReferringPhysicianId",
                table: "Patients",
                column: "ReferringPhysicianId",
                principalTable: "ReferringPhysicians",
                principalColumn: "Id"); // كان SetNull في Down، الإبقاء عليه أو تغييره لـ SetNull إذا أردت
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // دالة Down يجب أن تعكس عكس ما تم في Up
            // بما أننا حذفنا أوامر DeleteData من Up، يجب ألا يكون هناك أوامر InsertData هنا
            // إلا إذا كانت هذه البيانات موجودة أصلاً وتم حذفها بسبب تغييرات أخرى غير DeleteData

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrderItems_LabOrders_LabOrderId",
                table: "LabOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrderItems_Tests_TestId",
                table: "LabOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrders_Patients_PatientId",
                table: "LabOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrders_ReferringPhysicians_ReferringPhysicianId",
                table: "LabOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_ReferringPhysicians_ReferringPhysicianId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Patients",
                type: "TEXT",
                nullable: false, // يعيده إلى الحالة القديمة (NOT NULL)
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            // !!!!!!!!!! بداية الجزء الذي تم حذفه من Down !!!!!!!!!!
            // تم حذف جميع أسطر migrationBuilder.InsertData(...) من هنا
            // لأنها كانت تعيد البيانات التي تم حذفها (افتراضياً) في دالة Up الأصلية.
            // بما أننا أزلنا الحذف من Up، لا يجب إعادة الإضافة هنا.
            // !!!!!!!!!! نهاية الجزء الذي تم حذفه من Down !!!!!!!!!!


            // إعادة بناء المفاتيح الخارجية كما كانت قبل التعديل في Up (إذا لزم الأمر)
            // هذا الجزء مهم لضمان أن التراجع عن الـ Migration (rollback) يعمل بشكل صحيح
            // تم الإبقاء على الإعدادات الأصلية التي كانت في دالة Down التي أرسلتها
            migrationBuilder.AddForeignKey(
               name: "FK_LabOrderItems_LabOrders_LabOrderId",
               table: "LabOrderItems",
               column: "LabOrderId",
               principalTable: "LabOrders",
               principalColumn: "Id",
               onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrderItems_Tests_TestId",
                table: "LabOrderItems",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrders_Patients_PatientId",
                table: "LabOrders",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrders_ReferringPhysicians_ReferringPhysicianId",
                table: "LabOrders",
                column: "ReferringPhysicianId",
                principalTable: "ReferringPhysicians",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_ReferringPhysicians_ReferringPhysicianId",
                table: "Patients",
                column: "ReferringPhysicianId",
                principalTable: "ReferringPhysicians",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}