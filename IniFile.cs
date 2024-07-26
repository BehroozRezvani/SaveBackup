﻿using System.Runtime.InteropServices;
using System.Text;

namespace FolderSaver
{
    public class IniFile(string filePath)
    {
        private string FilePath { get; } = filePath;

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

        public bool HasHotKey(string section, string hotKey)
        {
            return Read(section, hotKey).Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public string GetModifierKey(string section, string hotKey)
        {
            return Read(section, hotKey);
        }

        public string GetSaveFolder()
        {
            string folderPath = Read("SourceFolder", "Path");
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            return folderPath;
        }

        public string Read(string section, string key)
        {
            StringBuilder SB = new(255);
            int result = GetPrivateProfileString(section, key, "", SB, 255, FilePath);
            if (result == 0)
            {
                throw new Exception("Error reading INI file.");
            }
            else if (result == 254)
            {
                throw new Exception("Buffer size is too small to hold the entire string value.");
            }
            return SB.ToString();
        }
    }
}
