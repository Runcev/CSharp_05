using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CSharp_05.Tools.Managers;
using CSharp_05.Tools.Navigation;
using CSharp_05.ViewModels;

namespace CSharp_05
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, IContentOwner
    {
        public ContentControl ContentControl => _contentControl;


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            NavigationManager.Instance.Initialize(new InitializationNavigationModel(this));
            NavigationManager.Instance.Navigate(ViewType.DataView);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            StationManager.CloseApp();
        }
    }

}

