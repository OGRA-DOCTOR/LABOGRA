using System.Windows.Controls;
using LABOGRA.ViewModels; // لاستخدام الـ ViewModel

namespace LABOGRA.Views.TestsManagement
{
    /// <summary>
    /// Interaction logic for TestsManagementView.xaml
    /// </summary>
    public partial class TestsManagementView : UserControl // تم تغيير من Window إلى UserControl
    {
        public TestsManagementView()
        {
            InitializeComponent();
            // تعيين الـ DataContext للـ View
            DataContext = new TestsManagementViewModel();
        }
    }
}