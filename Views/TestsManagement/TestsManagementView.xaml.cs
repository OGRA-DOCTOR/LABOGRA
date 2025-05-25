// بداية الكود لملف Views/TestsManagement/TestsManagementView.xaml.cs
using System.Windows.Controls;
// لم نعد بحاجة لـ using LABOGRA.ViewModels; هنا

namespace LABOGRA.Views.TestsManagement
{
    public partial class TestsManagementView : UserControl
    {
        public TestsManagementView()
        {
            InitializeComponent();
            // لا نقوم بتعيين DataContext هنا.
            // سيتم تعيينه من MainWindow.xaml.cs
        }
    }
}
// نهاية الكود لملف Views/TestsManagement/TestsManagementView.xaml.cs