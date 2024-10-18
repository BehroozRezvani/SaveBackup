using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SaveBackup.src
{
    public static class ConfigManager
    {
        private readonly static string _filePath = Path.Combine(AppContext.BaseDirectory, "config.ini");

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;

        public static uint GetHotKeys(string section)
        {
            uint modifier = 0;
            if (File.Exists(_filePath))
            {
                if (HasHotKey(section, "ALT"))
                {
                    modifier |= MOD_ALT;
                }
                if (HasHotKey(section, "CTRL"))
                {
                    modifier |= MOD_CONTROL;
                }
                if (HasHotKey(section, "SHIFT"))
                {
                    modifier |= MOD_SHIFT;
                }
                if (modifier == 0)
                {
                    Console.WriteLine("Config file was found but settings could not be loaded.");
                }
            }
            if (modifier == 0)
            {
                switch (section)
                {
                    case "Save":
                        Console.WriteLine("Save Hotkeys could not be loaded, Default: Save: CTRL + SHIFT + S");
                        break;
                    case "Restore":
                        Console.WriteLine("Restore Hotkeys could not be loaded, Default: Restore: CTRL + SHIFT + Z");
                        break;
                    case "Quit":
                        Console.WriteLine("Quit Hotkeys could not be loaded, Default: Quit: CTRL + SHIFT + Q");
                        break;
                    default:
                        Console.WriteLine("Something went wrong, default Hotkeys will be used.");
                        Console.WriteLine("Save: CTRL + SHIFT + S");
                        Console.WriteLine("Restore: CTRL + SHIFT + Z");
                        Console.WriteLine("Quit: CTRL + SHIFT + Q");
                        break;
                }
                modifier = MOD_CONTROL | MOD_SHIFT;
            }
            return modifier;
        }

        public static Keys GetModKey(string section)
        {
            string modifier = GetModifierKey(section);
            if (File.Exists(_filePath) && modifier != "")
            {
                return KeyMap.GetKey(modifier);
            }
            switch (section)
            {
                case "Save":
                    Console.WriteLine("Save Hotkeys could not be loaded, Default: Save: CTRL + SHIFT + S");
                    return Keys.S;
                case "Restore":
                    Console.WriteLine("Restore Hotkeys could not be loaded, Default: Restore: CTRL + SHIFT + Z");
                    return Keys.Z;
                case "Quit":
                    Console.WriteLine("Quit Hotkeys could not be loaded, Default: Quit: CTRL + SHIFT + Q");
                    return Keys.Q;
                default:
                    Console.WriteLine("Something went wrong, default Hotkeys will be used.");
                    Console.WriteLine("Save: CTRL + SHIFT + S");
                    Console.WriteLine("Restore: CTRL + SHIFT + Z");
                    Console.WriteLine("Quit: CTRL + SHIFT + Q");
                    break;
            }
            return Keys.None;
        }

        private static bool HasHotKey(string section, string hotKey)
        {
            return Read(section, hotKey).Equals("true", StringComparison.OrdinalIgnoreCase);
        }


        private static string GetModifierKey(string section)
        {
            return Read(section, "MODIFIER");
        }

        public static string Read(string section, string key)
        {
            StringBuilder SB = new(255);
            int result = GetPrivateProfileString(section, key, "", SB, 255, _filePath);
            if (result == 0)
            {
                throw new Exception("Error reading INI file.");
            }
            else if (result == 255)
            {
                throw new Exception("Buffer size is too small to hold the entire string value.");
            }
            return SB.ToString();
        }

    }
}