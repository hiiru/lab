using System.Windows;
using BoderlessFun.WinAPIClasses;

namespace BoderlessFun
{
    public partial class App : Application
    {
        public static bool DoResize { get; set; }

        public static System.Windows.Forms.Screen MoveToScreen { get; set; }

        private static ShellHook _hook;

        protected override void OnStartup(StartupEventArgs e)
        {
            DoResize = true;
            MoveToScreen = System.Windows.Forms.Screen.PrimaryScreen;
            if (_hook == null)
                _hook = new ShellHook();
            base.OnStartup(e);
        }
    }
}