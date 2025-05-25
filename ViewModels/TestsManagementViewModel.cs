// بداية الكود لملف ViewModels/TestsManagementViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq; // تمت إضافة هذا السطر
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class TestsManagementViewModel : ObservableObject
    {
        private readonly LabDbContext _dbContext; // تعديل: لاستقبال DbContext مباشرة

        [ObservableProperty] private string testName = string.Empty;
        [ObservableProperty] private string? testAbbreviation;
        [ObservableProperty] private Test? selectedTest;

        // خاصية جديدة لتحديد ما إذا كنا في وضع التعديل أم الإضافة
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAddMode))]
        private bool isEditMode = false;
        public bool IsAddMode => !IsEditMode;


        public ObservableCollection<Test> Tests { get; } = new ObservableCollection<Test>();

        // تعديل المنشئ ليقبل LabDbContext
        public TestsManagementViewModel(LabDbContext dbContext)
        {
            _dbContext = dbContext; // تخزين DbContext المستلم
            _ = LoadTestsAsync(); // تغيير اسم الدالة وإضافة async
        }

        private async Task LoadTestsAsync() // تغيير اسم الدالة وإضافة async
        {
            try
            {
                // استخدام _dbContext مباشرة
                var testsFromDb = await _dbContext.Tests.Include(t => t.ReferenceValues).ToListAsync(); // تضمين القيم المرجعية
                Tests.Clear();
                foreach (var test in testsFromDb)
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
                if (IsEditMode && SelectedTest != null)
                {
                    // وضع التعديل
                    SelectedTest.Name = TestName;
                    SelectedTest.Abbreviation = TestAbbreviation;
                    // لا يوجد Unit أو ReferenceRange هنا مباشرة، ستتم إدارتها بشكل منفصل

                    _dbContext.Tests.Update(SelectedTest);
                    await _dbContext.SaveChangesAsync();

                    // تحديث العنصر في القائمة المعروضة
                    var index = Tests.IndexOf(SelectedTest);
                    if (index != -1) Tests[index] = SelectedTest; // قد تحتاج لتحديث أعمق إذا كان SelectedTest نسخة

                    MessageBox.Show("تم تحديث نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // وضع الإضافة
                    var newTest = new Test
                    {
                        Name = TestName,
                        Abbreviation = TestAbbreviation,
                    };
                    _dbContext.Tests.Add(newTest);
                    await _dbContext.SaveChangesAsync();
                    Tests.Add(newTest); // إضافة الكائن الجديد للقائمة
                    MessageBox.Show("تم حفظ نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ نوع التحليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDelete))]
        private void PrepareEditTest() // تغيير اسم الدالة
        {
            if (SelectedTest != null)
            {
                TestName = SelectedTest.Name;
                TestAbbreviation = SelectedTest.Abbreviation;
                IsEditMode = true; // الدخول في وضع التعديل
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
                        _dbContext.Tests.Remove(SelectedTest);
                        await _dbContext.SaveChangesAsync();
                        Tests.Remove(SelectedTest);
                        MessageBox.Show("تم حذف نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                    }
                    catch (DbUpdateException ex) { MessageBox.Show($"حدث خطأ أثناء حذف نوع التحليل: {ex.InnerException?.Message ?? ex.Message}\nقد يكون التحليل مرتبطاً بطلبات فحوصات حالية ولا يمكن حذفه.", "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error); }
                    catch (Exception ex) { MessageBox.Show($"حدث خطأ أثناء حذف نوع التحليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
        }

        [RelayCommand]
        private void CancelOperation() // تغيير اسم الدالة
        {
            ResetForm();
        }

        private void ResetForm()
        {
            TestName = string.Empty; TestAbbreviation = null;
            SelectedTest = null;
            IsEditMode = false; // الخروج من وضع التعديل
        }

        // عند تغيير الاختيار، نخرج من وضع التعديل إذا لم يكن العنصر المحدد هو الذي يتم تعديله
        partial void OnSelectedTestChanged(Test? oldValue, Test? newValue)
        {
            if (IsEditMode && newValue != oldValue && oldValue != null) // إذا كنا نعدل ثم اخترنا عنصراً آخر
            {
                // يمكن أن نعرض رسالة تحذير هنا قبل إلغاء التعديل
                // MessageBox.Show("تم إلغاء التعديلات الحالية بسبب تغيير التحديد.");
                CancelOperation(); // إلغاء وضع التعديل
            }
            // تحديث حالة الأوامر التي تعتمد على SelectedTest
            PrepareEditTestCommand.NotifyCanExecuteChanged();
            DeleteTestCommand.NotifyCanExecuteChanged();
        }
    }
}
// نهاية الكود لملف ViewModels/TestsManagementViewModel.cs