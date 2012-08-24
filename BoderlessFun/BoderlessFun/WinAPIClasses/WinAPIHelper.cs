using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace BoderlessFun.WinAPIClasses
{
    public class WinAPIHelper
    {
        // helper variables for setting borderless
        private static int HIDEBAR_WS = (int)(WinAPI.WindowStyles.WS_CAPTION | WinAPI.WindowStyles.WS_THICKFRAME | WinAPI.WindowStyles.WS_MAXIMIZE | WinAPI.WindowStyles.WS_SYSMENU);

        private static int HIDEBAR_WS_EX = (int)(WinAPI.WindowStyles.WS_EX_DLGMODALFRAME | WinAPI.WindowStyles.WS_EX_CLIENTEDGE | WinAPI.WindowStyles.WS_EX_STATICEDGE);
        private static int HIDEBAR_SWP = (int)(WinAPI.SWP.FRAMECHANGED | WinAPI.SWP.NOZORDER | WinAPI.SWP.NOOWNERZORDER);
        private static int HIDEBAR_SWP_NORESIZE = (int)(WinAPI.SWP.FRAMECHANGED | WinAPI.SWP.NOMOVE | WinAPI.SWP.NOSIZE | WinAPI.SWP.NOZORDER | WinAPI.SWP.NOOWNERZORDER);

        public static bool IsBorderless(Process p)
        {
            if (p == null || p.MainWindowHandle == IntPtr.Zero) return false;
            int style = WinAPI.GetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_STYLE);
            bool borderlessStyle = (style & (int)(WinAPI.WindowStyles.WS_CAPTION | WinAPI.WindowStyles.WS_THICKFRAME)) == 0;
            return borderlessStyle;
        }

        public static bool SetBorderless(Process p, bool resize = true, Screen screen = null)
        {
            if (p == null || p.MainWindowHandle == IntPtr.Zero) return false;
            if (IsBorderless(p)) return true;
            int style = WinAPI.GetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_STYLE);
            WinAPI.SetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_STYLE, (style & ~HIDEBAR_WS));

            int extStyle = WinAPI.GetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_EXSTYLE);
            WinAPI.SetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_EXSTYLE, (extStyle & ~HIDEBAR_WS_EX));

            if (resize)
            {
                if (screen == null) screen = Screen.PrimaryScreen;
                System.Drawing.Rectangle r = screen.WorkingArea;
                WinAPI.SetWindowPos(p.MainWindowHandle, 0, r.X, r.Y, r.Width, r.Height, HIDEBAR_SWP);
            }
            else
            {
                WinAPI.SetWindowPos(p.MainWindowHandle, 0, 0, 0, 0, 0, HIDEBAR_SWP_NORESIZE);
            }
            return true;
        }

        public static bool UnsetBorderless(Process p, bool resize = true, Screen screen = null)
        {
            if (p == null || p.MainWindowHandle == IntPtr.Zero) return false;
            if (!IsBorderless(p)) return true;

            int style = WinAPI.GetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_STYLE);
            WinAPI.SetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_STYLE, (style | HIDEBAR_WS));

            int extStyle = WinAPI.GetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_EXSTYLE);
            WinAPI.SetWindowLong(p.MainWindowHandle, (int)WinAPI.GWL.GWL_EXSTYLE, (extStyle | HIDEBAR_WS_EX));
            if (resize)
            {
                if (screen == null) screen = Screen.PrimaryScreen;
                System.Drawing.Rectangle r = screen.WorkingArea;
                WinAPI.SetWindowPos(p.MainWindowHandle, 0, r.X, r.Y, r.Width, r.Height, HIDEBAR_SWP);
            }
            else
                WinAPI.SetWindowPos(p.MainWindowHandle, 0, 0, 0, 0, 0, HIDEBAR_SWP_NORESIZE);

            return true;
        }
    }
}