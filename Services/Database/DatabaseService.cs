using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System; // For StringSplitOptions

namespace LABOGRA.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly LabDbContext _context;
        public DatabaseService(LabDbContext context)
        {
            _context = context;
        }

        #region Patient Operations
        public async Task<List<Patient>> GetPatientsAsync()
        {
            return await _context.Patients
                .Include(p => p.ReferringPhysician)
                .Include(p => p.LabOrders)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.ReferringPhysician)
                .Include(p => p.LabOrders)
                    .ThenInclude(lo => lo.Items)
                        .ThenInclude(item => item.Test)
                             .ThenInclude(t => t != null ? t.ReferenceValues : null) // Handle possible null Test
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // <<=== ENSURE THIS IMPLEMENTATION IS CORRECT ===>>
        public async Task<Patient> SavePatientAsync(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            if (patient.Id == 0) // New patient
            {
                _context.Patients.Add(patient);
            }
            else // Existing patient
            {
                // Ensure the entity is tracked correctly for update
                var existingPatient = await _context.Patients.FindAsync(patient.Id);
                if (existingPatient != null)
                {
                    _context.Entry(existingPatient).CurrentValues.SetValues(patient);
                    // If ReferringPhysician is an object and its ID might have changed or it's a new one
                    if (patient.ReferringPhysicianId.HasValue)
                    {
                        existingPatient.ReferringPhysicianId = patient.ReferringPhysicianId;
                        // EF will handle the relationship if the physician with this ID exists.
                        // No need to attach patient.ReferringPhysician if only ID is used.
                    }
                    else
                    {
                        existingPatient.ReferringPhysicianId = null;
                        existingPatient.ReferringPhysician = null;
                    }
                }
                else // Should not happen if ID is from a valid existing patient
                {
                    _context.Patients.Update(patient); // Fallback, or throw error
                }
            }
            await _context.SaveChangesAsync();
            return patient; // Return the saved/updated patient (EF updates the ID on Add)
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
            var query = _context.Patients.Include(p => p.ReferringPhysician).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(lowerSearchTerm)) ||
                    (p.GeneratedId != null && p.GeneratedId.Contains(searchTerm)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)) ||
                    (p.Email != null && p.Email.ToLower().Contains(lowerSearchTerm)));
            }
            return await query.AsNoTracking().ToListAsync();
        }
        #endregion

        // ... (باقي الأقسام LabOrderA, Test, TestReferenceValue, ReferringPhysician, Database Utilities كما هي من الرد السابق)
        #region عمليات الطلبات المخبرية - استخدام LabOrderA
        public async Task<List<LabOrderA>> GetLabOrdersAsync() { return await _context.LabOrders.Include(lo => lo.Patient).Include(lo => lo.ReferringPhysician).Include(lo => lo.Items).ThenInclude(loi => loi.Test).AsNoTracking().ToListAsync(); }
        public async Task<LabOrderA?> GetLabOrderByIdAsync(int id) { return await _context.LabOrders.Include(lo => lo.Patient).Include(lo => lo.ReferringPhysician).Include(lo => lo.Items).ThenInclude(loi => loi.Test).ThenInclude(t => t != null ? t.ReferenceValues : null).FirstOrDefaultAsync(lo => lo.Id == id); }
        public async Task<LabOrderA> SaveLabOrderAsync(LabOrderA labOrder) { if (labOrder.Id == 0) { _context.LabOrders.Add(labOrder); } else { _context.Entry(labOrder).State = EntityState.Modified; } await _context.SaveChangesAsync(); return labOrder; }
        public async Task<bool> DeleteLabOrderAsync(int id) { var labOrder = await _context.LabOrders.FindAsync(id); if (labOrder == null) return false; _context.LabOrders.Remove(labOrder); await _context.SaveChangesAsync(); return true; }
        public async Task<List<LabOrderA>> GetLabOrdersByPatientIdAsync(int patientId) { return await _context.LabOrders.Where(lo => lo.PatientId == patientId).Include(lo => lo.ReferringPhysician).Include(lo => lo.Items).ThenInclude(loi => loi.Test).ThenInclude(t => t != null ? t.ReferenceValues : null).AsNoTracking().ToListAsync(); }
        #endregion
        #region عمليات الفحوصات - استخدام ReferenceValues
        public async Task<List<Test>> GetTestsAsync() { return await _context.Tests.Include(t => t.ReferenceValues).AsNoTracking().ToListAsync(); }
        public async Task<Test?> GetTestByIdAsync(int id) { return await _context.Tests.Include(t => t.ReferenceValues).FirstOrDefaultAsync(t => t.Id == id); }
        public async Task<Test> SaveTestAsync(Test test) { if (test.Id == 0) { _context.Tests.Add(test); } else { _context.Entry(test).State = EntityState.Modified; } await _context.SaveChangesAsync(); return test; }
        public async Task<bool> DeleteTestAsync(int id) { var test = await _context.Tests.FindAsync(id); if (test == null) return false; _context.Tests.Remove(test); await _context.SaveChangesAsync(); return true; }
        #endregion
        #region عمليات القيم المرجعية
        public async Task<List<TestReferenceValue>> GetReferenceValuesAsync() { return await _context.TestReferenceValues.Include(rv => rv.Test).AsNoTracking().ToListAsync(); }
        public async Task<List<TestReferenceValue>> GetReferenceValuesByTestIdAsync(int testId) { return await _context.TestReferenceValues.Where(rv => rv.TestId == testId).AsNoTracking().ToListAsync(); }
        public async Task<TestReferenceValue> SaveReferenceValueAsync(TestReferenceValue referenceValue) { if (referenceValue.Id == 0) { _context.TestReferenceValues.Add(referenceValue); } else { _context.Entry(referenceValue).State = EntityState.Modified; } await _context.SaveChangesAsync(); return referenceValue; }
        #endregion
        #region عمليات الأطباء
        public async Task<List<ReferringPhysician>> GetPhysiciansAsync() { return await _context.ReferringPhysicians.AsNoTracking().ToListAsync(); }
        public async Task<ReferringPhysician?> GetPhysicianByIdAsync(int id) { return await _context.ReferringPhysicians.FindAsync(id); }
        public async Task<ReferringPhysician> SavePhysicianAsync(ReferringPhysician physician) { if (physician.Id == 0) { _context.ReferringPhysicians.Add(physician); } else { _context.Entry(physician).State = EntityState.Modified; } await _context.SaveChangesAsync(); return physician; }
        #endregion
        #region عمليات قاعدة البيانات
        public async Task<bool> EnsureDatabaseCreatedAsync() { return await _context.Database.EnsureCreatedAsync(); }
        public async Task<int> SaveChangesAsync() { return await _context.SaveChangesAsync(); }
        public Task<bool> BackupDatabaseAsync(string backupPath) { try { var cs = _context.Database.GetConnectionString(); if (cs?.Contains("Data Source=") == true) { var dbPath = cs.Split(new[] { "Data Source=" }, StringSplitOptions.None)[1].Split(';')[0]; File.Copy(dbPath, backupPath, true); return Task.FromResult(true); } return Task.FromResult(false); } catch { return Task.FromResult(false); } }
        public Task<bool> RestoreDatabaseAsync(string backupPath) { try { var cs = _context.Database.GetConnectionString(); if (cs?.Contains("Data Source=") == true && File.Exists(backupPath)) { var dbPath = cs.Split(new[] { "Data Source=" }, StringSplitOptions.None)[1].Split(';')[0]; File.Copy(backupPath, dbPath, true); return Task.FromResult(true); } return Task.FromResult(false); } catch { return Task.FromResult(false); } }
        #endregion

        // --- تنفيذ الدوال الجديدة ---
        public async Task<Patient?> GetLatestPatientByGeneratedIdPrefixAsync(string idPrefix) { return await _context.Patients.Where(p => p.GeneratedId != null && p.GeneratedId.StartsWith(idPrefix)).OrderByDescending(p => p.GeneratedId).AsNoTracking().FirstOrDefaultAsync(); }
        public async Task<ReferringPhysician> FindOrCreateReferringPhysicianAsync(string physicianName) { var existing = await _context.ReferringPhysicians.FirstOrDefaultAsync(rp => rp.Name.ToLower() == physicianName.ToLower()); if (existing != null) return existing; var newPhysician = new ReferringPhysician { Name = physicianName }; _context.ReferringPhysicians.Add(newPhysician); /* await _context.SaveChangesAsync(); // Consider if save is needed here or as part of larger unit of work */ return newPhysician; }
        public async Task<bool> AddPatientWithLabOrderAsync(Patient patient) { if (patient == null) return false; _context.Patients.Add(patient); try { int changes = await _context.SaveChangesAsync(); return changes > 0; } catch { return false; } }
        public async Task<LabOrderItem?> GetLabOrderItemByIdAsync(int labOrderItemId) { return await _context.LabOrderItems.Include(item => item.Test).FirstOrDefaultAsync(item => item.Id == labOrderItemId); }
        public async Task<bool> UpdateLabOrderItemResultAsync(int labOrderItemId, string? result, string? unit) { var item = await _context.LabOrderItems.FindAsync(labOrderItemId); if (item != null) { item.Result = result; item.ResultUnit = unit; try { await _context.SaveChangesAsync(); return true; } catch { return false; } } return false; }
    }
}