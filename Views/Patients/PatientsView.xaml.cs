// بداية الكود لملف Views/Patients/PatientsView.xaml.cs
using System.Windows.Controls;
// لم نعد بحاجة لـ using LABOGRA.ViewModels; هنا لأننا لا ننشئ ViewModel

namespace LABOGRA.Views.Patients
{
    public partial class PatientsView : UserControl
    {
        public PatientsView()
        {
            InitializeComponent();
            // لا نقوم بتعيين DataContext هنا.
            // سيتم تعيينه من MainWindow.xaml.cs
        }
    }
}
// نهاية الكود لملف Views/Patients/PatientsView.xaml.cs