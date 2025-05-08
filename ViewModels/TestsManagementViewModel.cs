using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LABOGRA.Models;
using LABOGRA.Services.Database.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace LABOGRA.ViewModels
{
    public partial class TestsManagementViewModel : ObservableObject
    {
        private readonly LabDbContext _context;

        [ObservableProperty]
        private string testName = string.Empty;

        [ObservableProperty]
        private string? testAbbreviation;

        // تم التعليق على الخصائص المرتبطة بالحقول التي تمت إزالتها من Test.cs
        // سنحتاج لطريقة جديدة لإدارة الوحدات والقيم المرجعية لاحقاً
        // [ObservableProperty]
        // private string? testUnit; 
        //
        // [ObservableProperty]
        // private string? testReferenceRange; 

        [ObservableProperty]
        private Test? selectedTest;

        public ObservableCollection<Test> Tests { get; } = new ObservableCollection<Test>();

        [RelayCommand]
        private async Task SaveTestAsync()
        {
            if (string.IsNullOrWhiteSpace(TestName))
            {
                MessageBox.Show("الرجاء إدخال اسم التحليل.", "خطأ في الإدخال", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ملاحظة: لا يزال هذا يضيف جديداً دائماً، يجب تحسينه للتحديث لاحقاً.

            try
            {
                var newTest = new Test
                {
                    Name = TestName,
                    Abbreviation = TestAbbreviation,
                    // تم التعليق على الأسطر التي تستخدم الحقول المحذوفة
                    // Unit = TestUnit, 
                    // ReferenceRange = TestReferenceRange 
                };

                _context.Tests.Add(newTest);
                await _context.SaveChangesAsync();
                Tests.Add(newTest);
                MessageBox.Show("تم حفظ نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ نوع التحليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void EditTest()
        {
            if (SelectedTest != null)
            {
                TestName = SelectedTest.Name;
                TestAbbreviation = SelectedTest.Abbreviation;
                // تم التعليق على الأسطر التي تستخدم الحقول المحذوفة
                // TestUnit = SelectedTest.Unit;             // <-- يسبب خطأ CS0117 & CS1061
                // TestReferenceRange = SelectedTest.ReferenceRange; // <-- يسبب خطأ CS0117 & CS1061
            }
            else
            {
                MessageBox.Show("الرجاء تحديد التحليل المراد تعديله من القائمة.", "تحديد عنصر", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task DeleteTestAsync()
        {
            if (SelectedTest != null)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف التحليل: {SelectedTest.Name}؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // عند الحذف، نحتاج لحذف القيم المرجعية المرتبطة به أيضاً إذا لم نستخدم Cascade Delete
                        // بما أننا استخدمنا Cascade Delete في DbContext، يفترض أن يتم حذفها تلقائياً.
                        _context.Tests.Remove(SelectedTest);
                        await _context.SaveChangesAsync();
                        Tests.Remove(SelectedTest);
                        MessageBox.Show("تم حذف نوع التحليل بنجاح!", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                    }
                    catch (DbUpdateException ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء حذف نوع التحليل: {ex.Message}\nقد يكون التحليل مرتبطاً بطلبات فحوصات حالية ولا يمكن حذفه.", "خطأ في الحذف", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء حذف نوع التحليل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("الرجاء تحديد التحليل المراد حذفه من القائمة.", "تحديد عنصر", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            ResetForm();
        }

        public TestsManagementViewModel()
        {
            var contextFactory = new LabDbContextFactory();
            _context = contextFactory.CreateDbContext(Array.Empty<string>());
            LoadTests();
        }

        private async void LoadTests()
        {
            try
            {
                // قد نحتاج لتضمين ReferenceValues هنا إذا أردنا عرضها لاحقاً
                var testsFromDb = await _context.Tests.ToListAsync();
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

        private void ResetForm()
        {
            TestName = string.Empty;
            TestAbbreviation = null;
            // TestUnit = null;             // تم التعليق
            // TestReferenceRange = null; // تم التعليق
            SelectedTest = null;
        }
    }
}