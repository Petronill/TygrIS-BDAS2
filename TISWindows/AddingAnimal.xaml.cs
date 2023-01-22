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
    /// Interaction logic for AddingAnimal.xaml
    /// </summary>
    public partial class AddingAnimal : Window
    {
        Animal animal;
        HttpClient client;

        public AddingAnimal(HttpClient client)
        {
            this.client = client;
            InitializeComponent();
            sex.Items.Add("F");
            sex.Items.Add("M");
            CreateAnimal();
        }

        private void CreateAnimal()
        {
            HttpResponseMessage response = client.GetAsync("Enclosure/").Result;
            string enclosureString = response.Content.ReadAsStringAsync().Result;
            List<Enclosure> enclosur = JsonSerializer.Deserialize<List<Enclosure>>(enclosureString);

            for (int i = 0; i < enclosur.Count; i++)
            {
                enclosure.Items.Add(enclosur[i].Name);
            }

            HttpResponseMessage response2 = client.GetAsync("Pavilion/").Result;
            string pavilionString = response.Content.ReadAsStringAsync().Result;
            List<Pavilion> pavil = JsonSerializer.Deserialize<List<Pavilion>>(pavilionString);

            for (int i = 0; i < pavil.Count; i++)
            {
                pavilon.Items.Add(pavil[i].Name);
            }

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
                if (!string.IsNullOrEmpty(nameCzech.Text)) { 
                string[] speciesCZ = nameCzech.Text.Split(' ');
                animal.Species.CzechName = speciesCZ[0];
                animal.Species.Genus.CzechName = speciesCZ[1];
                }
                else
                {
                    animal.Species.CzechName = " ";
                    animal.Species.Genus.CzechName = " ";
                }
                if (!string.IsNullOrEmpty(nameLatin.Text)) { 
                string[] speciesLat = nameLatin.Text.Split(' ');
                animal.Species.LatinName = speciesLat[0];
                animal.Species.Genus.LatinName = speciesLat[1];
                }
                else
                {
                    animal.Species.LatinName = " ";
                    animal.Species.Genus.LatinName = " ";
                }
                animal.Sex.Abbreviation = sex.SelectedItem.ToString();
                for (int i = 0; i < enclosur.Count; i++)
                {
                    if (enclosur[i].Name.Equals(enclosure.SelectedItem.ToString()))
                    {
                        animal.Enclosure = enclosur[i];
                        break;
                    }
                }
                for (int i = 0; i < pavil.Count; i++)
                {
                    if (pavil[i].Name.Equals(pavilon.SelectedItem.ToString()))
                    {
                        animal.Enclosure.Pavilion = pavil[i];
                        break;
                    }
                }
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
                    animal.Death = DateTime.Now;
                }


                if (!string.IsNullOrEmpty(support.Text))
                {
                    animal.MaintCosts = Int32.Parse(support.Text);
                }
                else
                {
                    animal.MaintCosts = 0;
                }

                if (!string.IsNullOrEmpty(keeperId.Text))
                {
                    animal.KeeperId= Int32.Parse(keeperId.Text);
                }
                else
                {
                    animal.KeeperId = 0;
                }

                if (!string.IsNullOrEmpty(adopterId.Text))
                {
                    animal.AdopterId = Int32.Parse(adopterId.Text);
                }
                else
                {
                    animal.AdopterId= 0;
                }

                var changedAnimal = JsonSerializer.Serialize(animal);
                client.PostAsync("Animal/", new StringContent(changedAnimal, Encoding.UTF8, "application/json"));
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
                    animal.PhotoId = Int32.Parse(fileID.Content.ReadAsStringAsync().Result);
                }
            };
        }


    }
}
