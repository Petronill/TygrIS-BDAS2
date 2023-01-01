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

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HttpClient client;
        Person? user = null;
        string BASE_ADDRESS = "https://localhost:44333/api/";

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
                TextBlock name = (TextBlock)animal.FindName("name");
                TextBlock sex = (TextBlock)animal.FindName("sex");
                TextBlock species = (TextBlock)animal.FindName("species");
                TextBlock genus = (TextBlock)animal.FindName("genus");
                TextBlock birth = (TextBlock)animal.FindName("birth");
                TextBlock death = (TextBlock)animal.FindName("death");
                TextBlock cost = (TextBlock)animal.FindName("costs");

                //photo.DataContext = new Uri("Items/defaultUser.png");
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
                TextBlock firstName = (TextBlock)employee.FindName("firstName");
                TextBlock secondName = (TextBlock)employee.FindName("secondName");
                TextBlock pin = (TextBlock)employee.FindName("pin");
                TextBlock phone = (TextBlock)employee.FindName("phoneNumber");
                TextBlock email = (TextBlock)employee.FindName("email");

                //photo.DataContext = new Uri("Items/defaultUser.png");
                firstName.Text = content[i].FirstName;
                secondName.Text = content[i].SecondName;
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

        private void OnClickInfo(object sender, RoutedEventArgs e)
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
            Label name = (Label)profileMenu.FindName("name");
            Label age = (Label)profileMenu.FindName("age");
            Label address = (Label)profileMenu.FindName("address");
            Label email = (Label)profileMenu.FindName("email");
            Label phone = (Label)profileMenu.FindName("phone");

            if (content != null)
            {
                name.Content = content.FirstName + " " + content.SecondName;
                age.Content = content.Birthday().ToString("dd. MM. YYYY");
                address.Content = content.Address.Street + " " + content.Address.HouseNumber + " " + content.Address.City;
                email.Content = content.Email;
                phone.Content = content.PhoneNumber;
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
