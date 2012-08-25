using System;
using System.Windows;

namespace ManagedLOL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow window;

        public static event EventHandler SettingsChanged;

        public bool HasCreepTimerWindow { get { return creepWindow != null; } }

        private CreepTimer.CreepTimer creepWindow;
        private Configuration.Settings settingsWindow;
        private ItemChanger.ItemChanger itemChangerWindow;

        public MainWindow()
        {
            InitializeComponent();
            window = this;
        }

        public void ShowTimer()
        {
            if (creepWindow == null || creepWindow.IsClosed)
                creepWindow = new CreepTimer.CreepTimer();
            creepWindow.Show();
        }

        private void btnTimer_Click(object sender, RoutedEventArgs e)
        {
            ShowTimer();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (settingsWindow == null || settingsWindow.IsClosed)
                settingsWindow = new Configuration.Settings();
            settingsWindow.Show();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        internal static void ReloadSettings()
        {
            if (MainWindow.SettingsChanged != null)
                MainWindow.SettingsChanged(window, null);
            if (window != null && window.HasCreepTimerWindow)
                window.ShowTimer();
        }

        private void btnItemChanger_Click(object sender, RoutedEventArgs e)
        {
            if (itemChangerWindow == null || itemChangerWindow.IsClosed)
                itemChangerWindow = new ItemChanger.ItemChanger();
        }
    }
}