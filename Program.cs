
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace FolderSaver
{
    public partial class Zippy
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern uint GlobalAddAtomA(string lpString);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern ushort GlobalDeleteAtom(uint nAtom);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, uint keyId, uint fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, uint id);
        [DllImport("user32.dll")]
        static extern int GetMessageA(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            readonly IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            readonly IntPtr lParam;
            readonly int time;
            readonly POINT pt;
            readonly int lPrivate;
        }

        const uint MOD_ALT = 0x0001;
        const uint MOD_CONTROL = 0x0002;
        const uint MOD_SHIFT = 0x0004;
        const uint WM_HOTKEY = 0x0312;

        public static void Main()
        {
            // uint modifier = 0;
            Console.WriteLine(System.AppContext.BaseDirectory);
            string iniFilePath = Path.Combine(System.AppContext.BaseDirectory, "config.ini");
            // if (File.Exists(iniFilePath))
            // {
            //     IniFile iniFile = new(iniFilePath);
            //     if (iniFile.HasHotKey("Save", "ALT"))
            //     {
            //         modifier |= MOD_ALT;
            //     }
            //     if (iniFile.HasHotKey("Save", "CTRL"))
            //     {
            //         modifier |= MOD_CONTROL;
            //     }
            //     if (iniFile.HasHotKey("Save", "SHIFT"))
            //     {
            //         modifier |= MOD_SHIFT;
            //     }

            // }
            // else 
            // {
            //     Console.WriteLine("INI file not found, defualt shortcuts will be used.\n");
            //     Console.WriteLine("Save: CTRL + SHIFT + S");
            //     Console.WriteLine("Restore: CTRL + SHIFT + Z");
            //     Console.WriteLine("Quit: CTRL + SHIFT + Q");
            // }

            static uint GetHotKeys(string iniFilePath, string section)
            {
                uint modifier = 0;
                if (File.Exists(iniFilePath))
                {
                    IniFile iniFile = new(iniFilePath);
                    if (iniFile.HasHotKey("Save", "ALT"))
                    {
                        modifier |= MOD_ALT;
                    }
                    if (iniFile.HasHotKey("Save", "CTRL"))
                    {
                        modifier |= MOD_CONTROL;
                    }
                    if (iniFile.HasHotKey("Save", "SHIFT"))
                    {
                        modifier |= MOD_SHIFT;
                    }
                    if(modifier == 0)
                    {
                        Console.WriteLine("Config file was found but settings could not be loaded");
                    }
                }
                if(modifier == 0)
                {
                    switch (section)
                    {
                        case "Save":
                            Console.WriteLine("Save HotKeys could not be loaded, Save: CTRL + SHIFT + S");
                            break;
                        case "Restore":
                             Console.WriteLine("Restore HotKeys could not be loaded, Save: CTRL + SHIFT + Z");
                             break;
                        case "Quit":
                             Console.WriteLine("Quit HotKeys could not be loaded, Save: CTRL + SHIFT + Q");
                             break;
                        default:
                            Console.WriteLine("Something went wrong, default shortcuts will be used.");
                            Console.WriteLine("Save: CTRL + SHIFT + S");
                            Console.WriteLine("Restore: CTRL + SHIFT + Z");
                            Console.WriteLine("Quit: CTRL + SHIFT + Q");
                        break;
                    }
                    modifier = MOD_CONTROL | MOD_SHIFT;
                }
                return modifier;
            }

            static void ZipFolder()
            {
                try
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string zipFolder = Path.Combine(documentsPath, "NBGI_Backup");
                    string sourceFolder = Path.Combine(documentsPath, "NBGI");
                    string zipFile = Path.Combine(zipFolder, $"NBGI_{DateTime.Now:yyyyMMddHHmmss}.zip");
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

            static void RestoreFolder()
            {
                try
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string zipFolder = Path.Combine(documentsPath, "NBGI_Backup");
                    string sourceFolder = Path.Combine(documentsPath, "NBGI");

                    string zipFile = "";
                    foreach (string file in Directory.EnumerateFiles(zipFolder))
                    {
                        if (!file.EndsWith(".zip"))
                        {
                            continue;
                        }
                        if (zipFile == "")
                        {
                            zipFile = file;
                        }
                        if (String.Compare(zipFile, file) < 0)
                        {
                            zipFile = file;
                        }
                    }
                    if (zipFile == "")
                    {
                        Console.WriteLine("No backup file found");
                        //NotifyIcon notifyIcon = new NotifyIcon();
                        //notifyIcon.Icon = System.Drawing.SystemIcons.Warning;
                        //notifyIcon.Visible = true;
                        //notifyIcon.ShowBalloonTip(5000, "Completed", "This is a toast notification", ToolTipIcon.None);
                        return;
                    }
                    Console.WriteLine($"Restoring from {zipFile}");
                    ZipArchive zip = ZipFile.OpenRead(zipFile);
                    ZipFile.ExtractToDirectory(zipFile, sourceFolder, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            uint atom = GlobalAddAtomA("ZipperHotKey");
            uint atomRestore = GlobalAddAtomA("ZipperHotKeyRestore");
            uint atomQuit = GlobalAddAtomA("ZipperHotKeyQuit");

            // RegisterHotKey(0, atom, modifier, Keys.S);

            bool Save = RegisterHotKey(0, atom, MOD_CONTROL | MOD_SHIFT, Keys.S);
            bool Restore = RegisterHotKey(0, atomRestore, MOD_CONTROL | MOD_SHIFT, Keys.Z);
            bool Quit = RegisterHotKey(0, atomQuit, MOD_CONTROL | MOD_SHIFT, Keys.Q);

            MSG msg;
            while (GetMessageA(out msg, IntPtr.Zero, 0, 0) != 0)
            {
                if (msg.message != WM_HOTKEY) continue;
                System.Console.WriteLine("Hotkey pressed");
                if (msg.wParam == atom)
                {
                    ZipFolder();
                }
                else if (msg.wParam == atomRestore)
                {
                    RestoreFolder();
                }
                else if (msg.wParam == atomQuit)
                {
                    break;
                }
            }

            UnregisterHotKey(0, atom);
            UnregisterHotKey(0, atomRestore);
            UnregisterHotKey(0, atomQuit);

            GlobalDeleteAtom(atom);
            GlobalDeleteAtom(atomRestore);
            GlobalDeleteAtom(atomQuit);
        }        
    }
}