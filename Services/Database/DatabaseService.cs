using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
namespace LABOGRA.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly LabDbContext _context;
        public DatabaseService(LabDbContext context)
        {
            _context = context;
        }
        #region عمليات المرضى - مع استخدام PhoneNumber الصحيح
        public async Task<List<Patient>> GetPatientsAsync()
        {
            return await _context.Patients
                .Include(p => p.LabOrders)
                .ToListAsync();
        }
        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.LabOrders)
                .ThenInclude(lo => lo.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Patient> SavePatientAsync(Patient patient)
        {
            if (patient.Id == 0)
            {
                _context.Patients.Add(patient);
            }
            else
            {
                _context.Patients.Update(patient);
            }

            await _context.SaveChangesAsync();
            return patient;
        }
        public async Task<bool> DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return false;
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetPatientsAsync();
            return await _context.Patients
                .Where(p => p.Name.Contains(searchTerm) ||
                           (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)) ||
                           (p.Email != null && p.Email.Contains(searchTerm)))
                .Include(p => p.LabOrders)
                .ToListAsync();
        }
        #endregion
        #region عمليات الطلبات المخبرية - استخدام LabOrderA
        public async Task<List<LabOrderA>> GetLabOrdersAsync()
        {
            return await _context.LabOrders
                .Include(lo => lo.Patient)
                .Include(lo => lo.ReferringPhysician)
                .Include(lo => lo.Items)
                .ThenInclude(loi => loi.Test)
                .ToListAsync();
        }
        public async Task<LabOrderA?> GetLabOrderByIdAsync(int id)
        {
            return await _context.LabOrders
                .Include(lo => lo.Patient)
                .Include(lo => lo.ReferringPhysician)
                .Include(lo => lo.Items)
                .ThenInclude(loi => loi.Test)
                .FirstOrDefaultAsync(lo => lo.Id == id);
        }
        public async Task<LabOrderA> SaveLabOrderAsync(LabOrderA labOrder)
        {
            if (labOrder.Id == 0)
            {
                _context.LabOrders.Add(labOrder);
            }
            else
            {
                _context.LabOrders.Update(labOrder);
            }

            await _context.SaveChangesAsync();
            return labOrder;
        }
        public async Task<bool> DeleteLabOrderAsync(int id)
        {
            var labOrder = await _context.LabOrders.FindAsync(id);
            if (labOrder == null) return false;
            _context.LabOrders.Remove(labOrder);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<LabOrderA>> GetLabOrdersByPatientIdAsync(int patientId)
        {
            return await _context.LabOrders
                .Where(lo => lo.PatientId == patientId)
                .Include(lo => lo.ReferringPhysician)
                .Include(lo => lo.Items)
                .ThenInclude(loi => loi.Test)
                .ToListAsync();
        }
        #endregion
        #region عمليات الفحوصات - استخدام ReferenceValues
        public async Task<List<Test>> GetTestsAsync()
        {
            return await _context.Tests
                .Include(t => t.ReferenceValues)
                .ToListAsync();
        }
        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _context.Tests
                .Include(t => t.ReferenceValues)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<Test> SaveTestAsync(Test test)
        {
            if (test.Id == 0)
            {
                _context.Tests.Add(test);
            }
            else
            {
                _context.Tests.Update(test);
            }

            await _context.SaveChangesAsync();
            return test;
        }
        public async Task<bool> DeleteTestAsync(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null) return false;
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion
        #region عمليات القيم المرجعية
        public async Task<List<TestReferenceValue>> GetReferenceValuesAsync()
        {
            return await _context.TestReferenceValues
                .Include(rv => rv.Test)
                .ToListAsync();
        }
        public async Task<List<TestReferenceValue>> GetReferenceValuesByTestIdAsync(int testId)
        {
            return await _context.TestReferenceValues
                .Where(rv => rv.TestId == testId)
                .ToListAsync();
        }
        public async Task<TestReferenceValue> SaveReferenceValueAsync(TestReferenceValue referenceValue)
        {
            if (referenceValue.Id == 0)
            {
                _context.TestReferenceValues.Add(referenceValue);
            }
            else
            {
                _context.TestReferenceValues.Update(referenceValue);
            }

            await _context.SaveChangesAsync();
            return referenceValue;
        }
        #endregion
        #region عمليات الأطباء
        public async Task<List<ReferringPhysician>> GetPhysiciansAsync()
        {
            return await _context.ReferringPhysicians.ToListAsync();
        }
        public async Task<ReferringPhysician?> GetPhysicianByIdAsync(int id)
        {
            return await _context.ReferringPhysicians.FindAsync(id);
        }
        public async Task<ReferringPhysician> SavePhysicianAsync(ReferringPhysician physician)
        {
            if (physician.Id == 0)
            {
                _context.ReferringPhysicians.Add(physician);
            }
            else
            {
                _context.ReferringPhysicians.Update(physician);
            }

            await _context.SaveChangesAsync();
            return physician;
        }
        #endregion
        #region عمليات قاعدة البيانات
        public async Task<bool> EnsureDatabaseCreatedAsync()
        {
            try
            {
                return await _context.Database.EnsureCreatedAsync();
            }
            catch
            {
                return false;
            }
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                if (connectionString?.Contains("Data Source=") == true)
                {
                    var dbPath = connectionString.Split("Data Source=")[1].Split(';')[0];
                    File.Copy(dbPath, backupPath, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                if (connectionString?.Contains("Data Source=") == true && File.Exists(backupPath))
                {
                    var dbPath = connectionString.Split("Data Source=")[1].Split(';')[0];
                    File.Copy(backupPath, dbPath, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}