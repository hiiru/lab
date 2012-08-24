using System.Windows.Forms;

namespace BoderlessFun.WinAPIClasses
{
    /// <summary>
    /// Extended NativeWindows used for shellhook.
    /// </summary>
    public class NativeWindowEx : NativeWindow
    {
        public delegate void MessageReceivedEventHandler(Message m);

        /// <summary>
        /// Event is fired each time a WndProc message is recieved.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;

        protected override void WndProc(ref Message m)
        {
            if (MessageReceived != null) MessageReceived(m);
            base.WndProc(ref m);
        }
    }
}