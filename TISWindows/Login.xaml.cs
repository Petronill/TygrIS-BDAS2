using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TISWindows
{
    /// <summary>
    /// Interakční logika pro Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        private void OnClickLogIn(object sender, RoutedEventArgs e)
        {
            // This is useless, can be deleted but I am too afraid to do so
            MainWindow mainWindow = new MainWindow();
            mainWindow.userName.Content = loginName.GetLineText(0);
        }
    }
}
