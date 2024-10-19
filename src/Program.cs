using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SaveBackup.src
{
    [SupportedOSPlatform("Windows6.1")]
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

        const uint WM_HOTKEY = 0x0312;
        
        private static void SetupINIFile()
        {
            if (!File.Exists("config.ini"))
            {
                ConfigManager.Write("Save", "ALT", "false");
                ConfigManager.Write("Save", "CTRL", "true");
                ConfigManager.Write("Save", "SHIFT", "true");
                ConfigManager.Write("Save", "MODIFIER", "S");

                ConfigManager.Write("Restore", "ALT", "false");
                ConfigManager.Write("Restore", "CTRL", "true");
                ConfigManager.Write("Restore", "SHIFT", "true");
                ConfigManager.Write("Restore", "MODIFIER", "Z");

                ConfigManager.Write("Quit", "ALT", "false");
                ConfigManager.Write("Quit", "CTRL", "true");
                ConfigManager.Write("Quit", "SHIFT", "true");
                ConfigManager.Write("Quit", "MODIFIER", "Q");

                ConfigManager.Write("Game", "Name", "Dark_Souls_Remastered");
                ConfigManager.Write("Game", "SavePath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI"));
                ConfigManager.Write("Game", "ZipPath", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI_Backup"));

                ConfigManager.Write("ToastNotification", "Enabled", "");
            }
        }

        private static void ZipFolder(bool toastNotify)
        {
            try
            {
                string name = ConfigManager.Read("Game", "Name");
                string saveFolder = ConfigManager.Read("Game", "SavePath");
                string zipPath = ConfigManager.Read("Game", "ZipPath");
                string zipFile = Path.Combine(zipPath, $"{name}_{DateTime.Now:yyyyMMddHHmmss}{".zip"}");
                if (!Directory.Exists(zipPath))
                {
                    Directory.CreateDirectory(zipPath);
                }
                using ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Create);
                foreach (string file in Directory.EnumerateFiles(saveFolder, "*", SearchOption.AllDirectories))
                {
                    zip.CreateEntryFromFile(file, Path.GetRelativePath(saveFolder, file));
                }
                if (toastNotify)
                {
                    NotifyIcon notifyIcon = new()
                    {
                        Icon = System.Drawing.SystemIcons.Application,
                        Visible = true
                    };
                    notifyIcon.ShowBalloonTip(2000, "Backup Completed", $"File name: {zipFile}", ToolTipIcon.None);
                    notifyIcon.BalloonTipClicked += (sender, e) => System.Diagnostics.Process.Start("explorer.exe", zipPath);
                }
                else
                {
                    Console.WriteLine($"Backup created! File: {zipFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void RestoreFolder(bool toastNotify)
        {
            try
            {
                string saveFolder = ConfigManager.Read("Game", "SavePath");
                string zipPath = ConfigManager.Read("Game", "ZipPath");
                string zipFile = "";

                foreach (string file in Directory.EnumerateFiles(zipPath))
                {
                    if (!file.EndsWith(".zip"))
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
                    Console.WriteLine("No backup file found.");
                    return;
                }
                Console.WriteLine($"{"Restoring from"} {zipFile}");
                ZipArchive zip = ZipFile.OpenRead(zipFile);
                ZipFile.ExtractToDirectory(zipFile, saveFolder, true);

                if(toastNotify)
                {
                    NotifyIcon notifyIcon = new()
                    {
                        Icon = System.Drawing.SystemIcons.Application,
                        Visible = true
                    };
                    notifyIcon.ShowBalloonTip(2000, "Back up Restored!", $"From {zipFile}", ToolTipIcon.None);
                    notifyIcon.BalloonTipClicked += (sender, e) => notifyIcon.Dispose();
                }
                else
                {
                    Console.WriteLine($"Backup Restored! using File: {zipFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Main()
        {
            SetupINIFile();

            string NORMAL = Console.IsOutputRedirected ? "" : "\x1b[39m";
            string RED = Console.IsOutputRedirected ? "" : "\x1b[91m";
            string GREEN = Console.IsOutputRedirected ? "" : "\x1b[92m";

            bool toast;
            string toastNotification = ConfigManager.Read("ToastNotification", "Enabled");
            toast = toastNotification.Equals("true", StringComparison.OrdinalIgnoreCase);
            if(string.IsNullOrEmpty(toastNotification))
            {
                Console.WriteLine($"Would you like Toast Notifications? {GREEN}Y / y{NORMAL} for Yes, {RED}N / n{NORMAL} for No");
                string? input = Console.ReadLine();
                toast = input != null && input.Equals("y", StringComparison.CurrentCultureIgnoreCase);
                Console.WriteLine("This will be saved for future, you can edit this by modifying config.ini file.\n");
                ConfigManager.Write("ToastNotification", "Enabled", toast.ToString().ToLower());
            }

            uint saveAtom = GlobalAddAtomA("ZipperHotKeySave");
            uint restoreAtom = GlobalAddAtomA("ZipperHotKeyRestore");
            uint quitAtom = GlobalAddAtomA("ZipperHotKeyQuit");

            Console.Write("Save HotKey:     ");
            RegisterHotKey(0, saveAtom, ConfigManager.GetHotKeys("Save"), ConfigManager.GetModKey("Save"));
            Console.Write("Restore HotKey:  ");
            RegisterHotKey(0, restoreAtom, ConfigManager.GetHotKeys("Restore"), ConfigManager.GetModKey("Restore"));
            Console.Write("Quit HotKey:     ");
            RegisterHotKey(0, quitAtom, ConfigManager.GetHotKeys("Quit"), ConfigManager.GetModKey("Quit"));

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            Console.WriteLine($"\n{GREEN}Ready!{NORMAL}");
            while (GetMessageA(out MSG msg, nint.Zero, 0, 0) != 0)
            {
                if (msg.message != WM_HOTKEY) continue;
                if (msg.wParam == saveAtom)
                {
                    ZipFolder(toast);
                }
                else if (msg.wParam == restoreAtom)
                {
                    RestoreFolder(toast);
                }
                else if (msg.wParam == quitAtom)
                {
                    break;
                }
            }

            void OnProcessExit(object? sender, EventArgs e)
            {
                Console.WriteLine("\nQuit!");
                UnregisterHotKey(0, saveAtom);
                UnregisterHotKey(0, restoreAtom);
                UnregisterHotKey(0, quitAtom);

                GlobalDeleteAtom(saveAtom);
                GlobalDeleteAtom(restoreAtom);
                GlobalDeleteAtom(quitAtom);
            }
        }
    }
}