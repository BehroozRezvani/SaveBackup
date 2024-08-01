using System.IO.Compression;
using System.Runtime.InteropServices;

namespace SaveBackup.src
{
    public partial class Zippy
    {
        private static void ZipFolder()
        {
            try
            {
                string saveFolder = ConfigManager.Read("SaveFolder", "Path");
                string name = ConfigManager.Read("SaveFolder", "Name");
                string zipPath = ConfigManager.Read("SaveFolder", "ZipPath");
                string zipFile = Path.Combine(zipPath, $"{name}_{DateTime.Now:yyyyMMddHHmmss}{Texts.dotZip}");
                if (!Directory.Exists(zipPath))
                {
                    Directory.CreateDirectory(zipPath);
                }
                using ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Create);
                foreach (string file in Directory.EnumerateFiles(saveFolder, "*", SearchOption.AllDirectories))
                {
                    zip.CreateEntryFromFile(file, Path.GetRelativePath(saveFolder, file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void RestoreFolder()
        {
            try
            {
                string saveFolder = ConfigManager.Read("SaveFolder", "Path");
                string zipPath = ConfigManager.Read("SaveFolder", "ZipPath");
                string zipFile = "";

                foreach (string file in Directory.EnumerateFiles(zipPath))
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
                ZipFile.ExtractToDirectory(zipFile, saveFolder, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void Main()
        {
            uint saveAtom = GlobalAddAtomA(Texts.ZipperHKSave);
            uint restoreAtom = GlobalAddAtomA(Texts.ZipperHKRestore);
            uint quitAtom = GlobalAddAtomA(Texts.ZipperHKQuit);

            RegisterHotKey(0, saveAtom, ConfigManager.GetHotKeys(Texts.Save), ConfigManager.GetModKey(Texts.Save));
            RegisterHotKey(0, restoreAtom, ConfigManager.GetHotKeys(Texts.Restore), ConfigManager.GetModKey(Texts.Restore));
            RegisterHotKey(0, quitAtom, ConfigManager.GetHotKeys(Texts.Quit), ConfigManager.GetModKey(Texts.Quit));

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