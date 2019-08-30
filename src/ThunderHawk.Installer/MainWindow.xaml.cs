using System.IO;
using System.Windows;

namespace ThunderHawk.Installer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Text;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
