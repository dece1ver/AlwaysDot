using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using App.Extensions.pInvoke;

namespace App.Extensions.Keyboard
{
    internal class KeyboardListener
    {
        private const int WH_KEYBOARD_LL = 13;
        

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private readonly LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;

        
        public KeyboardListener()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookId = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            WinApi.UnhookWindowsHookEx(_hookId);
        }


        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return WinApi.SetWindowsHookEx(WH_KEYBOARD_LL, proc, WinApi.GetModuleHandle(curModule?.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if ((nCode < 0 || wParam != (IntPtr)WM.KEYDOWN) && wParam != (IntPtr)WM.SYSKEYDOWN)
                return WinApi.CallNextHookEx(_hookId, nCode, wParam, lParam);
            var vkCode = Marshal.ReadInt32(lParam);

            if (Program.PassProcesses.Contains(WinApi.GetForegroundProcessName())) return (IntPtr)0;

            if (KeyboardLayout.Current != KeyboardLayout.Ru)
            {
                return (IntPtr)0;
            }

            if (vkCode != (int)Keys.Decimal) return WinApi.CallNextHookEx(_hookId, nCode, wParam, lParam);
            UnHookKeyboard();
            Keyboard.KeyDown(Keys.OemQuestion);
            HookKeyboard();
            return (IntPtr)1;
        }
    }
}
