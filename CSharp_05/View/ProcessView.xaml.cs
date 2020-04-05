using System.Windows.Controls;
using CSharp_05.Tools.Navigation;
using CSharp_05.ViewModels;

namespace CSharp_05.View
{
    /// <summary>
    /// Interaction logic for ProcessView.xaml
    /// </summary>
    public partial class ProcessView : UserControl, INavigatable
    {
        public ProcessView()
        {
            InitializeComponent();
            DataContext = new ProcessViewModel();
        }
    }
}