using LABOGRA.ViewModels;
using System.Windows.Controls;

namespace LABOGRA.Views.Results
{
    public partial class ResultsView : UserControl
    {
        public ResultsView()
        {
            InitializeComponent();
            // DataContext = new ResultsViewModel(); // يفضل تعيينه من XAML أو عند الحاجة
        }
    }
}