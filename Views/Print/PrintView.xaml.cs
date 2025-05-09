using System.Windows.Controls; // يجب أن يكون هذا الـ using لأن PrintView أصبحت UserControl
using LABOGRA.ViewModels; // لاستخدام الـ ViewModel

namespace LABOGRA.Views.Print
{
    /// <summary>
    /// Interaction logic for PrintView.xaml
    /// </summary>
    // تم تغيير الفئة لترث من UserControl بدلاً من Window
    public partial class PrintView : UserControl
    {
        public PrintView()
        {
            InitializeComponent();
            // تعيين الـ DataContext للـ View إلى نسخة جديدة من PrintViewModel
            // هذا يربط الواجهة (XAML) بالمنطق والبيانات في الـ ViewModel
            DataContext = new PrintViewModel();
        }
    }
}