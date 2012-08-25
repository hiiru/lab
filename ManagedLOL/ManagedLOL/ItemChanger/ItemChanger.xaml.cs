using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using ManagedLOL.Configuration;
using ManagedLOL.ItemChanger.Items;

namespace ManagedLOL.ItemChanger
{
    /// <summary>
    /// Interaction logic for ItemChanger.xaml
    /// </summary>
    public partial class ItemChanger : Window
    {
        public bool IsClosed { get; private set; }

        private Dictionary<string, string> Champions;

        private string SelectedChampion;

        private string configFolder { get { return SettingsHelper.LolPathItemsCharacters + @"\" + SelectedChampion; } }

        private string configFileClassic { get { return configFolder + @"\RecItemsCLASSIC.ini"; } }

        private string configFileDominion { get { return configFolder + @"\RecItemsODIN.ini"; } }

        private List<ItemControl> itemSlots;
        private List<Item> items;

        //1 = classic, 2 = dominion
        private int Mode = 1;

        public ItemChanger()
        {
            InitializeComponent();
            this.Closed += delegate(object sender, EventArgs args) { IsClosed = true; };
            dockpanel.Visibility = System.Windows.Visibility.Hidden;
            if (SettingsHelper.HasLolPath)
            {
                Champions = new Dictionary<string, string>();
                foreach (string path in Directory.GetFiles(SettingsHelper.LolPathImagesChampions, SettingsHelper.ChampionPortraitSearchPattern))
                {
                    FileInfo file = new FileInfo(path);
                    string name = file.Name.Substring(0, file.Name.IndexOf('_'));
                    Champions.Add(name, path);
                }
                if (Champions.Count == 0)
                {
                    this.Hide();
                    MessageBox.Show("Couldn't load Champons", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }

                ddlHero.ItemsSource = Champions;

                if (File.Exists("items.xml"))
                {
                    //TODO parse xml
                    XmlSerializer ser = new XmlSerializer(typeof(List<Item>));
                    FileStream fs = new FileStream("items.xml", FileMode.Open);
                    items = (List<Item>)ser.Deserialize(fs);
                    fs.Close();
                }
                else
                {
                    var result = MessageBox.Show("No items.xml found, should I try to enumerate the items?\n\nNote: not all items might be found, correctly named or duplicated.\nThose that are found might not all work ingame", "Items not found!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        items = new List<Item>();
                        foreach (string path in Directory.GetFiles(SettingsHelper.LolPathImagesItems))
                        {
                            FileInfo file = new FileInfo(path);
                            int index = file.Name.IndexOf('_');
                            if (index == -1) continue;
                            string strid = file.Name.Substring(0, index);
                            string name = file.Name.Substring(index + 1, file.Name.Length - file.Extension.Length - index - 1).Replace('_', ' ');
                            int id = 0;
                            if (int.TryParse(strid, out id) && id > 1000)
                            {
                                var item = new Item(id, name, path);
                                items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        this.Close();
                        return;
                    }
                }
                LoadItems();

                itemSlots = new List<ItemControl>();
                for (int i = 0; i < 6; i++)
                {
                    var itemControl = new ItemControl();
                    itemControl.IsMainSlot = true;
                    itemControl.MainSlotNumber = i;
                    itemControl.Item = null;
                    itemSlots.Add(itemControl);
                    wrapItemSlots.Children.Add(itemControl);
                }

                this.Show();
            }
            else
            {
                this.Hide();
                MessageBox.Show("League of Legends Path not Found.\nPlease make sure the settings are correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void LoadItems()
        {
            if (items == null) return;
            List<Item> toDisplay = items.Where(x => x.Available == Item.ItemAvailability.Both).ToList();
            switch (Mode)
            {
                case 1:
                    toDisplay.AddRange(items.Where(x => x.Available == Item.ItemAvailability.Classic));
                    break;

                case 2:
                    toDisplay.AddRange(items.Where(x => x.Available == Item.ItemAvailability.Dominion));
                    break;
            }
            if (SelectedChampion == "Viktor") toDisplay.AddRange(items.Where(x => x.Available == Item.ItemAvailability.Viktor));
            wrapItems.Children.Clear();
            foreach (Item item in toDisplay)
            {
                var itemControl = new ItemControl();
                itemControl.Item = item;
                wrapItems.Children.Add(itemControl);
            }
        }

        private void ClearItems()
        {
            if (itemSlots == null) return;
            foreach (ItemControl ic in itemSlots)
            {
                ic.Item = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!SettingsHelper.HasLolPath)
            {
                this.Hide();
                MessageBox.Show("League of Legends Path not Found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            if (!Directory.Exists(SettingsHelper.LolPathItemsCharacters))
                Directory.CreateDirectory(SettingsHelper.LolPathItemsCharacters);
        }

        private void ddlHero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyValuePair<string, string> value = (KeyValuePair<string, string>)ddlHero.SelectedValue;
            lblName.Content = SelectedChampion = value.Key;
            if (File.Exists(SettingsHelper.LolPathImagesChampions + @"\" + SelectedChampion + @"_0.jpg"))
                imgBackground.Source = new BitmapImage(new Uri(SettingsHelper.LolPathImagesChampions + @"\" + SelectedChampion + @"_0.jpg"));
            dockpanel.Visibility = System.Windows.Visibility.Visible;

            LoadConfig(configFileClassic);
            btnSave.IsEnabled = true;
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
        string key,
        string val,
        string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

        private void LoadConfig(string path)
        {
            ClearItems();
            string configFile;
            switch (Mode)
            {
                case 1:
                    configFile = configFileClassic;
                    break;

                case 2:
                    configFile = configFileDominion;
                    break;
                default:
                    return;
            }
            if (File.Exists(configFile))
            {
                int slotId = 0;
                foreach (ItemControl itemControl in itemSlots)
                {
                    slotId++;
                    if (slotId >= 7) break;
                    if (itemControl == null || itemControl.Item == null) continue;
                    StringBuilder SB = new StringBuilder(10);
                    int i = GetPrivateProfileString("ItemSet1", "RecItem" + slotId, "0", SB, 10, configFile);
                    int id = 0;
                    if (int.TryParse(SB.ToString(), out id) && id != 0)
                    {
                        var item = items.FirstOrDefault(x => x.ID == id);
                        itemControl.Item = item;
                    }
                    else
                        itemControl.Item = null;
                }
                btnDelete.IsEnabled = true;
            }
        }

        private void SaveConfig(string path)
        {
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);

            string configFile;
            switch (Mode)
            {
                case 1:
                    configFile = configFileClassic;
                    break;

                case 2:
                    configFile = configFileDominion;
                    break;
                default:
                    return;
            }
            WritePrivateProfileString("ItemSet1", "SetName", "ManagedLOLSet", configFile);
            int slotId = 0;
            foreach (ItemControl itemControl in itemSlots)
            {
                slotId++;
                if (slotId >= 7) break;
                if (itemControl == null || itemControl.Item == null) continue;
                WritePrivateProfileString("ItemSet1", "RecItem" + slotId, itemControl.Item.ID.ToString(), configFile);
            }
            btnDelete.IsEnabled = true;
        }

        private void DeleteConfig(string path)
        {
            if (File.Exists(configFileClassic))
                File.Delete(configFileClassic);
            if (File.Exists(configFileDominion))
                File.Delete(configFileDominion);
            if (Directory.Exists(configFolder))
                Directory.Delete(configFolder);
            btnDelete.IsEnabled = false;
            ClearItems();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig(configFileClassic);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteConfig(configFileClassic);
        }

        private void modeButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearItems();
            if (!this.IsLoaded) return;
            ToggleButton tb = sender as ToggleButton;
            if (tb == null) return;
            switch (tb.Name)
            {
                case "btnClassic":
                    btnClassic.IsChecked = true;
                    btnDominion.IsChecked = false;
                    Mode = 1;
                    break;

                case "btnDominion":
                    btnClassic.IsChecked = false;
                    btnDominion.IsChecked = true;
                    Mode = 2;
                    break;
            }
            LoadItems();
        }
    }
}