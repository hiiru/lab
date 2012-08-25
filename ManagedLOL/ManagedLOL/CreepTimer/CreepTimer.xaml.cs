using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ManagedLOL.CreepTimer.Creeps;

namespace ManagedLOL.CreepTimer
{
    /// <summary>
    /// Interaction logic for CreepTimer.xaml
    /// </summary>
    public partial class CreepTimer : Window
    {
        private MinorCreepTimers mctBlue;
        private MinorCreepTimers mctPurple;
        private CreepControl<Baron> baron;
        private CreepControl<Dragon> dragon;

        public bool IsClosed { get; private set; }

        private int _displayMode;

        public CreepTimer()
        {
            InitializeComponent();
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            mctBlue = new MinorCreepTimers();
            mctBlue.Background = Brushes.DarkBlue;
            mctPurple = new MinorCreepTimers(true);
            mctPurple.Background = Brushes.Purple;
            baron = new CreepControl<Baron>();
            dragon = new CreepControl<Dragon>();
            panel.Children.Add(mctBlue);
            panel.Children.Add(baron);
            panel.Children.Add(dragon);
            panel.Children.Add(mctPurple);
            _displayMode = Properties.Settings.Default.CT_DisplayMode;
            UpdateDisplay();
            MainWindow.SettingsChanged += SettingsChanged_EventHandler;
            this.Closed += delegate(object sender, EventArgs args) { IsClosed = true; };
        }

        private void UpdateDisplay()
        {
            switch (_displayMode)
            {
                case 1:
                    SetWindow();
                    break;
                default:
                    SetOverlay();
                    break;
            }
            this.Show();
        }

        private void SetOverlay()
        {
            if (!this.IsLoaded)
            {
                this.WindowStyle = System.Windows.WindowStyle.None;
                this.Opacity = 0.7;
                this.ShowInTaskbar = false;
                this.Topmost = true;
                this.AllowsTransparency = true;
            }
            btnMinimize.Visibility = System.Windows.Visibility.Visible;
            btnMinimize.Height = 10;
            this.Width = mctBlue.Width + baron.Width + dragon.Width + mctPurple.Width;
            this.Height = 65;
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[Properties.Settings.Default.CT_Screen];
            this.Left = ((screen.WorkingArea.Width - this.Width) / 2) + screen.WorkingArea.X;
            this.Top = screen.WorkingArea.Y;
        }

        private void SetWindow()
        {
            if (!this.IsLoaded)
            {
                this.AllowsTransparency = false;
                this.WindowStyle = System.Windows.WindowStyle.ToolWindow;
                this.Opacity = 1;
                this.ShowInTaskbar = true;
                this.Topmost = false;
            }
            btnMinimize.Visibility = System.Windows.Visibility.Hidden;
            btnMinimize.Height = 0;
            this.Width = mctBlue.Width + baron.Width + dragon.Width + mctPurple.Width + 7;
            this.Height = 80;
        }

        private void SettingsChanged_EventHandler(object sender, EventArgs args)
        {
            if (_displayMode == Properties.Settings.Default.CT_DisplayMode)
                UpdateDisplay();
            else
                this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (panel.Visibility == System.Windows.Visibility.Hidden)
            {
                panel.Visibility = System.Windows.Visibility.Visible;
                this.Height = 65;
            }
            else
            {
                panel.Visibility = System.Windows.Visibility.Hidden;
                this.Height = 0;
            }
        }

        private void WndSourceInitialized(object sender, EventArgs e)
        {
            HwndSource hnd = HwndSource.FromHwnd((new WindowInteropHelper(this)).Handle);
            hnd.AddHook(new HwndSourceHook(WndProc));
        }

        private void WndClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HwndSource hnd = HwndSource.FromHwnd((new WindowInteropHelper(this)).Handle);
            hnd.RemoveHook(new HwndSourceHook(WndProc));
        }

        private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case (int)Libraries.WinAPI.WM.LBUTTONUP:
                case (int)Libraries.WinAPI.WM.MBUTTONUP:
                case (int)Libraries.WinAPI.WM.RBUTTONUP:
                    var x = Process.GetProcessesByName("League of Legends");
                    if (x == null || x.Length == 0) break;
                    Libraries.WinAPI.SetForegroundWindow(x[0].MainWindowHandle);
                    break;
            }
            return IntPtr.Zero;
        }
    }
}