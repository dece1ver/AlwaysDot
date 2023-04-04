using System;
using System.Globalization;
using System.Text;

namespace App.Extensions.Keyboard
{
    internal sealed class KeyboardLayout
    {
        public const int Ru = 1049;
        public const int En = 1033;
        
        public static int Current => WinApi.GetKeyboardLayout(WinApi.GetWindowThreadProcessId(WinApi.GetForegroundWindow(), IntPtr.Zero));

        private readonly uint hkl;

        private KeyboardLayout(CultureInfo cultureInfo) => hkl = WinApi.LoadKeyboardLayout(new StringBuilder(cultureInfo.LCID.ToString("x8")), Keyboard.KlfActivate);

        private KeyboardLayout(uint hkl) => this.hkl = hkl;

        public static KeyboardLayout GetCurrent() => new KeyboardLayout(WinApi.GetKeyboardLayout(Environment.CurrentManagedThreadId));

        public static KeyboardLayout Load(CultureInfo culture) => new KeyboardLayout(culture);

        public void Activate() => WinApi.ActivateKeyboardLayout(hkl, Keyboard.KlfSetForProcess);
    }
}
