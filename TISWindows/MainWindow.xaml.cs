using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using TISModelLibrary;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Timers;

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /* 
     * TODO: 3. Dokumentace aplikace - cca 80% done
     * 
     * TODO: 4. Combobox pro Admina pro "simulování" jiných uživatelů
     * 
     * TODO: 6. Opravit nezobrazujici se obrazek u zvirat - nvm jak + mozna by se hodil nejaky scrollbar
     * 
     * TODO: ukladani Editovanych prvku
     */

    public partial class MainWindow : Window
    {
        HttpClient client;
        Person? user = null;
        string BASE_ADDRESS = "https://localhost:42069/api/";

        public MainWindow()
        {
            InitializeComponent();
            SetUpClient();
        }

        private void SetUpClient()
        {
            if (client != null)
            {
                client.Dispose();
            }
            client = new HttpClient();
            client.BaseAddress = new Uri(BASE_ADDRESS);
            client.DefaultRequestHeaders.Accept.Clear();
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
            HttpResponseMessage result = client.GetAsync("Animal/").Result;
            string res = result.Content.ReadAsStringAsync().Result;
            var content = JsonSerializer.Deserialize<List<Animal>>(res);

            AnimalList animalList = new AnimalList();
            Grid animalGrid = (Grid)XamlReader.Parse(XamlWriter.Save(animalList.grid));
            StackPanel list = (StackPanel)animalGrid.FindName("animalList");
            StackPanel ogThreeAnimals = (StackPanel)list.FindName("threeAnimals");
            StackPanel ogAnimal = (StackPanel)ogThreeAnimals.FindName("animal");
            ogThreeAnimals.Children.Clear();
            StackPanel threeAnimals = XamlReader.Parse(XamlWriter.Save(ogThreeAnimals)) as StackPanel;
            for (int i = 0; i < content.Count; i++)
            {
                StackPanel animal = XamlReader.Parse(XamlWriter.Save(ogAnimal)) as StackPanel;
                Image photo = (Image)animal.FindName("picture");
                photo.Source = new BitmapImage(new Uri(@"/Items/defaultAnimal.jpg", UriKind.RelativeOrAbsolute));
                photo.Width = 100;
                TextBox name = (TextBox)animal.FindName("name");
                TextBox sex = (TextBox)animal.FindName("sex");
                TextBox species = (TextBox)animal.FindName("species");
                TextBox genus = (TextBox)animal.FindName("genus");
                TextBox birth = (TextBox)animal.FindName("birth");
                TextBox death = (TextBox)animal.FindName("death");
                TextBox cost = (TextBox)animal.FindName("costs");

                name.Text = content[i].Name;
                species.Text = content[i].Species.CzechName;
                genus.Text = content[i].Species.Genus.CzechName;
                birth.Text = content[i].Birth.ToString("dd. MM. yyyy");
                death.Text = content[i].Death?.ToString("dd. MM. yyyy") ?? "Ještě žije";
                cost.Text = content[i].MaintCosts.ToString() + " Kč";
                sex.Text = content[i].Sex.Abbreviation;

                threeAnimals.Children.Add(animal);
                if (i % 5 == 4 || i == content.Count - 1)
                {
                    list.Children.Add(threeAnimals);
                    threeAnimals = XamlReader.Parse(XamlWriter.Save(ogThreeAnimals)) as StackPanel;
                }
            }
            Content.Children.Add(animalGrid);
        }

        private void OnClickEmployees()
        {
            Content.Children.Clear();
            HttpResponseMessage result = client.GetAsync("Keeper/").Result;
            string res = result.Content.ReadAsStringAsync().Result;
            var content = JsonSerializer.Deserialize<List<Person>>(res);

            Employees employeeList = new Employees();
            Grid employeeGrid = (Grid)XamlReader.Parse(XamlWriter.Save(employeeList.grid));
            StackPanel list = (StackPanel)employeeGrid.FindName("employeeList");
            StackPanel ogThreeEmployees = (StackPanel)list.FindName("threeEmployees");
            StackPanel ogEmployee = (StackPanel)ogThreeEmployees.FindName("employee");
            ogThreeEmployees.Children.Clear();
            StackPanel threeEmployees = XamlReader.Parse(XamlWriter.Save(ogThreeEmployees)) as StackPanel;
            for (int i = 0; i < content.Count; i++)
            {
                StackPanel employee = XamlReader.Parse(XamlWriter.Save(ogEmployee)) as StackPanel;
                Image photo = (Image)employee.FindName("piture");
                TextBox firstName = (TextBox)employee.FindName("firstName");
                TextBox secondName = (TextBox)employee.FindName("secondName");
                TextBox pin = (TextBox)employee.FindName("pin");
                TextBox phone = (TextBox)employee.FindName("phoneNumber");
                TextBox email = (TextBox)employee.FindName("email");

                firstName.Text = content[i].FirstName;
                secondName.Text = content[i].LastName;
                pin.Text = content[i].PIN.ToString();
                phone.Text = content[i].PhoneNumber.ToString();
                email.Text = content[i].Email;

                threeEmployees.Children.Add(employee);
                if (i % 5 == 4 || i == content.Count - 1)
                {
                    list.Children.Add(threeEmployees);
                    threeEmployees = XamlReader.Parse(XamlWriter.Save(ogThreeEmployees)) as StackPanel;
                }
            }
            Content.Children.Add(employeeGrid);
        }

        private async void OnClickInfo(object sender, RoutedEventArgs e)
        {
            if (userName.Content.Equals("Nepřihlášen"))
            {
                Image warning = new Image();
                Content.Children.Clear();
                warning.Source = new BitmapImage(new Uri(@"/Items/nejstePrihlasen.jpg", UriKind.RelativeOrAbsolute));
                warning.Width = Content.Width;
                warning.Height = Content.Height;
                warning.Margin = new Thickness(0, 0, 140, 0);
                Content.Children.Add(warning);
                await Task.Delay(2500);
                OnClickZoo(sender, e);
            }
            else
            {
                Content.Children.Clear();
                UserProfile profile = new UserProfile();
                HttpResponseMessage result = client.GetAsync("User/").Result;
                string res = result.Content.ReadAsStringAsync().Result;
                var content = JsonSerializer.Deserialize<Person>(res);

                string panel = XamlWriter.Save(profile.profile);
                StackPanel profileMenu = (StackPanel)XamlReader.Parse(panel);
                Button btnAnimal = (Button)profileMenu.FindName("animalList");
                Button btnKeeper = (Button)profileMenu.FindName("keepers");
                TextBox name = (TextBox)profileMenu.FindName("name");
                TextBox age = (TextBox)profileMenu.FindName("age");
                TextBox address = (TextBox)profileMenu.FindName("address");
                TextBox email = (TextBox)profileMenu.FindName("email");
                TextBox phone = (TextBox)profileMenu.FindName("phone");

               
                if (content != null)
                {
                    name.Text = content.FirstName + " " + content.LastName;
                    age.Text = content.Birthday().ToString("dd. MM. yyyy");
                    address.Text = content.Address.Street + " " + content.Address.HouseNumber + " " + content.Address.City;
                    email.Text = content.Email;
                    phone.Text = content.PhoneNumber.ToString();
                }

                btnAnimal.Click += (s, e) =>
                {
                    OnClickAnimals(sender, e);
                };
                btnKeeper.Click += (s, e) =>
                {
                    OnClickEmployees();
                };

                Content.Children.Add(profileMenu);

            }

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
            Content.Children.Clear();
            Login login = new Login();
            string panel = XamlWriter.Save(login.loginMenu);
            StackPanel loginMenu = (StackPanel)XamlReader.Parse(panel);
            Button btn = (Button)loginMenu.FindName("pokus");
            TextBox name = (TextBox)loginMenu.FindName("loginName");
            TextBox pswrd = (TextBox)loginMenu.FindName("loginPassword");
            btn.Click += (s, e) =>
            {
                SetUpClient();
                client.DefaultRequestHeaders.Add("Tis-User", name.Text);
                client.DefaultRequestHeaders.Add("Tis-Hash", pswrd.Text);
                HttpResponseMessage result = client.GetAsync("Login/").Result;
                result.EnsureSuccessStatusCode();
                string bodyOfMessage = result.Content.ReadAsStringAsync().Result;
                if (Int32.Parse(bodyOfMessage) >= 0)
                {
                    userName.Content = name.Text;
                    loggOut.IsEnabled = true;
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
        private void OnClickLogOut(object sender, RoutedEventArgs e)
        {
            userName.Content = "Nepřihlášen";
            user = null;
            client.DefaultRequestHeaders.Clear();
            loggOut.IsEnabled = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();

        }
    }
}
