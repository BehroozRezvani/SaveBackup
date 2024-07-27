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

        private static void ZipFolder()
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string zipFolder = Path.Combine(documentsPath, Strings.NBGIBackup);
                string sourceFolder = Path.Combine(documentsPath, Strings.NBGI);
                string zipFile = Path.Combine(zipFolder, $"{Strings.NBGI}_{DateTime.Now:yyyyMMddHHmmss}{Strings.dotZip}");
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
            string iniFilePath = Path.Combine(AppContext.BaseDirectory, Strings.configFile);
            IniFile iniFile = new(iniFilePath);
            uint modifier = 0;
            if (File.Exists(iniFilePath))
            {
                if (iniFile.HasHotKey(section, Strings.ALT))
                {
                    modifier |= MOD_ALT;
                }
                if (iniFile.HasHotKey(section, Strings.CTRL))
                {
                    modifier |= MOD_CONTROL;
                }
                if (iniFile.HasHotKey(section, Strings.SHIFT))
                {
                    modifier |= MOD_SHIFT;
                }
                if (modifier == 0)
                {
                    Console.WriteLine(Strings.FileFoundNoConfig);
                }
            }
            if (modifier == 0)
            {
                switch (section)
                {
                    case "Save":
                        Console.WriteLine(Strings.SaveNotloaded, Strings.SaveHotkey);
                        break;
                    case "Restore":
                        Console.WriteLine(Strings.RestoreNotloaded, Strings.RestoreHotkey);
                        break;
                    case "Quit":
                        Console.WriteLine(Strings.QuitNotloaded, Strings.QuitHotkey);
                        break;
                    default:
                        Console.WriteLine(Strings.SomethingWrongDefaultHotKey);
                        Console.WriteLine(Strings.SaveHotkey);
                        Console.WriteLine(Strings.RestoreHotkey);
                        Console.WriteLine(Strings.QuitHotkey);
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
                string zipFolder = Path.Combine(documentsPath, Strings.NBGIBackup);
                string sourceFolder = Path.Combine(documentsPath, Strings.NBGI);

                string zipFile = "";
                foreach (string file in Directory.EnumerateFiles(zipFolder))
                {
                    if (!file.EndsWith(Strings.dotZip))
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
                    Console.WriteLine(Strings.FileNotFound);
                    //NotifyIcon notifyIcon = new NotifyIcon();
                    //notifyIcon.Icon = System.Drawing.SystemIcons.Warning;
                    //notifyIcon.Visible = true;
                    //notifyIcon.ShowBalloonTip(5000, "Completed", "This is a toast notification", ToolTipIcon.None);
                    return;
                }
                Console.WriteLine($"{Strings.RestoreFrom} {zipFile}");
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

        public static void Main()
        {
            uint saveAtom = GetAtom(Strings.ZipperHKSave);
            uint restoreAtom = GetAtom(Strings.ZipperHKRestore);
            uint quitAtom = GetAtom(Strings.ZipperHKQuit);

            RegisterHotKey(0, saveAtom, GetHotKeys(Strings.Save), Keys.S);
            RegisterHotKey(0, restoreAtom, GetHotKeys(Strings.Restore), Keys.Z);
            RegisterHotKey(0, quitAtom, GetHotKeys(Strings.Quit), Keys.Q);

            //MSG msg;
            while (GetMessageA(out MSG msg, nint.Zero, 0, 0) != 0)
            {
                if (msg.message != WM_HOTKEY) continue;
                Console.WriteLine(Strings.HKPressed);
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