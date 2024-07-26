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

        public string GetSaveFolder()
        {
            return Read("SourceFolder", "Path") ?? string.Empty;
        }


        public string GetModifierKey(string section, string hotKey)
        {
            return Read(section, hotKey);
        }

        private string Read(string section, string key)
        {
            StringBuilder SB = new(255) {};
            int i = GetPrivateProfileString(section, key, "", SB, 255, this.FilePath);
            return SB.ToString();
        }
    }
}
