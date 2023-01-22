using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TISModelLibrary;

namespace TISWindows
{
    /// <summary>
    /// Interaction logic for AddingUser.xaml
    /// </summary>
    public partial class AddingUser : Window
    {
        Person user;
        HttpClient client;
        public AddingUser(HttpClient client)
        {
            this.client = client;
            InitializeComponent();
            role.Items.Add("KEEPER");
            role.Items.Add("ADOPTER");
        }

        private void CreateUser()
        {
            role.SelectionChanged += (s, e) =>
            {
                if (role.SelectedItem.ToString().Equals("KEEPER"))
                {
                    donationPanel.Visibility = Visibility.Hidden;
                    wagePanel.Visibility = Visibility.Visible;
                    supervisorPanel.Visibility = Visibility.Visible;

                }
                else if (role.SelectedItem.ToString().Equals("ADOPTER"))
                {
                    donationPanel.Visibility = Visibility.Visible;
                    wagePanel.Visibility = Visibility.Hidden;
                    supervisorPanel.Visibility = Visibility.Hidden;
                }
            };

            pictureChange.Click += (s, e) =>
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
            };

            btnChange.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(name.Text))
                {
                    string[] wholeName = name.Text.Split(' ');
                    user.FirstName = wholeName[0];
                    user.LastName = wholeName[1];
                }
                else
                {
                    user.FirstName = " ";
                    user.LastName = " ";
                }
                if (!string.IsNullOrEmpty(pin.Text))
                {
                    user.PIN = Int64.Parse(pin.Text);
                }
                else
                {
                    user.PIN = 0;
                }

                if (!string.IsNullOrEmpty(address.Text))
                {
                    string[] addressString = address.Text.Split(' ');
                    user.Address.Street = addressString[0];
                    user.Address.HouseNumber = Int32.Parse(addressString[1]);
                    user.Address.City = addressString[2];
                    user.Address.PostalCode = Int32.Parse(addressString[3]);
                    user.Address.Country = addressString[4];

                }
                else
                {
                    user.Address.Street = " ";
                    user.Address.HouseNumber = 0;
                    user.Address.City = " ";
                    user.Address.PostalCode = 0;
                    user.Address.Country = " ";
                }

                if (!string.IsNullOrEmpty(email.Text))
                {
                    user.Email = (email.Text);
                }
                else
                {
                    user.Email = " ";
                }
                if (!string.IsNullOrEmpty(phone.Text))
                {
                    user.PhoneNumber = Int64.Parse(phone.Text);
                }
                else
                {
                    user.PhoneNumber = 0;
                }

                if (!string.IsNullOrEmpty(account.Text))
                {
                    user.AccountNumber = Int64.Parse(account.Text);
                }
                else
                {
                    user.AccountNumber = 0;
                }

                if (role.SelectedItem.ToString().Equals("KEEPER"))
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
                    if (!string.IsNullOrEmpty(supervisorId.Text))
                    {
                        keeper.SupervisorId = Int32.Parse(supervisorId.Text);
                    }
                    else
                    {
                        keeper.SupervisorId = 0;
                    }

                    var changedAnimal = JsonSerializer.Serialize(keeper);
                    client.PostAsync("Keeper/", new StringContent(changedAnimal, Encoding.UTF8, "application/json"));
                }
                else if (role.SelectedItem.ToString().Equals("ADOPTER"))
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

            };

        }
    }
}
