using System.IO.Compression;
using System.Runtime.InteropServices;

namespace SaveBackup.src
{
    public partial class Zippy
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern uint GlobalAddAtomA([MarshalAs(UnmanagedType.LPWStr)] string lpString);

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


        private static void ZipFolder(bool toastNotify)
        {
            try
            {
                string saveFolder = ConfigManager.Read("SaveFolder", "SavePath");
                string name = ConfigManager.Read("SaveFolder", "Name");
                string zipPath = ConfigManager.Read("SaveFolder", "ZipPath");
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
                    Console.WriteLine($"Backup created! File: {zipFile} at directory: {zipPath}");
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
                string saveFolder = ConfigManager.Read("SaveFolder", "SavePath");
                string zipPath = ConfigManager.Read("SaveFolder", "ZipPath");
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
                    Console.WriteLine($"Backup Restored! using File: {zipFile} at directory: {zipPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Main()
        {
            bool toast;
            string toastNotification = ConfigManager.Read("ToastNotification", "Enabled");
            toast = toastNotification.Equals("true", StringComparison.OrdinalIgnoreCase);
            if(string.IsNullOrEmpty(toastNotification))
            {
                Console.WriteLine("Would you like Toast Notifications? Yes: y, No: n ");
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
            Console.WriteLine("\nReady!");
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