// نفس كود الإصدار 3 الذي تم تقديمه سابقاً لملف Views\Patients\PatientsView.xaml.cs
// لم يتم إجراء تغييرات تتطلب إصداراً جديداً في هذه المرحلة.
// الرجاء لصق محتوى الإصدار 3 هنا بالكامل.
// إليك المحتوى مجدداً للتسهيل:

using System;
using System.Windows.Controls;
using LABOGRA.ViewModels;

namespace LABOGRA.Views.Patients
{
    /// <summary>
    /// Interaction logic for PatientsView.xaml
    /// </summary>
    public partial class PatientsView : UserControl
    {
        public PatientsView()
        {
            InitializeComponent();
            // تعيين الـ DataContext للـ View
            DataContext = new PatientsViewModel();
        }
    }
}