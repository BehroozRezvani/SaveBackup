﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        //[DllImport("kernel32")]
        //private static extern long WritePrivateProfileString(string section,
        //string key,
        //string val,
        //string filePath);

        //[DllImport("kernel32")]
        //private static extern int GetPrivateProfileString(int Section,
        //string Key,
        //string Value,
        //[MarshalAs(UnmanagedType.LPArray)] byte[] Result,
        //int Size, string FileName);

        public bool HasCTRL()
        {
            if (Read("Save", "CTRL").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        public bool HasALT() {
            if (Read("Save", "ALT").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        public bool HasSHIFT()
        {
            if (Read("Save", "SHIFT").Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public string GetModifierKey()
        {
            return Read("Save", "MODIFIER");
        }

        private string Read(string section, string key)
        {
            StringBuilder SB = new(255) {};
            int i = GetPrivateProfileString(section, key, "", SB, 255, this.FilePath);
            return SB.ToString();
        }
    }
}
