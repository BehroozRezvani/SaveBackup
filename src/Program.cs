using System.IO.Compression;
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

        // private static uint GetHotKeys(string section)
        // {
        //     string iniFilePath = Path.Combine(AppContext.BaseDirectory, Texts.configFile);
        //     IniFile iniFile = new(iniFilePath);
        //     uint modifier = 0;
        //     if (File.Exists(iniFilePath))
        //     {
        //         if (iniFile.HasHotKey(section, Texts.ALT))
        //         {
        //             modifier |= MOD_ALT;
        //         }
        //         if (iniFile.HasHotKey(section, Texts.CTRL))
        //         {
        //             modifier |= MOD_CONTROL;
        //         }
        //         if (iniFile.HasHotKey(section, Texts.SHIFT))
        //         {
        //             modifier |= MOD_SHIFT;
        //         }
        //         if (modifier == 0)
        //         {
        //             Console.WriteLine(Texts.FileFoundNoConfig);
        //         }
        //     }
        //     if (modifier == 0)
        //     {
        //         switch (section)
        //         {
        //             case "Save":
        //                 Console.WriteLine(Texts.SaveNotloaded, Texts.SaveHotkey);
        //                 break;
        //             case "Restore":
        //                 Console.WriteLine(Texts.RestoreNotloaded, Texts.RestoreHotkey);
        //                 break;
        //             case "Quit":
        //                 Console.WriteLine(Texts.QuitNotloaded, Texts.QuitHotkey);
        //                 break;
        //             default:
        //                 Console.WriteLine(Texts.SomethingWrongDefaultHotKey);
        //                 Console.WriteLine(Texts.SaveHotkey);
        //                 Console.WriteLine(Texts.RestoreHotkey);
        //                 Console.WriteLine(Texts.QuitHotkey);
        //                 break;
        //         }
        //         modifier = MOD_CONTROL | MOD_SHIFT;
        //     }
        //     return modifier;
        // }

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

        // private static Keys GetModKey(string section)
        // {
        //     string iniFilePath = Path.Combine(AppContext.BaseDirectory, Texts.configFile);
        //     IniFile iniFile = new(iniFilePath);
        //     string modifier = GetModifierKey(section);
        //     if (File.Exists(iniFilePath) && modifier != "")
        //     {
        //         if (keyMap.TryGetValue(modifier, out Keys userKey))
        //         {
        //             return userKey;
        //         }
        //     }
        //     switch (section)
        //     {
        //         case "Save":
        //             Console.WriteLine(Texts.SaveNotloaded, Texts.SaveHotkey);
        //             return Keys.S;
        //         case "Restore":
        //             Console.WriteLine(Texts.RestoreNotloaded, Texts.RestoreHotkey);
        //             return Keys.Z;
        //         case "Quit":
        //             Console.WriteLine(Texts.QuitNotloaded, Texts.QuitHotkey);
        //             return Keys.Q;
        //         default:
        //             Console.WriteLine(Texts.SomethingWrongDefaultHotKey);
        //             Console.WriteLine(Texts.SaveHotkey);
        //             Console.WriteLine(Texts.RestoreHotkey);
        //             Console.WriteLine(Texts.QuitHotkey);
        //             break;
        //     }
        //     return Keys.None;
        // }

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