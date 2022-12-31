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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Net.Http;

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClickZoo(object sender, RoutedEventArgs e)
        {
            Content.Children.Clear();

            MainWindow mainWindow = new MainWindow();

            string panel = XamlWriter.Save(mainWindow.Content);
            DockPanel content = (DockPanel)XamlReader.Parse(panel);
            Button btnAnimal = (Button)content.FindName("animalButton");
            Button btnJob = (Button)content.FindName("findOut");
            btnAnimal.Click += (s, e) =>
            {
                OnClickAnimals(sender, e);
            };
            btnJob.Click += (s, e) =>
            {
                OnClickEmployee(sender, e);
            };

            Content.Children.Add(content);

        }
        private void OnClickAnimals(object sender, RoutedEventArgs e)
        {
            Content.Children.Clear();
            AnimalList animalList = new AnimalList();

            string panel = XamlWriter.Save(animalList.animalList);
            StackPanel animalMenu = (StackPanel)XamlReader.Parse(panel);
            TextBlock name = (TextBlock)animalMenu.FindName("name");
            TextBlock species = (TextBlock)animalMenu.FindName("species");
            TextBlock genus = (TextBlock)animalMenu.FindName("genus");
            TextBlock birth = (TextBlock)animalMenu.FindName("birth");
            TextBlock death = (TextBlock)animalMenu.FindName("death");
            TextBlock cost = (TextBlock)animalMenu.FindName("costs");
            Button donate = (Button)animalMenu.FindName("donate");
            Content.Children.Add(animalMenu);

            donate.Click += (s, e) =>
            {
                //TODO: what the hell am I gonna put here? Take money from the person who is logged in i guess?
            };



        }
        private void OnClickInfo(object sender, RoutedEventArgs e)
        {
            Content.Children.Clear();
            UserProfile profile = new UserProfile();

            string panel = XamlWriter.Save(profile.profile);
            StackPanel profileMenu = (StackPanel)XamlReader.Parse(panel);
            Button btnAnimal = (Button)profileMenu.FindName("animalsList");
            Button btnKeeper = (Button)profileMenu.FindName("keepers");
            btnAnimal.Click += (s, e) =>
            {
                OnClickAnimals(sender, e);
            };
            btnKeeper.Click += (s, e) =>
            {
                //TODO: Make a list of employees(nebo jak se to splluje nvm).... like the animal list
            };

            Content.Children.Add(profileMenu);

        }
        private async void OnClickEmployee(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo { FileName = @"https://www.youtube.com/watch?v=a3Z7zEc7AXQ", UseShellExecute = true });
                }
                catch (System.ComponentModel.Win32Exception noBrowser)
                {
                    if (noBrowser.ErrorCode == -2147467259)
                        MessageBox.Show(noBrowser.Message);
                }
                catch (System.Exception other)
                {
                    MessageBox.Show(other.Message);
                }
            });
        }

        private void OnClickLogin(object sender, RoutedEventArgs e)
        {
            client.BaseAddress = new Uri("https://localhost:44333/api/login/");
            client.DefaultRequestHeaders.Accept.Clear();
            Content.Children.Clear();
            Login login = new Login();
            string panel = XamlWriter.Save(login.loginMenu);
            StackPanel loginMenu = (StackPanel)XamlReader.Parse(panel);
            Button btn = (Button)loginMenu.FindName("pokus");
            TextBox name = (TextBox)loginMenu.FindName("loginName");
            TextBox pswrd = (TextBox)loginMenu.FindName("loginPassword");
            client.DefaultRequestHeaders.Add("Tis-User", name.Text);
            client.DefaultRequestHeaders.Add("Tis-Hash", pswrd.Text);
            HttpResponseMessage result = client.GetAsync(client.BaseAddress).Result;
            result.EnsureSuccessStatusCode();
            string bodyOfMessage = result.Content.ReadAsStringAsync().Result;
            btn.Click += (s, e) =>
            {
                //TODO: if statement with a method, that takes the user and checks, if it's really him
                if (Int32.Parse(bodyOfMessage) > 0)
                {
                    userName.Content = name.Text;
                    OnClickZoo(sender, e);
                }
                else
                {
                    Label lbl = (Label)loginMenu.FindName("errMsg");
                    lbl.Content = "Špatně zadané jméno/heslo, zkuste znovu.";
                }
            };
            Content.Children.Add(loginMenu);
        }
    }
}
