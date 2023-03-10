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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using System.Reflection;
using System.Threading;
using System.Net.Http.Headers;

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /* 
     * TODO:
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
            if (userName.Content.Equals("Nepřihlášen") && user == null)
            {
                NotLoggedIn(sender, e);
            }
            else
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
                List<StackPanel> elements = new List<StackPanel>();
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
                    elements.Add(animal);

                    photo.MouseDown += (s, e) =>
                    {

                    };
                }

                edit.Click += (s, e) =>
                {
                    for (int i = 0; i < content.Count; i++)
                    {
                        StackPanel actual = elements[i];
                        TextBox name = (TextBox)actual.FindName("name");
                        TextBox sex = (TextBox)actual.FindName("sex");
                        TextBox species = (TextBox)actual.FindName("species");
                        TextBox genus = (TextBox)actual.FindName("genus");
                        TextBox birth = (TextBox)actual.FindName("birth");
                        TextBox death = (TextBox)actual.FindName("death");
                        TextBox cost = (TextBox)actual.FindName("costs");

                        content[i].Name = name.Text;
                        content[i].Species.CzechName = species.Text;
                        content[i].Species.Genus.CzechName = genus.Text;
                        content[i].MaintCosts = Int32.Parse(cost.Text);
                        content[i].Sex.Abbreviation = sex.Text;
                    }
                    var ress = JsonSerializer.Serialize(content);
                    var send = client.PostAsync("Animal/", new StringContent(ress, Encoding.UTF8, "application/json")).Result;
                };

                Content.Children.Add(animalGrid);
            }
        }

        private void OnClickEmployees()
        {
            Content.Children.Clear();
            HttpResponseMessage result = client.GetAsync("Subordinate/" + user.Id).Result;
            string res = result.Content.ReadAsStringAsync().Result;
            var content = JsonSerializer.Deserialize<List<Keeper>>(res);

            Employees employeeList = new Employees();
            Grid employeeGrid = (Grid)XamlReader.Parse(XamlWriter.Save(employeeList.grid));
            StackPanel list = (StackPanel)employeeGrid.FindName("employeeList");
            StackPanel ogThreeEmployees = (StackPanel)list.FindName("threeEmployees");
            StackPanel ogEmployee = (StackPanel)ogThreeEmployees.FindName("employee");
            ogThreeEmployees.Children.Clear();
            StackPanel threeEmployees = XamlReader.Parse(XamlWriter.Save(ogThreeEmployees)) as StackPanel;
            int counter = 0;
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
                counter++;
                if (i % 5 == 4 || i == content.Count - 1)
                {
                    list.Children.Add(threeEmployees);
                    threeEmployees = XamlReader.Parse(XamlWriter.Save(ogThreeEmployees)) as StackPanel;
                }  
            }
            if (counter % 5 != 0)
            {
                list.Children.Add(threeEmployees);
            }
            Content.Children.Add(employeeGrid);
        }

        private async void OnClickInfo(object sender, RoutedEventArgs e)
        {
            if (userName.Content.Equals("Nepřihlášen") && user == null)
            {
                NotLoggedIn(sender, e);
            }
            else
            {
                Content.Children.Clear();
                UserProfile profile = new UserProfile();
                HttpResponseMessage result = client.GetAsync("User/").Result;
                string res = result.Content.ReadAsStringAsync().Result;
                user = JsonSerializer.Deserialize<Person>(res);

                string panel = XamlWriter.Save(profile.profile);
                StackPanel profileMenu = (StackPanel)XamlReader.Parse(panel);
                Button btnAnimal = (Button)profileMenu.FindName("animalList");
                Button btnKeeper = (Button)profileMenu.FindName("keepers");
                Button btnChange = (Button)profileMenu.FindName("pictureChange");
                Button saveChange = (Button)profileMenu.FindName("saveChange");
                Button emulator = (Button)profileMenu.FindName("emulator");
                Image profilePic = (Image)profileMenu.FindName("picture");
                TextBox name = (TextBox)profileMenu.FindName("name");
                TextBox age = (TextBox)profileMenu.FindName("age");
                TextBox address = (TextBox)profileMenu.FindName("address");
                TextBox email = (TextBox)profileMenu.FindName("email");
                TextBox phone = (TextBox)profileMenu.FindName("phone");
                ComboBox users = (ComboBox)profileMenu.FindName("users");
                profilePic = RefreshPhoto(profileMenu);
                //TODO pridat RefreshPhoto na Image, aby se nacetl hnedka...


                HttpResponseMessage people = client.GetAsync("Person/").Result;
                string toString = people.Content.ReadAsStringAsync().Result;
                var userList = JsonSerializer.Deserialize<List<Person>>(toString);

                HttpResponseMessage nowLogged = client.GetAsync("Login/").Result;
                nowLogged.EnsureSuccessStatusCode();
                string bodyOfMessage = nowLogged.Content.ReadAsStringAsync().Result;
                if (Int32.Parse(bodyOfMessage) == 0)
                {
                    users.Visibility = Visibility.Visible;
                    emulator.Visibility = Visibility.Visible;

                    for (int i = 0; i < userList.Count; i++)
                    {
                        users.Items.Add(userList[i].FirstName);
                    }
                    users.SelectedIndex = 0;
                }

                if (user != null)
                {
                    name.Text = user.FirstName + " " + user.LastName;
                    age.Text = user.Birthday().ToString("dd. MM. yyyy");
                    address.Text = user.Address.Street + " " + user.Address.HouseNumber + " " + user.Address.City;
                    email.Text = user.Email;
                    phone.Text = user.PhoneNumber.ToString();
                }

                Content.Children.Add(profileMenu);

                users.SelectionChanged += (s, e) =>
                {
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
                name.TextChanged += (s, e) =>
                {
                    saveChange.IsEnabled = true;
                };
                age.TextChanged += (s, e) =>
                {
                    saveChange.IsEnabled = true;
                };
                address.TextChanged += (s, e) =>
                {
                    saveChange.IsEnabled = true;
                };
                email.TextChanged += (s, e) =>
                {
                    saveChange.IsEnabled = true;
                };
                phone.TextChanged += (s, e) =>
                {
                    saveChange.IsEnabled = true;
                };

                saveChange.Click += (s, e) =>
                {
                    string[] splitName = name.Text.Split(' ');
                    user.FirstName = splitName[0];
                    user.LastName = splitName[1];
                    string[] splitAddress = address.Text.Split(' ');
                    user.Address.Street = splitAddress[0];
                    user.Address.HouseNumber = Int32.Parse(splitAddress[1]);
                    user.Address.City = splitAddress[2];
                    user.Email = email.Text;
                    user.PhoneNumber = Int64.Parse(phone.Text);
                    UserProfileChange();
                };

                btnChange.Click += (s, e) =>
                {
                    profilePic = UserPhotoChange(profileMenu);
                };
                btnAnimal.Click += (s, e) =>
                {
                    OnClickAnimalsOfUser(sender, e);
                };
                btnKeeper.Click += (s, e) =>
                {
                    if (user.Role == PersonalRoles.KEEPER)
                    {
                        OnClickEmployees();
                    }
                    else
                    {
                        OnClickEmployeeOfAnimals();
                    }

                };

                emulator.Click += (s, e) =>
                {
                    Thread thread = new Thread(new ThreadStart(EmulatorShenanigans));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start();
                };


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
            OnClickZoo(sender, e);
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
            BASE_ADDRESS = wholeAddress;
        }

        private Image UserPhotoChange(StackPanel profileMenu)
        {
            PhotoChange();
            UserProfileChange();
            return RefreshPhoto(profileMenu);

        }
        private void PhotoChange()
        {
            if (user != null)
            {
                OpenFileDialog change = new OpenFileDialog();
                change.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
                change.Multiselect = false;
                if (change.ShowDialog() == true)
                {
                    var pic = new Document()
                    {
                        Name = change.SafeFileName,
                        Extension = Path.GetExtension(change.FileName),
                        Data = Document.SerializeBytes(File.ReadAllBytes(change.FileName)),
                    };
                    var content = JsonSerializer.Serialize(pic);

                    var fileID = client.PostAsync("File/", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                    user.PhotoId = Int32.Parse(fileID.Content.ReadAsStringAsync().Result);
                }
            }
        }
        private void UserProfileChange()
        {
            var changedUser = JsonSerializer.Serialize(user);
            client.PostAsync("User/", new StringContent(changedUser, Encoding.UTF8, "application/json"));
        }

        private Image RefreshPhoto(StackPanel profileMenu)
        {
            Image profilePic = (Image)profileMenu.FindName("picture");

            HttpResponseMessage imageFile = client.GetAsync("File/" + user.PhotoId).Result;
            string readToString = imageFile.Content.ReadAsStringAsync().Result;

            Document deserializace = JsonSerializer.Deserialize<Document>(readToString);
            if (deserializace?.Data != null)
            {
                byte[] data = deserializace.GetBytes();
                var image = new BitmapImage();
                using (var mem = new MemoryStream(data))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                profilePic.Source = image;
            }
            else
            {
                profilePic.Source = new BitmapImage(new Uri(@"/Items/defaultUser.png", UriKind.RelativeOrAbsolute));
            }
            return profilePic;
        }

        private async Task NotLoggedIn(object sender, RoutedEventArgs e)
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

        private void OnClickAnimalsOfUser(object sender, RoutedEventArgs e)
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
            List<StackPanel> elements = new List<StackPanel>();
            int counter = 0;
            for (int i = 0; i < content.Count; i++)
            {
                if (content[i].AdopterId == user.Id)
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
                    counter++;
                    if (i % 5 == 4 || i == content.Count - 1)
                    {
                        list.Children.Add(threeAnimals);
                        threeAnimals = XamlReader.Parse(XamlWriter.Save(ogThreeAnimals)) as StackPanel;
                    }
                    elements.Add(animal);
                }
            }
            if (counter % 5 != 0)
            {
                list.Children.Add(threeAnimals);
            }
            Content.Children.Add(animalGrid);

            edit.Click += (s, e) =>
            {
                for (int i = 0; i < content.Count; i++)
                {
                    StackPanel actual = elements[i];
                    TextBox name = (TextBox)actual.FindName("name");
                    TextBox sex = (TextBox)actual.FindName("sex");
                    TextBox species = (TextBox)actual.FindName("species");
                    TextBox genus = (TextBox)actual.FindName("genus");
                    TextBox birth = (TextBox)actual.FindName("birth");
                    TextBox death = (TextBox)actual.FindName("death");
                    TextBox cost = (TextBox)actual.FindName("costs");

                    content[i].Name = name.Text;
                    content[i].Species.CzechName = species.Text;
                    content[i].Species.Genus.CzechName = genus.Text;
                    content[i].MaintCosts = Int32.Parse(cost.Text);
                    content[i].Sex.Abbreviation = sex.Text;
                }
                var ress = JsonSerializer.Serialize(content);
                var send = client.PostAsync("Animal/", new StringContent(ress, Encoding.UTF8, "application/json")).Result;
            };
        }

        /*TODO:Jeste aby to nastavovalo pri zmene uzivatele usera v main window... to bude ostry...
         */
        private void EmulatorShenanigans()
        {
            Emulator profile = new Emulator(user, client);
            profile.Show();
            System.Windows.Threading.Dispatcher.Run();

            /*lock(profileMenu){
            Content.Children.Add(profileMenu);
            }*/
        }

        private void OnClickEmployeeOfAnimals()
        {
            Content.Children.Clear();
            HttpResponseMessage result = client.GetAsync("Keeper/").Result;
            string res = result.Content.ReadAsStringAsync().Result;
            var content = JsonSerializer.Deserialize<List<Keeper>>(res);

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
    }

}
