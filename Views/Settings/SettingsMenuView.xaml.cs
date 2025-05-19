// الإصدار: 1 (لهذا الملف)
// اسم الملف: SettingsMenuView.xaml.cs
// الوصف: الكود الخلفي لـ SettingsMenuView. لا يحتاج منطقاً إضافياً في MVVM.
using System.Windows.Controls;

namespace LABOGRA.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsMenuView.xaml
    /// </summary>
    public partial class SettingsMenuView : UserControl
    {
        public SettingsMenuView()
        {
            InitializeComponent();
            // الـ DataContext سيتم تعيينه في MainWindow.xaml.cs
        }
    }
}