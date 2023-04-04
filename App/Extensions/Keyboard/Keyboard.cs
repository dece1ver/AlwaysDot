namespace App.Extensions.Keyboard
{
    public class Keyboard
    {

        private const int KeyEventExtendedKey = 1;
        private const int KeyEventKeyUp = 2;
        public const uint KlfActivate = 0x00000001;
        public const uint KlfSetForProcess = 0x00000100;

        public static void KeyDown(Keys vKey)
        {
           WinApi.keybd_event((byte)vKey, 0, KeyEventExtendedKey, 0);
        }

        public static void KeyUp(Keys vKey)
        {
            WinApi.keybd_event((byte)vKey, 0, KeyEventExtendedKey | KeyEventKeyUp, 0);
        }

        public static void KeyPress(Keys vKey)
        {
            KeyDown(vKey);
            KeyUp(vKey);
        }

    }
}
