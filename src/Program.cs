﻿using System.IO.Compression;
using System.Runtime.InteropServices;

namespace SaveBackup.src
{
    public partial class Zippy
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern uint GlobalAddAtomA(string lpString);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern ushort GlobalDeleteAtom(uint nAtom);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(nint hWnd, uint keyId, uint fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(nint hWnd, uint id);
        [DllImport("user32.dll")]
        static extern int GetMessageA(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            readonly nint hwnd;
            public uint message;
            public nuint wParam;
            readonly nint lParam;
            readonly int time;
            readonly POINT pt;
            readonly int lPrivate;
        }

        const uint MOD_ALT = 0x0001;
        const uint MOD_CONTROL = 0x0002;
        const uint MOD_SHIFT = 0x0004;
        const uint WM_HOTKEY = 0x0312;

        private static readonly Dictionary<string, Keys> keyMap = [];

        private static void ZipFolder()
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string zipFolder = Path.Combine(documentsPath, Texts.NBGIBackup);
                string sourceFolder = Path.Combine(documentsPath, Texts.NBGI);
                string zipFile = Path.Combine(zipFolder, $"{Texts.NBGI}_{DateTime.Now:yyyyMMddHHmmss}{Texts.dotZip}");
                if (!Directory.Exists(zipFolder))
                {
                    Directory.CreateDirectory(zipFolder);
                }
                using ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Create);
                foreach (string file in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
                {
                    zip.CreateEntryFromFile(file, Path.GetRelativePath(sourceFolder, file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static uint GetHotKeys(string section)
        {
            string iniFilePath = Path.Combine(AppContext.BaseDirectory, Texts.configFile);
            IniFile iniFile = new(iniFilePath);
            uint modifier = 0;
            if (File.Exists(iniFilePath))
            {
                if (iniFile.HasHotKey(section, Texts.ALT))
                {
                    modifier |= MOD_ALT;
                }
                if (iniFile.HasHotKey(section, Texts.CTRL))
                {
                    modifier |= MOD_CONTROL;
                }
                if (iniFile.HasHotKey(section, Texts.SHIFT))
                {
                    modifier |= MOD_SHIFT;
                }
                if (modifier == 0)
                {
                    Console.WriteLine(Texts.FileFoundNoConfig);
                }
            }
            if (modifier == 0)
            {
                switch (section)
                {
                    case "Save":
                        Console.WriteLine(Texts.SaveNotloaded, Texts.SaveHotkey);
                        break;
                    case "Restore":
                        Console.WriteLine(Texts.RestoreNotloaded, Texts.RestoreHotkey);
                        break;
                    case "Quit":
                        Console.WriteLine(Texts.QuitNotloaded, Texts.QuitHotkey);
                        break;
                    default:
                        Console.WriteLine(Texts.SomethingWrongDefaultHotKey);
                        Console.WriteLine(Texts.SaveHotkey);
                        Console.WriteLine(Texts.RestoreHotkey);
                        Console.WriteLine(Texts.QuitHotkey);
                        break;
                }
                modifier = MOD_CONTROL | MOD_SHIFT;
            }
            return modifier;
        }

        private static void RestoreFolder()
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string zipFolder = Path.Combine(documentsPath, Texts.NBGIBackup);
                string sourceFolder = Path.Combine(documentsPath, Texts.NBGI);

                string zipFile = "";
                foreach (string file in Directory.EnumerateFiles(zipFolder))
                {
                    if (!file.EndsWith(Texts.dotZip))
                    {
                        continue;
                    }
                    if (zipFile == "")
                    {
                        zipFile = file;
                    }
                    if (string.Compare(zipFile, file) < 0)
                    {
                        zipFile = file;
                    }
                }
                if (zipFile == "")
                {
                    Console.WriteLine(Texts.FileNotFound);
                    //NotifyIcon notifyIcon = new NotifyIcon();
                    //notifyIcon.Icon = System.Drawing.SystemIcons.Warning;
                    //notifyIcon.Visible = true;
                    //notifyIcon.ShowBalloonTip(5000, "Completed", "This is a toast notification", ToolTipIcon.None);
                    return;
                }
                Console.WriteLine($"{Texts.RestoreFrom} {zipFile}");
                ZipArchive zip = ZipFile.OpenRead(zipFile);
                ZipFile.ExtractToDirectory(zipFile, sourceFolder, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static uint GetAtom(string atomName)
        {
            return GlobalAddAtomA(atomName);
        }

        // private static readonly Dictionary<string, Keys> keyMap = new Dictionary<string, Keys>
        // {
        //     { "A", Keys.A },
        //     { "B", Keys.B },
        //     { "C", Keys.C },
        //     { "D", Keys.D },
        //     { "E", Keys.E },
        //     { "F", Keys.F },
        //     { "G", Keys.G },
        //     { "H", Keys.H },
        //     { "I", Keys.I },
        //     { "J", Keys.J },
        //     { "K", Keys.K },
        //     { "L", Keys.L },
        //     { "M", Keys.M },
        //     { "N", Keys.N },
        //     { "O", Keys.O },
        //     { "P", Keys.P },
        //     { "Q", Keys.Q },
        //     { "R", Keys.R },
        //     { "S", Keys.S },
        //     { "T", Keys.T },
        //     { "U", Keys.U },
        //     { "V", Keys.V },
        //     { "W", Keys.W },
        //     { "X", Keys.X },
        //     { "Y", Keys.Y },
        //     { "Z", Keys.Z },
        //     { "0", Keys.D0 },
        //     { "1", Keys.D1 },
        //     { "2", Keys.D2 },
        //     { "3", Keys.D3 },
        //     { "4", Keys.D4 },
        //     { "5", Keys.D5 },
        //     { "6", Keys.D6 },
        //     { "7", Keys.D7 },
        //     { "8", Keys.D8 },
        //     { "9", Keys.D9 },
        //     { "F1", Keys.F1 },
        //     { "F2", Keys.F2 },
        //     { "F3", Keys.F3 },
        //     { "F4", Keys.F4 },
        //     { "F5", Keys.F5 },
        //     { "F6", Keys.F6 },
        //     { "F7", Keys.F7 },
        //     { "F8", Keys.F8 },
        //     { "F9", Keys.F9 },
        //     { "F10", Keys.F10 },
        //     { "F11", Keys.F11 },
        //     { "F12", Keys.F12 },
        //     { "NumPad0", Keys.NumPad0 },
        //     { "NumPad1", Keys.NumPad1 },
        //     { "NumPad2", Keys.NumPad2 },
        //     { "NumPad3", Keys.NumPad3 },
        //     { "NumPad4", Keys.NumPad4 },
        //     { "NumPad5", Keys.NumPad5 },
        //     { "NumPad6", Keys.NumPad6 },
        //     { "NumPad7", Keys.NumPad7 },
        //     { "NumPad8", Keys.NumPad8 },
        //     { "NumPad9", Keys.NumPad9 }
        // };

        private static void InitKeyMap()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                keyMap[c.ToString()] = (Keys)Enum.Parse(typeof(Keys), c.ToString());
            }
            for (int i = 0; i <= 9; i++)
            {
                keyMap[i.ToString()] = (Keys)Enum.Parse(typeof(Keys), "D" + i);
            }
            for (int i = 1; i <= 12; i++)
            {
                keyMap["F" + i] = (Keys)Enum.Parse(typeof(Keys), "F" + i);
            }
            for (int i = 0; i <= 9; i++)
            {
                keyMap["NumPad" + i] = (Keys)Enum.Parse(typeof(Keys), "NumPad" + i);
            }
        }

        private static Keys GetModKey(string section)
        {
            string iniFilePath = Path.Combine(AppContext.BaseDirectory, Texts.configFile);
            IniFile iniFile = new(iniFilePath);
            string modifier = iniFile.GetModifierKey(section);
            if (File.Exists(iniFilePath) && modifier != "")
            {
                if (keyMap.TryGetValue(modifier, out Keys userKey))
                {
                    return userKey;
                }
            }
            switch (section)
            {
                case "Save":
                    Console.WriteLine(Texts.SaveNotloaded, Texts.SaveHotkey);
                    return Keys.S;
                case "Restore":
                    Console.WriteLine(Texts.RestoreNotloaded, Texts.RestoreHotkey);
                    return Keys.Z;
                case "Quit":
                    Console.WriteLine(Texts.QuitNotloaded, Texts.QuitHotkey);
                    return Keys.Q;
                default:
                    Console.WriteLine(Texts.SomethingWrongDefaultHotKey);
                    Console.WriteLine(Texts.SaveHotkey);
                    Console.WriteLine(Texts.RestoreHotkey);
                    Console.WriteLine(Texts.QuitHotkey);
                    break;
            }
            return Keys.None;
        }

        public static void Main()
        {
            InitKeyMap();
            uint saveAtom = GetAtom(Texts.ZipperHKSave);
            uint restoreAtom = GetAtom(Texts.ZipperHKRestore);
            uint quitAtom = GetAtom(Texts.ZipperHKQuit);

            RegisterHotKey(0, saveAtom, GetHotKeys(Texts.Save), GetModKey(Texts.Save));
            RegisterHotKey(0, restoreAtom, GetHotKeys(Texts.Restore), GetModKey(Texts.Restore));
            RegisterHotKey(0, quitAtom, GetHotKeys(Texts.Quit), GetModKey(Texts.Quit));

            //MSG msg;
            while (GetMessageA(out MSG msg, nint.Zero, 0, 0) != 0)
            {
                if (msg.message != WM_HOTKEY) continue;
                Console.WriteLine(Texts.HKPressed);
                if (msg.wParam == saveAtom)
                {
                    ZipFolder();
                }
                else if (msg.wParam == restoreAtom)
                {
                    RestoreFolder();
                }
                else if (msg.wParam == quitAtom)
                {
                    break;
                }
            }

            UnregisterHotKey(0, saveAtom);
            UnregisterHotKey(0, restoreAtom);
            UnregisterHotKey(0, quitAtom);

            GlobalDeleteAtom(saveAtom);
            GlobalDeleteAtom(restoreAtom);
            GlobalDeleteAtom(quitAtom);
        }
    }
}