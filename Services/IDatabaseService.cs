using LABOGRA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LABOGRA.Services
{
    public interface IDatabaseService
    {
        // Patient Operations
        Task<List<Patient>> GetPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> SavePatientAsync(Patient patient); // <<=== ENSURE THIS SIGNATURE EXISTS AND RETURNS Task<Patient>
        Task<bool> DeletePatientAsync(int id);
        Task<List<Patient>> SearchPatientsAsync(string searchTerm); // Basic search
        // Consider adding: Task<List<Patient>> AdvancedSearchPatientsAsync(PatientSearchCriteria criteria);

        // LabOrderA Operations
        Task<List<LabOrderA>> GetLabOrdersAsync();
        Task<LabOrderA?> GetLabOrderByIdAsync(int id);
        Task<LabOrderA> SaveLabOrderAsync(LabOrderA labOrder);
        Task<bool> DeleteLabOrderAsync(int id);
        Task<List<LabOrderA>> GetLabOrdersByPatientIdAsync(int patientId);

        // Test Operations
        Task<List<Test>> GetTestsAsync();
        Task<Test?> GetTestByIdAsync(int id);
        Task<Test> SaveTestAsync(Test test);
        Task<bool> DeleteTestAsync(int id);

        // TestReferenceValue Operations
        Task<List<TestReferenceValue>> GetReferenceValuesAsync();
        Task<List<TestReferenceValue>> GetReferenceValuesByTestIdAsync(int testId);
        Task<TestReferenceValue> SaveReferenceValueAsync(TestReferenceValue referenceValue);

        // ReferringPhysician Operations
        Task<List<ReferringPhysician>> GetPhysiciansAsync();
        Task<ReferringPhysician?> GetPhysicianByIdAsync(int id);
        Task<ReferringPhysician> SavePhysicianAsync(ReferringPhysician physician);

        // Database Utility Operations
        Task<bool> EnsureDatabaseCreatedAsync();
        Task<int> SaveChangesAsync(); // General SaveChanges
        Task<bool> BackupDatabaseAsync(string backupPath);
        Task<bool> RestoreDatabaseAsync(string backupPath);

        // --- New methods added previously ---
        Task<Patient?> GetLatestPatientByGeneratedIdPrefixAsync(string idPrefix);
        Task<ReferringPhysician> FindOrCreateReferringPhysicianAsync(string physicianName);
        Task<bool> AddPatientWithLabOrderAsync(Patient patient);

        Task<LabOrderItem?> GetLabOrderItemByIdAsync(int labOrderItemId);
        Task<bool> UpdateLabOrderItemResultAsync(int labOrderItemId, string? result, string? unit);
    }
}