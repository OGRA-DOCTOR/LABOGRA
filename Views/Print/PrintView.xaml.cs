// بداية الكود لملف Views/Print/PrintView.xaml.cs
using System.Windows.Controls;
// لم نعد بحاجة لـ using LABOGRA.ViewModels; هنا

namespace LABOGRA.Views.Print
{
    public partial class PrintView : UserControl
    {
        public PrintView()
        {
            InitializeComponent();
            // لا نقوم بتعيين DataContext هنا.
            // سيتم تعيينه من MainWindow.xaml.cs
        }
    }
}
// نهاية الكود لملف Views/Print/PrintView.xaml.cs