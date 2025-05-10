using System.Windows.Controls;
using LABOGRA.ViewModels;

namespace LABOGRA.Views.Print
{
    /// <summary>
    /// منطق التفاعل لـ PrintView.xaml
    /// </summary>
    public partial class PrintView : UserControl
    {
        public PrintView()
        {
            InitializeComponent();
            // تعيين الـ DataContext للعرض إلى نسخة جديدة من PrintViewModel
            // هذا يربط الواجهة (XAML) بالمنطق والبيانات في الـ ViewModel
            DataContext = new PrintViewModel();
        }
    }
}