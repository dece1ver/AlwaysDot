using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using static App.Extensions.Keyboard.KeyboardListener;

namespace App
{
    internal class WinApi
    {
        public const string User32 = "user32.dll";
        public const string Kernel32 = "kernel32.dll";

        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;


        [DllImport(User32)]
        public static extern int RegisterWindowMessage(string message);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport(User32)]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport(User32,
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            EntryPoint = "LoadKeyboardLayout",
            SetLastError = true,
            ThrowOnUnmappableChar = false)]
        public static extern uint LoadKeyboardLayout(
            StringBuilder pwszKLID,
            uint flags);

        [DllImport(User32, SetLastError = true)]
        public static extern ushort GetKeyboardLayout([In] int idThread);

        [DllImport(User32,
            CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            EntryPoint = "ActivateKeyboardLayout",
            SetLastError = true,
            ThrowOnUnmappableChar = false)]
        public static extern uint ActivateKeyboardLayout(
            uint hkl,
            uint flags);

        [DllImport(User32, SetLastError = true)]
        public static extern int GetWindowThreadProcessId([In] IntPtr hWnd, [Out, Optional] IntPtr lpdwProcessId);
            
        [DllImport(User32, SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
        
        [DllImport(User32)]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport(User32)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport(User32)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static int RegisterWindowMessage(string format, params object[] args)
        {
            var message = string.Format(format, args);
            return RegisterWindowMessage(message);
        }

        public static string GetForegroundProcessName()
        {
            var pid = IntPtr.Zero;
            var _ = GetWindowThreadProcessId(GetForegroundWindow(), pid);

            foreach (var p in Process.GetProcesses())
            {
                if ((IntPtr)p.Id == pid) return p.ProcessName;
            }
            return string.Empty;
        }

        public static void ShowToFront(IntPtr window)
        {
            ShowWindow(window, SW_SHOWNORMAL);
            SetForegroundWindow(window);
        }
    }
}
