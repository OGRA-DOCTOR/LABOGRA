using LABOGRA.Models;
namespace LABOGRA.Services
{
    public interface IDatabaseService
    {
        // عمليات المرضى - استخدام الخصائص الصحيحة
        Task<List<Patient>> GetPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> SavePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int id);
        Task<List<Patient>> SearchPatientsAsync(string searchTerm);
        // عمليات الطلبات المخبرية - استخدام LabOrderA
        Task<List<LabOrderA>> GetLabOrdersAsync();
        Task<LabOrderA?> GetLabOrderByIdAsync(int id);
        Task<LabOrderA> SaveLabOrderAsync(LabOrderA labOrder);
        Task<bool> DeleteLabOrderAsync(int id);
        Task<List<LabOrderA>> GetLabOrdersByPatientIdAsync(int patientId);
        // عمليات الفحوصات
        Task<List<Test>> GetTestsAsync();
        Task<Test?> GetTestByIdAsync(int id);
        Task<Test> SaveTestAsync(Test test);
        Task<bool> DeleteTestAsync(int id);
        // عمليات القيم المرجعية
        Task<List<TestReferenceValue>> GetReferenceValuesAsync();
        Task<List<TestReferenceValue>> GetReferenceValuesByTestIdAsync(int testId);
        Task<TestReferenceValue> SaveReferenceValueAsync(TestReferenceValue referenceValue);
        // عمليات الأطباء
        Task<List<ReferringPhysician>> GetPhysiciansAsync();
        Task<ReferringPhysician?> GetPhysicianByIdAsync(int id);
        Task<ReferringPhysician> SavePhysicianAsync(ReferringPhysician physician);
        // عمليات قاعدة البيانات
        Task<bool> EnsureDatabaseCreatedAsync();
        Task<int> SaveChangesAsync();
        Task<bool> BackupDatabaseAsync(string backupPath);
        Task<bool> RestoreDatabaseAsync(string backupPath);
    }
}