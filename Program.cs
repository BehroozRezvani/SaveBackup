
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

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
        IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        IntPtr lParam;
        int time;
        POINT pt;
        int lPrivate;
    }


    const uint MOD_CONTROL = 0x0002;
    const uint MOD_SHIFT = 0x0004;
    const uint WM_HOTKEY = 0x0312;

    static void loadINIFile(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine("INI file not found, defualt shortcuts will be used.");
            Console.WriteLine("Save: CTRL + SHIFT + S");
            Console.WriteLine("Restore: CTRL + SHIFT + Z");
            Console.WriteLine("Quit: CTRL + SHIFT + Q");
            return;
        }
        INIFile iniFile = new INIFile(file);
        string save = iniFile.Read("Shortcuts", "Save");
    }

    public static void Main()
    {
        Console.WriteLine(System.AppContext.BaseDirectory);
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
                using (ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
                {
                    foreach (string file in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
                    {
                        zip.CreateEntryFromFile(file, Path.GetRelativePath(sourceFolder, file));
                    }
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

class INIFile
{
    private string filePath;

    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section,
    string key,
    string val,
    string filePath);

    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section,
    string key,
    string def,
    StringBuilder retVal,
    int size,
    string filePath);

    public INIFile(string filePath)
    {
        this.filePath = filePath;
    }

    public void Write(string section, string key, string value)
    {
        WritePrivateProfileString(section, key, value.ToLower(), this.filePath);
    }

    public string Read(string section, string key)
    {
        StringBuilder SB = new StringBuilder(255);
        int i = GetPrivateProfileString(section, key, "", SB, 255, this.filePath);
        return SB.ToString();
    }

    public string FilePath
    {
        get { return this.filePath; }
        set { this.filePath = value; }
    }
}