using System;
using System.Threading;
using System.Windows.Forms;

namespace App
{
    internal class SingleInstance
    {
        public static readonly int WM_SHOWFIRSTINSTANCE =
            WinApi.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", ProgramInfo.AssemblyGuid);

        private static Mutex _mutex;
        public static bool Start()
        {
            var mutexName = $"Local\\{ProgramInfo.AssemblyGuid}";

            _mutex = new Mutex(true, mutexName, out var onlyInstance);
            return onlyInstance;
        }
        public static void ShowFirstInstance()
        {
            MessageBox.Show("Always Dot уже запущен.", "Always Dot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            WinApi.PostMessage(
                (IntPtr)WinApi.HWND_BROADCAST,
                WM_SHOWFIRSTINSTANCE,
                IntPtr.Zero,
                IntPtr.Zero);
        }
        public static void Stop()
        {
            _mutex.ReleaseMutex();
        }
    }
}
