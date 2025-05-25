// بداية الكود لملف ViewModels/TestsManagementViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services; // For IDatabaseService
// using LABOGRA.Services.Database.Data; // No longer using LabDbContext directly
using Microsoft.EntityFrameworkCore; // Still might be needed for some specific EF types
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class TestsManagementViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService; // Use IDatabaseService

        [ObservableProperty] private string testName = string.Empty;
        [ObservableProperty] private string? testAbbreviation;
        [ObservableProperty] private Test? selectedTest;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAddMode))]
        private bool isEditMode = false;
        public bool IsAddMode => !IsEditMode;

        public ObservableCollection<Test> Tests { get; } = new ObservableCollection<Test>();

        // Constructor now takes IDatabaseService
        public TestsManagementViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _ = LoadTestsAsync();
        }

        private async Task LoadTestsAsync()
        {
            try
            {
                // Use the database service
                var testsFromDb = await _databaseService.GetTestsAsync(); // This should include ReferenceValues as per our DatabaseService
                Tests.Clear();
                foreach (var test in testsFromDb.OrderBy(t => t.Name)) // Optional: Sort here if not sorted by service
                {
                    Tests.Add(test);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل أنواع التحاليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task SaveTestAsync()
        {
            if (string.IsNullOrWhiteSpace(TestName))
            {
                MessageBox.Show("الرجاء إدخال اسم التحليل.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Test testToSave;
                bool isNew = false;

                if (IsEditMode && SelectedTest != null)
                {
                    // Edit mode
                    testToSave = SelectedTest; // Work on the selected instance
                    testToSave.Name = TestName;
                    testToSave.Abbreviation = TestAbbreviation;
                    // ReferenceValues are typically managed in a separate UI/ViewModel
                }
                else
                {
                    // Add mode
                    isNew = true;
                    testToSave = new Test
                    {
                        Name = TestName,
                        Abbreviation = TestAbbreviation,
                    };
                }

                // Use the database service to save
                Test savedTest = await _databaseService.SaveTestAsync(testToSave);

                if (isNew)
                {
                    Tests.Add(savedTest); // Add the newly saved test (which now has an ID) to the collection
                    MessageBox.Show("تم حفظ نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // If editing, refresh the item in the list if SaveTestAsync returns the updated entity
                    // or if the original SelectedTest instance was updated by EF Core change tracking.
                    // For simplicity, if the instance is the same, the UI bound to it will update.
                    // If SaveTestAsync could return a new instance (e.g. from a different context), update explicitly:
                    // var index = Tests.IndexOf(SelectedTest); // SelectedTest might be outdated if SaveTestAsync returns a new instance
                    // if (index != -1) Tests[index] = savedTest;
                    MessageBox.Show("تم تحديث نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ نوع التحليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
        private void PrepareEditTest()
        {
            if (SelectedTest != null)
            {
                TestName = SelectedTest.Name;
                TestAbbreviation = SelectedTest.Abbreviation;
                IsEditMode = true;
            }
        }

        private bool CanEditOrDelete() => SelectedTest != null;

        [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
        private async Task DeleteTestAsync()
        {
            if (SelectedTest != null)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف التحليل: {SelectedTest.Name}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Use the database service
                        bool success = await _databaseService.DeleteTestAsync(SelectedTest.Id);
                        if (success)
                        {
                            Tests.Remove(SelectedTest);
                            MessageBox.Show("تم حذف نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                            ResetForm();
                        }
                        else
                        {
                            MessageBox.Show($"لم يتم حذف نوع التحليل. قد يكون غير موجود.", "فشل الحذف", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    // DbUpdateException should be caught if there are FK constraints
                    catch (DbUpdateException dbEx) { MessageBox.Show($"حدث خطأ أثناء حذف نوع التحليل: {dbEx.InnerException?.Message ?? dbEx.Message}\nقد يكون التحليل مرتبطاً بطلبات فحوصات حالية ولا يمكن حذفه.", "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error); }
                    catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء حذف نوع التحليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
        }

        [RelayCommand]
        private void CancelOperation()
        {
            ResetForm();
        }

        private void ResetForm()
        {
            TestName = string.Empty; TestAbbreviation = null;
            SelectedTest = null; // This will trigger OnSelectedTestChanged
            IsEditMode = false;
        }

        partial void OnSelectedTestChanged(Test? oldValue, Test? newValue)
        {
            if (IsEditMode && newValue != oldValue && oldValue != null)
            {
                CancelOperation();
            }
            // Commands depending on SelectedTest are already handled by [NotifyCanExecuteChangedFor] or manually
            PrepareEditTestCommand.NotifyCanExecuteChanged();
            DeleteTestCommand.NotifyCanExecuteChanged();
        }
    }
}
// نهاية الكود لملف ViewModels/TestsManagementViewModel.cs