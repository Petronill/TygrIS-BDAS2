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
using TISModelLibrary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;


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
            var content = JsonSerializer.Deserialize<Animal>(res);

            AnimalList animalList = new AnimalList();
            //TODO: For cycle with all the animals in database??
            string panel = XamlWriter.Save(animalList.grid);
            Grid animal = (Grid)XamlReader.Parse(panel);
            StackPanel threeAnimals = (StackPanel)animal.FindName("threeAnimals");
            Image photo = (Image)animal.FindName("picture");
            TextBlock name = (TextBlock)animal.FindName("name");
            TextBlock sex = (TextBlock)animal.FindName("sex");
            TextBlock species = (TextBlock)animal.FindName("species");
            TextBlock genus = (TextBlock)animal.FindName("genus");
            TextBlock birth = (TextBlock)animal.FindName("birth");
            TextBlock death = (TextBlock)animal.FindName("death");
            TextBlock cost = (TextBlock)animal.FindName("costs");
            Button donate = (Button)animal.FindName("donate");

            if (content != null)
            {
                name.Text = content.Name;
                species.Text = content.Species.CzechName;
                genus.Text = content.Species.Genus.CzechName;
                birth.Text = content.Birth.ToString();
                if (content.Death.ToString().Length == 0)
                {
                    death.Text = "Ještě žije";
                }
                else
                {
                    death.Text = content.Death.ToString();
                }
                cost.Text = content.MaintCosts.ToString();
                sex.Text = content.Sex.ToString();
            }

            Content.Children.Add(animal);

            donate.Click += (s, e) =>
            {
                //TODO: what the hell am I gonna put here? Take money from the person who is logged in i guess?
            };
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
                age.Content = 45; // TODO: vypočítávat!
                address.Content = content.Address;
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
        private void OnClickEmployees()
        {
            HttpResponseMessage result = client.GetAsync("Person/").Result;
            string res = result.Content.ReadAsStringAsync().Result;
            //var objects = JsonConvert.DeserializeObject(res); 
            var content = JsonSerializer.Deserialize<Person>(res);
            //int count = result.Count;
            Employees employeeList = new Employees();
            //TODO: For cycle with all the Employees in database??
            string panel = XamlWriter.Save(employeeList.grid);
            Grid employee = (Grid)XamlReader.Parse(panel);

            for (int i = 0; i < 10; i++)
            {
                StackPanel threeEmployees = (StackPanel)employee.FindName("threeEmployees");
                Image photo = (Image)employee.FindName("piture");
                TextBlock title = (TextBlock)employee.FindName("title");
                TextBlock firstName = (TextBlock)employee.FindName("firstName");
                TextBlock secondName = (TextBlock)employee.FindName("secondName");
                TextBlock pin = (TextBlock)employee.FindName("pin");
                TextBlock phone = (TextBlock)employee.FindName("phoneNumber");
                TextBlock email = (TextBlock)employee.FindName("email");
                TextBlock wage = (TextBlock)employee.FindName("wage");
                if (result.IsSuccessStatusCode == true)
                {

                    photo.DataContext = new Uri("/Items/defaultUser.png");
                    //title.Text = content.Title.ToString();
                    firstName.Text = content.FirstName;
                    secondName.Text = content.SecondName;
                    pin.Text = content.PIN.ToString();
                    phone.Text = content.PhoneNumber.ToString();
                    email.Text = content.Email;
                    Content.Children.Add(employee);

                }
                else
                {
                    //TODO: ¯\_(ツ)_/¯ nvm jak napsat nějakou hlášku... JSRuntime nebo co nemuzu pry si stahnout
                }
            }
        }
    }
}
