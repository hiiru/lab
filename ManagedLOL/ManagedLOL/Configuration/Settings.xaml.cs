using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ManagedLOL.Configuration
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public bool IsClosed { get; private set; }

        public Settings()
        {
            InitializeComponent();
            LoadSettings();
            ddlScreen.ItemsSource = System.Windows.Forms.Screen.AllScreens.Select(x => x.DeviceName);

            this.Closed += delegate(object sender, EventArgs args) { IsClosed = true; };
        }

        public void LoadSettings()
        {
            // Creep Timer
            switch (Properties.Settings.Default.CT_DisplayMode)
            {
                case 1:
                    rbtWindow.IsChecked = true;
                    break;
                default:
                    rbtOverlay.IsChecked = true;
                    break;
            }

            if (Properties.Settings.Default.CT_Screen <= System.Windows.Forms.Screen.AllScreens.Count())
                ddlScreen.SelectedValue = System.Windows.Forms.Screen.AllScreens[Properties.Settings.Default.CT_Screen].DeviceName;
            else
                ddlScreen.SelectedValue = System.Windows.Forms.Screen.AllScreens.First(x => x.Primary).DeviceName;

            // Item Changer
            txtIC_LOLPath.Text = Properties.Settings.Default.IC_LOLPath;
        }

        public void SaveSettings()
        {
            // Creep Timer
            if (rbtWindow.IsChecked.Value)
                Properties.Settings.Default.CT_DisplayMode = 1;
            else
                Properties.Settings.Default.CT_DisplayMode = 0;

            Properties.Settings.Default.CT_Screen = ddlScreen.SelectedIndex;

            // Item Changer
            if (Directory.Exists(txtIC_LOLPath.Text))
                Properties.Settings.Default.IC_LOLPath = txtIC_LOLPath.Text;

            Properties.Settings.Default.Save();
            MainWindow.ReloadSettings();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            this.Close();
        }

        private void CT_DisplayMode_Changed(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender).Name == "rbtWindow")
            {
                lblScreen.IsEnabled = ddlScreen.IsEnabled = false;
            }
            else
            {
                lblScreen.IsEnabled = ddlScreen.IsEnabled = true;
            }
        }

        private void btnSelectLolPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();

            fbd.ShowNewFolderButton = false;
            fbd.Description = "Select League of Legends Folder";

            if (!string.IsNullOrWhiteSpace(txtIC_LOLPath.Text))
                fbd.SelectedPath = txtIC_LOLPath.Text;
            if (System.Windows.Forms.DialogResult.OK == fbd.ShowDialog())
            {
                txtIC_LOLPath.Text = fbd.SelectedPath;
            }
        }

        private void txtIC_LOLPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SettingsHelper.IsLolPath(txtIC_LOLPath.Text))
            {
                txtIC_LOLPath.Foreground = Brushes.DarkGreen;
            }
            else
            {
                txtIC_LOLPath.Foreground = Brushes.DarkRed;
            }
        }
    }
}