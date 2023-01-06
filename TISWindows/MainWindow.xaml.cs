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
using System.Reflection;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /* 
     * TODO: 4. Combobox pro Admina pro "simulování" jiných uživatelů
     * 
     * TODO: ukladani Editovanych prvku
     * 
     */

    public partial class MainWindow : Window
    {
        HttpClient client;
        Person? user = null;
        static string BASE_ADDRESS = "";
        static int skipAddress = 0;

        public MainWindow()
        {
            if (skipAddress == 0)
            {
                CreateAddress();
                skipAddress = 1;
            }
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
            Button edit = (Button)animalGrid.FindName("editSave");
            ogThreeAnimals.Children.Clear();
            StackPanel threeAnimals = XamlReader.Parse(XamlWriter.Save(ogThreeAnimals)) as StackPanel;
            for (int i = 0; i < content.Count; i++)
            {
                StackPanel animal = XamlReader.Parse(XamlWriter.Save(ogAnimal)) as StackPanel;
                Image photo = (Image)ogThreeAnimals.FindName("picture");
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
            edit.Click += (s, e) =>
            {
                var ress = JsonSerializer.Serialize(content);
                var send = client.PostAsync("Animal/",new StringContent(ress,Encoding.UTF8, "application/json")).Result;
            };

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
                ComboBox users = (ComboBox)profileMenu.FindName("users");

                HttpResponseMessage people = client.GetAsync("Person/").Result;
                string toString = people.Content.ReadAsStringAsync().Result;
                var userList = JsonSerializer.Deserialize<List<Person>>(toString);

                HttpResponseMessage nowLogged = client.GetAsync("Login/").Result;
                nowLogged.EnsureSuccessStatusCode();
                string bodyOfMessage = nowLogged.Content.ReadAsStringAsync().Result;
                if (Int32.Parse(bodyOfMessage) == 0)
                {
                    users.Visibility = Visibility.Visible;

                    for (int i = 0; i < userList.Count; i++)
                    {
                        users.Items.Add(userList[i].FirstName);
                    }
                    users.SelectedIndex = 0;
                }
                else
                {
                    if (content != null)
                    {
                        name.Text = content.FirstName + " " + content.LastName;
                        age.Text = content.Birthday().ToString("dd. MM. yyyy");
                        address.Text = content.Address.Street + " " + content.Address.HouseNumber + " " + content.Address.City;
                        email.Text = content.Email;
                        phone.Text = content.PhoneNumber.ToString();
                    }
                }
                users.SelectionChanged += (s, e) =>
                {
                    var peopleIWant = new Person();
                    for (int i = 0; i < userList.Count; i++)
                    {
                        if (userList[i].FirstName.Equals(users.SelectedItem.ToString()))
                        {
                            name.Text = userList[i].FirstName + " " + userList[i].LastName;
                            age.Text = userList[i].Birthday().ToString("dd. MM. yyyy");
                            address.Text = userList[i].Address.Street + " " + userList[i].Address.HouseNumber + " " + userList[i].Address.City;
                            email.Text = userList[i].Email;
                            phone.Text = userList[i].PhoneNumber.ToString();
                            break;
                        }
                    }
                };

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

        private void CreateAddress()
        {
            //"https://localhost:42069/api/";
            string wholeAddress = "https://";
            Address addressWindow = new Address();
            string panel = XamlWriter.Save(addressWindow.addressFinder);
            StackPanel addressMenu = (StackPanel)XamlReader.Parse(panel);
            Button def = (Button)addressMenu.FindName("default");
            Button accept = (Button)addressMenu.FindName("accept");
            TextBox address = (TextBox)addressMenu.FindName("address");

            Window window = new Window();
            window.Title = "zadávání adresy";
            window.Content = addressMenu;
                def.Click += (s, e) =>
                {

                    wholeAddress += "localhost:42069/api/";
                    window.Close();
                };
                accept.Click += (s, e) =>
                {
                    wholeAddress += address.Text + "/api/";
                    window.Close();
                };
            window.ShowDialog();
            BASE_ADDRESS= wholeAddress;
        }
    }
}
