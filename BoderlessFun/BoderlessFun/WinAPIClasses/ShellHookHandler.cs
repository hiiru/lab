using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace BoderlessFun.WinAPIClasses
{
    /// <summary>
    /// Shellhook which listens for created windows (auto-fullscreen) and handles global hotkeys.
    /// </summary>
    public class ShellHook
    {
        private NativeWindowEx _wnd;
        private IntPtr _wndHandle = IntPtr.Zero;

        public static bool IsRegistered { get; private set; }

        private bool _initShellHook = false;

        private bool _initWindowMessage = false;
        private bool _initHotkey1 = false;
        private bool _initHotkey2 = false;

        private WinAPI.ModifierKeys _HotKeyModifier = WinAPI.ModifierKeys.Control | WinAPI.ModifierKeys.Win;
        private WinAPI.VirtualKeys _HotKey1 = WinAPI.VirtualKeys.X;
        private WinAPI.VirtualKeys _HotKey2 = WinAPI.VirtualKeys.Y;
        private int _HotKey1Id = 1;
        private int _HotKey2Id = 2;

        public ShellHook()
        {
            if (IsRegistered)
                throw new InvalidOperationException("ShellHook already initialized.");
            try
            {
                _wnd = new NativeWindowEx();
                _wnd.CreateHandle(new CreateParams());
                _wndHandle = _wnd.Handle;

                // Register to receive shell-related events
                if (_wndHandle != IntPtr.Zero)
                    _initShellHook = WinAPI.RegisterShellHookWindow(_wnd.Handle);
                if (_initShellHook)
                    WinAPI.WM_SHELLHOOKMESSAGE = WinAPI.RegisterWindowMessage("SHELLHOOK");
                if (WinAPI.WM_SHELLHOOKMESSAGE != -1)
                    _initWindowMessage = true;
                if (!_initWindowMessage)
                {
                    Cleanup();
                    return;
                }
                _wnd.MessageReceived += ShellHookHandler;
                _initHotkey1 = WinAPI.RegisterHotKey(_wndHandle, _HotKey1Id, (uint)_HotKeyModifier, (uint)_HotKey1);
                _initHotkey2 = WinAPI.RegisterHotKey(_wndHandle, _HotKey2Id, (uint)_HotKeyModifier, (uint)_HotKey2);
                IsRegistered = true;
            }
            catch
            {
                Cleanup();
            }
        }

        ~ShellHook()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_wndHandle != IntPtr.Zero)
            {
                WinAPI.WM_SHELLHOOKMESSAGE = -1;
                if (_initShellHook)
                    WinAPI.DeregisterShellHookWindow(_wndHandle);
                if (_initHotkey1)
                    WinAPI.UnregisterHotKey(_wndHandle, _HotKey1Id);
                if (_initHotkey2)
                    WinAPI.UnregisterHotKey(_wndHandle, _HotKey2Id);
            }
            if (_wnd != null)
            {
                _wnd.DestroyHandle();
                _wnd = null;
            }
            IsRegistered = false;
        }

        private static object _lock = new object();

        private static readonly Dictionary<IntPtr, Process> _autoBorderlessHandles = new Dictionary<IntPtr, Process>();

        private static readonly HashSet<string> _autoBorderlessFiles = new HashSet<string>();

        public void AddAutoBorderless(IntPtr ptr, Process p)
        {
            lock (_lock)
            {
                if (_autoBorderlessHandles.ContainsKey(ptr))
                    _autoBorderlessHandles[ptr] = p;
                else
                    _autoBorderlessHandles.Add(ptr, p);
            }
        }

        public void RemoveAutoBorderless(IntPtr ptr)
        {
            lock (_lock)
            {
                if (_autoBorderlessHandles.ContainsKey(ptr))
                    _autoBorderlessHandles.Remove(ptr);
            }
        }

        public static bool IsAutoBorderlessHandle(IntPtr ptr)
        {
            return _autoBorderlessHandles.ContainsKey(ptr);
        }

        public static bool IsAutoBorderlessFilename(string str)
        {
            return _autoBorderlessFiles.Contains(str);
        }

        public static void AddAutoBorderless(string filename)
        {
            _autoBorderlessFiles.Add(filename);
        }

        public static void RemoveAutoBorderless(string filename)
        {
            _autoBorderlessFiles.Remove(filename);
        }

        private void CheckProcessForAutoBorderless(IntPtr ptr, Process p)
        {
            if (ptr == IntPtr.Zero || p == null) return;
            try
            {
                if (_autoBorderlessFiles.Contains(p.MainModule.ModuleName))
                    AddAutoBorderless(ptr, p);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Handles the Windows messages for the taskbar
        /// </summary>
        private void ShellHookHandler(System.Windows.Forms.Message msg)
        {
            int pid = 0;
            Process p;
            if (msg.Msg == WinAPI.WM_SHELLHOOKMESSAGE)
            {
                try
                {
                    switch (msg.WParam.ToInt32())
                    {
                        case (int)WinAPI.HSHELL.HSHELL_WINDOWCREATED:
                            try
                            {
                                WinAPI.GetWindowThreadProcessId(msg.LParam, out pid);
                                p = Process.GetProcessById(pid);
                                CheckProcessForAutoBorderless(msg.LParam, p);
                                Debug.WriteLine("HSHELL_WINDOWCREATED: " + p.Id + ": " + p.ProcessName + " borderless? " + WinAPIHelper.IsBorderless(p));
                            }
                            catch
                            {
                            }
                            break;

                        case (int)WinAPI.HSHELL.HSHELL_WINDOWDESTROYED:
                            if (IsAutoBorderlessHandle(msg.LParam))
                                RemoveAutoBorderless(msg.LParam);
                            break;

                        //Debug.WriteLine("HSHELL_WINDOWDESTROYED: " + msg.LParam + " " + msg.Result + " " + msg.HWnd);
                        //break;

                        case (int)WinAPI.HSHELL.HSHELL_WINDOWREPLACING:
                        case (int)WinAPI.HSHELL.HSHELL_WINDOWREPLACED:

                        //Debug.WriteLine("HSHELL_WINDOWREPLACING: " + msg.LParam + " " + msg.Result + " " + msg.HWnd);
                        //break;

                        case (int)WinAPI.HSHELL.HSHELL_WINDOWACTIVATED:
                        case (int)WinAPI.HSHELL.HSHELL_RUDEAPPACTIVATED:
                            if (IsAutoBorderlessHandle(msg.LParam))
                            {
                                try
                                {
                                    WinAPI.GetWindowThreadProcessId(msg.LParam, out pid);
                                    p = Process.GetProcessById(pid);
                                    WinAPIHelper.SetBorderless(p, App.DoResize, App.MoveToScreen);
                                }
                                catch
                                {
                                }
                            }
                            break;

                        case (int)WinAPI.HSHELL.HSHELL_FLASH:
                        case (int)WinAPI.HSHELL.HSHELL_ACTIVATESHELLWINDOW:
                        case (int)WinAPI.HSHELL.HSHELL_ENDTASK:
                        case (int)WinAPI.HSHELL.HSHELL_GETMINRECT:
                        case (int)WinAPI.HSHELL.HSHELL_REDRAW:

                        default:

                            //if (_autoBorderless.ContainsKey(msg.LParam))
                            //{
                            //   WinAPI.GetWindowThreadProcessId(msg.LParam, out pid);
                            //   //p = Process.GetProcessById(pid);
                            //   //if (_autoBorderless.ContainsKey(msg.LParam))
                            //   //   WinAPIHelper.SetBorderless(p);
                            //   Debug.WriteLine(msg.WParam + ": " + msg.LParam + " - PID: " + pid);
                            //   //+ " borderless? " + WinAPIHelper.IsBorderless(p) + " -> " + _autoBorderless.ContainsKey(msg.LParam));
                            //}
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("msg:" + msg.Msg.ToString());
                    Debug.WriteLine("Exception: " + ex.ToString());
                }
            }
            else if (msg.Msg == (int)WinAPI.WM.HOTKEY)
            {
                try
                {
                    IntPtr hnd = WinAPI.GetForegroundWindow();
                    if (hnd == IntPtr.Zero) return;
                    WinAPI.GetWindowThreadProcessId(hnd, out pid);
                    p = Process.GetProcessById(pid);
                    if (p.Id == 0) return;
                    WinAPI.VirtualKeys key = (WinAPI.VirtualKeys)(((int)msg.LParam >> 16) & 0xFFFF);

                    //WinAPI.ModifierKeys modifier = (WinAPI.ModifierKeys)((int)msg.LParam & 0xFFFF);

                    switch (key)
                    {
                        case WinAPI.VirtualKeys.X:
                            WinAPIHelper.SetBorderless(p, App.DoResize, App.MoveToScreen);
                            break;

                        case WinAPI.VirtualKeys.Y:
                            WinAPIHelper.UnsetBorderless(p, App.DoResize, App.MoveToScreen);
                            break;
                    }
                }
                catch
                {
                }
            }
        }
    }
}