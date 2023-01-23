using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TISModelLibrary;

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for Emulator.xaml
    /// </summary>
    public delegate void UserChanged(Person user);
    public partial class Emulator : Window
    {
        Person user;
        Person admin;
        Animal animal;
        HttpClient client;
        int counter = 0;

        public Emulator(Person user, HttpClient client)
        {
            this.client = client;
            admin = user;
            this.user = user;
            InitializeComponent();
            LoadDataIntoComponents();
            userAnimal.Items.Add("Users");
            userAnimal.Items.Add("Animals");
            userAnimal.SelectedIndex = 0;
        }

        public Person User => user;
        private void LoadDataIntoComponents()
        {
            adminWindow.Children.Clear();
            EmulatorUser emulator = new EmulatorUser();
            string panel = XamlWriter.Save(emulator.adminWindow);
            StackPanel profileMenu = (StackPanel)XamlReader.Parse(panel);
            StackPanel wagePanel = (StackPanel)profileMenu.FindName("wagePanel");
            StackPanel donationPanel = (StackPanel)profileMenu.FindName("donationPanel");
            StackPanel userProfile = (StackPanel)profileMenu.FindName("userProfile");

            ComboBox users = (ComboBox)profileMenu.FindName("users");
            TextBox name = (TextBox)profileMenu.FindName("name");
            TextBox pin = (TextBox)profileMenu.FindName("pin");
            TextBox address = (TextBox)profileMenu.FindName("address");
            TextBox email = (TextBox)profileMenu.FindName("email");
            TextBox phone = (TextBox)profileMenu.FindName("phone");
            TextBox account = (TextBox)profileMenu.FindName("account");
            TextBox donation = (TextBox)profileMenu.FindName("donation");
            TextBox wage = (TextBox)profileMenu.FindName("wage");


            Button btnChange = (Button)profileMenu.FindName("btnChange");
            Button pictureChange = (Button)profileMenu.FindName("pictureChange");
            Image picture = (Image)profileMenu.FindName("picture");
            ListBox loggList = (ListBox)profileMenu.FindName("loggList");

            HttpResponseMessage result = client.GetAsync("User/").Result;
            string res = result.Content.ReadAsStringAsync().Result;
            user = JsonSerializer.Deserialize<Person>(res);

            LoggRefresh(loggList);

            HttpResponseMessage people = client.GetAsync("Person/").Result;
            string toString = people.Content.ReadAsStringAsync().Result;
            var userList = JsonSerializer.Deserialize<List<Person>>(toString);

            for (int i = 0; i < userList.Count; i++)
            {
                users.Items.Add(userList[i].FirstName);
            }
            users.SelectedIndex = 0;

            Keeper keeper = new Keeper();
            Adopter adopter = new Adopter();

            if (user != null)
            {
                if (user.Role == PersonalRoles.KEEPER)
                {
                    HttpResponseMessage response = client.GetAsync("Keeper/" + user.Id).Result;
                    string keeperString = response.Content.ReadAsStringAsync().Result;
                    keeper = JsonSerializer.Deserialize<Keeper>(keeperString);

                    donationPanel.Visibility = Visibility.Hidden;
                    wagePanel.Visibility = Visibility.Visible;
                    //TODO Jak to ale budu ukladat jajajajajajaajajj
                    wage.Text = keeper.GrossWage.ToString();
                }
                else
                {
                    HttpResponseMessage response = client.GetAsync("Adopter/" + user.Id).Result;
                    string adopterString = response.Content.ReadAsStringAsync().Result;
                    adopter = JsonSerializer.Deserialize<Adopter>(adopterString);


                    donationPanel.Visibility = Visibility.Visible;
                    wagePanel.Visibility = Visibility.Hidden;
                    //TODO Jak todle ale potom budu ukladat k sakra jajajajajaj
                    donation.Text = adopter.Donation.ToString();
                }
                name.Text = user.FirstName + " " + user.LastName;
                pin.Text = user.PIN.ToString();
                account.Text = user.AccountNumber.ToString();
                address.Text = user.Address.Street + " " + user.Address.HouseNumber + " " + user.Address.City + " " + user.Address.Country + " " + user.Address.PostalCode;
                email.Text = user.Email;
                phone.Text = user.PhoneNumber.ToString();
            }
            users.SelectionChanged += (s, e) =>
            {
                btnChange.IsEnabled = false;

                //users.Items.Clear();
                //for (int i = 0; i < userList.Count; i++)
                //{
                //    users.Items.Add(userList[i].FirstName);

                //}
                //for (int i = 0; i < users.Items.Count; i++)
                //{
                //    users.SelectedIndex = i;
                //    if (users.SelectedItem.ToString().Equals(user.FirstName))
                //    {
                //        break;
                //    }
                //}

                for (int i = 0; i < userList.Count; i++)
                {
                    if (userList[i].FirstName.Equals(users.SelectedItem.ToString()))
                    {
                        user = userList[i];
                        if (userList[i].Role == PersonalRoles.KEEPER)
                        {
                            donationPanel.Visibility = Visibility.Hidden;
                            wagePanel.Visibility = Visibility.Visible;
                            //TODO Jak to ale budu ukladat jajajajajajaajajj
                            // Treba prevod na Keepera
                            wage.Text = keeper.GrossWage.ToString();
                        }
                        else
                        {
                            donationPanel.Visibility = Visibility.Visible;
                            wagePanel.Visibility = Visibility.Hidden;
                            //TODO Jak todle ale potom budu ukladat k sakra jajajajajaj
                            //Treba prevod na adoptra
                            donation.Text = adopter.Donation.ToString();
                        }
                        name.Text = user.FirstName + " " + user.LastName;
                        pin.Text = user.PIN.ToString();
                        account.Text = user.AccountNumber.ToString();
                        address.Text = user.Address.Street + " " + user.Address.HouseNumber + " " + user.Address.City + " " + user.Address.Country + " " + user.Address.PostalCode;
                        email.Text = user.Email;
                        phone.Text = user.PhoneNumber.ToString();
                        break;
                    }
                }
            };
            name.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            pin.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            address.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            email.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            phone.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            account.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            wage.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            account.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            donation.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };

            btnAdd.Click += (s, e) =>
            {
                if (userAnimal.SelectedItem.ToString() == "Animals")
                {
                    Thread thread = new Thread(new ThreadStart(AddAnimal));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start();

                    LoggRefresh(loggList);

                }
                else if (userAnimal.SelectedItem.ToString() == "Users")
                {
                    Thread thread = new Thread(new ThreadStart(AddUser));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start();

                    LoggRefresh(loggList);
                }

            };

            btnRemove.Click += (s, e) =>
            {
                if (userAnimal.SelectedItem.ToString() == "Animals")
                {
                    client.DeleteAsync("Animal/" + animal.Id);
                    //TODO Checknout jestli tydle 2 věci jsou dobře
                }
                else if (userAnimal.SelectedItem.ToString() == "Users")
                {
                    client.DeleteAsync("Person/" + user.Id);
                }
            };

            userAnimal.SelectionChanged += (s, e) =>
            {
                counter++;
                if (userAnimal.SelectedItem.ToString() == "Animals")
                    AnimalEmulator();
                else if (userAnimal.SelectedItem.ToString() == "Users")
                {
                    LoadDataIntoComponents();
                }

            };

            btnChange.Click += (s, e) =>
        {
            string[] splitName = name.Text.Split(' ');
            user.FirstName = splitName[0];
            user.LastName = splitName[1];
            string[] addressString = address.Text.Split(" ");
            TISModelLibrary.Address adresa = new TISModelLibrary.Address();
            if (addressString.Length == 4)
            {
                adresa.Street = addressString[0];
                adresa.HouseNumber = Int32.Parse(addressString[1]);
                adresa.City = addressString[2];
                adresa.Country = addressString[3];
                adresa.PostalCode = Int32.Parse(addressString[4]);
            }
            else if (addressString.Length == 5)
            {
                adresa.Street = addressString[0];
                adresa.HouseNumber = Int32.Parse(addressString[1]);
                adresa.City = addressString[2];
                adresa.Country = addressString[3] + " " + addressString[4];
                adresa.PostalCode = Int32.Parse(addressString[5]);
            }

            user.Address = adresa;
            user.Email = email.Text;
            user.PhoneNumber = Int64.Parse(phone.Text);
            user.PIN = Int64.Parse(pin.Text);
            user.AccountNumber = Int64.Parse(account.Text);

            if (user.Role == PersonalRoles.KEEPER)
            {
                Keeper keeper = new Keeper();
                keeper.Id = 0;
                keeper.FirstName = user.FirstName;
                keeper.LastName = user.LastName;
                keeper.Email = user.Email;
                keeper.Address = user.Address;
                keeper.AccountNumber = user.AccountNumber;
                keeper.PhoneNumber = user.PhoneNumber;
                keeper.PIN = user.PIN;
                keeper.Role = PersonalRoles.KEEPER;
                keeper.PhotoId = user.PhotoId;

                if (!string.IsNullOrEmpty(wage.Text))
                {
                    keeper.GrossWage = Int32.Parse(wage.Text);
                }
                else
                {
                    keeper.GrossWage = 0;
                }
                keeper.SupervisorId = 0;
                var changedAnimal = JsonSerializer.Serialize(keeper);
                client.PostAsync("Keeper/", new StringContent(changedAnimal, Encoding.UTF8, "application/json"));
            }
            else if (user.Role == PersonalRoles.ADOPTER)
            {
                Adopter adopter = new Adopter();
                adopter.Id = 0;
                adopter.FirstName = user.FirstName;
                adopter.LastName = user.LastName;
                adopter.Email = user.Email;
                adopter.Address = user.Address;
                adopter.AccountNumber = user.AccountNumber;
                adopter.PhoneNumber = user.PhoneNumber;
                adopter.PIN = user.PIN;
                adopter.Role = PersonalRoles.ADOPTER;
                adopter.PhotoId = user.PhotoId;

                if (!string.IsNullOrEmpty(donation.Text))
                {
                    adopter.Donation = Int32.Parse(donation.Text);
                }
                else
                {
                    adopter.Donation = 0;
                }
                var changedAnimal = JsonSerializer.Serialize(adopter);
                client.PostAsync("Adopter/", new StringContent(changedAnimal, Encoding.UTF8, "application/json"));
            }

            users.Items.Clear();
            for (int i = 0; i < userList.Count; i++)
            {
                users.Items.Add(userList[i].FirstName);
            }
            UserProfileChange();
        };

            pictureChange.Click += (s, e) =>
            {
                picture = UserPhotoChange(userProfile);
            };
            loggList.DataContextChanged += (s, e) =>
            {
                LoggRefresh(loggList);
            };

            adminWindow.Children.Add(profileMenu);

        }

        private void LoggRefresh(ListBox loggList)
        {
            HttpResponseMessage loggs = client.GetAsync("Log/").Result;
            string log = loggs.Content.ReadAsStringAsync().Result;
            List<LogEntry> entries = JsonSerializer.Deserialize<List<LogEntry>>(log);
            entries.OrderBy((log) => log.Time);
            foreach (var entry in entries)
            {
                loggList.Items.Add(entry.Table + " " + entry.Event + " " + entry.ToString() + " " + entry.Message);
            }

        }

        private void UserProfileChange()
        {
            var changedUser = JsonSerializer.Serialize(user);
            client.PostAsync("User/", new StringContent(changedUser, Encoding.UTF8, "application/json"));
        }
        private void AnimalProfileChange()
        {
            var changedAnimal = JsonSerializer.Serialize(animal);
            client.PostAsync("Animal/", new StringContent(changedAnimal, Encoding.UTF8, "application/json"));
        }

        private Image UserPhotoChange(StackPanel profileMenu)
        {
            PhotoChange();
            UserProfileChange();
            return RefreshPhoto(profileMenu);

        }
        private Image AnimalPhotoChange(StackPanel profileMenu)
        {
            PhotoChangeAnimal();
            AnimalProfileChange();
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
                        Extension = System.IO.Path.GetExtension(change.FileName),
                        Data = Document.SerializeBytes(File.ReadAllBytes(change.FileName)),
                    };
                    var content = JsonSerializer.Serialize(pic);

                    var fileID = client.PostAsync("File/", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                    user.PhotoId = Int32.Parse(fileID.Content.ReadAsStringAsync().Result);
                }
            }
        }
        private void PhotoChangeAnimal()
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
                        Extension = System.IO.Path.GetExtension(change.FileName),
                        Data = Document.SerializeBytes(File.ReadAllBytes(change.FileName)),
                    };
                    var content = JsonSerializer.Serialize(pic);

                    var fileID = client.PostAsync("File/", new StringContent(content, Encoding.UTF8, "application/json")).Result;
                    animal.PhotoId = Int32.Parse(fileID.Content.ReadAsStringAsync().Result);
                }
            }
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
        private void AnimalEmulator()
        {
            EmulatorAnimal emulator = new EmulatorAnimal();
            adminWindow.Children.Clear();
            string panel = XamlWriter.Save(emulator.animalWindow);
            StackPanel profileMenu = (StackPanel)XamlReader.Parse(panel);
            StackPanel animalProfile = (StackPanel)profileMenu.FindName("animalProfile");
            HttpResponseMessage people = client.GetAsync("Animal/").Result;
            string toString = people.Content.ReadAsStringAsync().Result;
            List<Animal> animalList = JsonSerializer.Deserialize<List<Animal>>(toString);
            animalList.OrderBy((animul) => animul.Name);

            ComboBox animals = (ComboBox)profileMenu.FindName("animals");
            TextBox name = (TextBox)profileMenu.FindName("name");
            TextBox sex = (TextBox)profileMenu.FindName("sex");
            TextBox nameCzech = (TextBox)profileMenu.FindName("nameCzech");
            TextBox nameLatin = (TextBox)profileMenu.FindName("nameLatin");
            TextBox enclosure = (TextBox)profileMenu.FindName("enclosure");
            TextBox birth = (TextBox)profileMenu.FindName("birth");
            TextBox death = (TextBox)profileMenu.FindName("death");
            TextBox support = (TextBox)profileMenu.FindName("support");

            Button btnChange = (Button)profileMenu.FindName("btnChange");
            Button pictureChange = (Button)profileMenu.FindName("pictureChange");
            Image picture = (Image)profileMenu.FindName("picture");
            ListBox loggList = (ListBox)profileMenu.FindName("loggList");

            for (int i = 0; i < animalList.Count; i++)
            {
                animals.Items.Add(animalList[i].Name);
            }
            animals.SelectedIndex = 0;
            animal = animalList[0];

            HttpResponseMessage loggs = client.GetAsync("Log/").Result;
            string log = loggs.Content.ReadAsStringAsync().Result;
            List<LogEntry> entries = JsonSerializer.Deserialize<List<LogEntry>>(log);
            entries.OrderBy((log) => log.Time);
            for (int i = 0; i < entries.Count; i++)
            {
                loggList.Items.Add(entries[i].Table + " " + entries[i].Event + " " + entries[i].Time.ToString() + " " + entries[i].Message);
            }

            if (animal != null)
            {
                name.Text = animal.Name;
                sex.Text = animal.Sex.Abbreviation;
                nameCzech.Text = animal.Species.CzechName + " " + animal.Species.Genus.CzechName;
                nameLatin.Text = animal.Species.LatinName + " " + animal.Species.Genus.LatinName;
                enclosure.Text = animal.Enclosure.Name + " " + animal.Enclosure.Pavilion.Name;
                birth.Text = animal.Birth.ToString();
                if (animal.Death != null)
                {
                    death.Text = animal.Death.ToString();
                }
                support.Text = animal.MaintCosts.ToString();
            }

            animals.SelectionChanged += (s, e) =>
            {
                btnChange.IsEnabled = false;
                for (int i = 0; i < animalList.Count; i++)
                {
                    death.Text = "";
                    if (animalList[i].Name.Equals(animals.SelectedItem.ToString()))
                    {
                        animal = animalList[i];
                        name.Text = animal.Name;
                        sex.Text = animal.Sex.Abbreviation;
                        nameCzech.Text = animal.Species.CzechName + " " + animal.Species.Genus.CzechName;
                        nameLatin.Text = animal.Species.LatinName + " " + animal.Species.Genus.LatinName;
                        //TODO Kaveh mi tady hazi nejakou zas chybu wtf
                        if (animal.Enclosure?.Name != null)
                            enclosure.Text = animal.Enclosure.Name + " ";
                        else if (animal.Enclosure?.Pavilion?.Name != null)
                        {
                            enclosure.Text += animal.Enclosure.Pavilion.Name;
                        }
                        birth.Text = animal.Birth.ToString();
                        if (animal.Death != null)
                        {
                            death.Text = animal.Death.ToString();
                        }
                        support.Text = animal.MaintCosts.ToString();
                        break;
                    };
                }
            };

            name.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            sex.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            nameCzech.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            nameLatin.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            enclosure.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            birth.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            death.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };
            support.TextChanged += (s, e) =>
            {
                btnChange.IsEnabled = true;
            };

            btnChange.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(name.Text))
                {
                    animal.Name = name.Text;
                }
                else
                {
                    animal.Name = " ";
                }
                if (!string.IsNullOrEmpty(nameCzech.Text) || !string.IsNullOrEmpty(nameLatin.Text))
                {
                    string[] speciesCZ = nameCzech.Text.Split(' ');
                    string[] speciesLat = nameLatin.Text.Split(' ');
                    Species species = new Species();
                    species.CzechName = speciesCZ[0];
                    species.LatinName = speciesLat[0];
                    Genus gen = new Genus();
                    gen.CzechName = speciesCZ[1];
                    gen.LatinName = speciesLat[1];
                    species.Genus = gen;
                    animal.Species = species;
                }
                else
                {
                    Genus gen = new Genus();
                    Species spes = new Species();
                    spes.Genus = gen;
                    animal.Species = spes;
                }
                Sex sexyTime = new Sex();
                sexyTime.Abbreviation = sex.Text;
                animal.Sex = sexyTime;
                if (!string.IsNullOrEmpty(birth.Text))
                {
                    animal.Birth = DateTime.Parse(birth.Text);
                }
                else
                {
                    //TODO todle neni dobry uprimne
                    animal.Birth = DateTime.Now;
                }
                if (!string.IsNullOrEmpty(death.Text))
                {
                    animal.Death = DateTime.Parse(death.Text);
                }
                else
                {
                    //TODO todle neni dobry uprimne
                    animal.Death = null;
                }


                if (!string.IsNullOrEmpty(support.Text))
                {
                    animal.MaintCosts = Int32.Parse(support.Text);
                }
                else
                {
                    animal.MaintCosts = 0;
                }

                var changedAnimal = JsonSerializer.Serialize(animal);
                client.PostAsync("Animal/", new StringContent(changedAnimal, Encoding.UTF8, "application/json"));

                AnimalProfileChange();
            };

            pictureChange.Click += (s, e) =>
            {
                picture = AnimalPhotoChange(animalProfile);
            };

            loggList.DataContextChanged += (s, e) =>
            {
                loggList.Items.Clear();
                HttpResponseMessage loggs = client.GetAsync("Log/").Result;
                string log = loggs.Content.ReadAsStringAsync().Result;
                List<LogEntry> entries = JsonSerializer.Deserialize<List<LogEntry>>(log);
                entries.OrderBy((log) => log.Time);
                for (int i = 0; i < entries.Count; i++)
                {
                    loggList.Items.Add(entries[i].Table + " " + entries[i].Event + " " + entries[i].Time.ToString() + " " + entries[i].Message);
                }
            };

            adminWindow.Children.Add(profileMenu);
        }
        private void AddAnimal()
        {
            AddingAnimal profile = new AddingAnimal(client);
            profile.Show();
            System.Windows.Threading.Dispatcher.Run();
        }
        private void AddUser()
        {
            AddingUser profile = new AddingUser(client);
            profile.Show();
            System.Windows.Threading.Dispatcher.Run();
        }
    }
}
