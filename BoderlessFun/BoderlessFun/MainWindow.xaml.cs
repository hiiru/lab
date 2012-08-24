using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace BoderlessFun
{
    public partial class MainWindow : Window
    {
        private Dictionary<int, string> _processes;

        public MainWindow()
        {
            InitializeComponent();
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            ddlProcesses.Items.Clear();
            _processes = new Dictionary<int, string>();
            foreach (Process p in Process.GetProcesses().OrderBy(x => x.ProcessName))
            {
                if (p.Id == 0 || p.Id == 4) continue;
                ddlProcesses.Items.Add(new ComboBoxItem() { Content = p.Id + " - " + p.ProcessName, Tag = p });
            }
            ddlScreen.ItemsSource = System.Windows.Forms.Screen.AllScreens.Select(x => x.DeviceName);
        }

        private void btnReloadProcesses_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void btnSetBorderless_Click(object sender, RoutedEventArgs e)
        {
            Process p = GetProcessFromDDL();
            if (p == null) return;
            WinAPIClasses.WinAPIHelper.SetBorderless(p, App.DoResize, App.MoveToScreen);
            UpdateButtons(p);
        }

        private void btnUnsetBorderless_Click(object sender, RoutedEventArgs e)
        {
            Process p = GetProcessFromDDL();
            if (p == null) return;
            WinAPIClasses.WinAPIHelper.UnsetBorderless(p, App.DoResize, App.MoveToScreen);
            UpdateButtons(p);
        }

        private void ddlProcesses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons(GetProcessFromDDL());
        }

        private void btnAddAutoBorderless_Click(object sender, RoutedEventArgs e)
        {
            Process p = GetProcessFromDDL();
            if (p == null) return;
            WinAPIClasses.ShellHook.AddAutoBorderless(GetProcessFilename(p));
            UpdateButtons(p);
        }

        private void btnRemoveAutoBorderless_Click(object sender, RoutedEventArgs e)
        {
            Process p = GetProcessFromDDL();
            if (p == null) return;
            WinAPIClasses.ShellHook.RemoveAutoBorderless(GetProcessFilename(p));
            UpdateButtons(p);
        }

        private Process GetProcessFromDDL()
        {
            ComboBoxItem item = ddlProcesses.SelectedItem as ComboBoxItem;
            if (item == null || string.IsNullOrWhiteSpace(item.Content.ToString())) return null;
            return item.Tag as Process;
        }

        private void UpdateButtons(Process p)
        {
            bool borderless = false, isAuto = false;
            bool disableAll = p == null;
            if (!disableAll)
            {
                borderless = WinAPIClasses.WinAPIHelper.IsBorderless(p);
                isAuto = WinAPIClasses.ShellHook.IsAutoBorderlessFilename(GetProcessFilename(p));
            }
            UpdateButtons(disableAll, borderless, isAuto);
        }

        private void UpdateButtons(bool disableAll, bool borderless, bool isAuto)
        {
            btnSetBorderless.IsEnabled = !disableAll && !borderless;
            btnUnsetBorderless.IsEnabled = !disableAll && borderless;
            btnAddAutoBorderless.IsEnabled = !disableAll && WinAPIClasses.ShellHook.IsRegistered && !isAuto;
            btnRemoveAutoBorderless.IsEnabled = !disableAll && WinAPIClasses.ShellHook.IsRegistered && isAuto;
        }

        private string GetProcessFilename(Process p)
        {
            if (p == null) return null;
            try
            {
                return p.MainModule.ModuleName;
            }
            catch
            {
                return null;
            }
        }

        private void rbResize_Checked(object sender, RoutedEventArgs e)
        {
            if (!this.IsInitialized) return;
            if (rbDontResize.IsChecked.Value)
                App.DoResize = false;
            else if (rbResize.IsChecked.Value)
                App.DoResize = true;
        }

        private void ddlScreen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlScreen.SelectedIndex < Screen.AllScreens.Count())
            {
                App.MoveToScreen = Screen.AllScreens[ddlScreen.SelectedIndex];
            }
        }
    }
}