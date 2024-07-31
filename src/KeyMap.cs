using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace SaveBackup.src
{
    public static class KeyMap
    {
        private static readonly Lazy<Dictionary<string, Keys>> keyMap = new Lazy<Dictionary<string, Keys>>(InitKeyMap);

        static KeyMap()
        {
            InitKeyMap();
        }

        public static Keys GetKey(string keyName)
        {
            if (keyMap.TryGetValue(keyName, out Keys value))
            {
                return value;
            }
            Console.WriteLine("Key not found: {0}", keyName);
            return Keys.None;
        }

        private static void InitKeyMap()
        {
            for (char c = 'A'; c <= 'Z'; c++)
            {
                keyMap[c.ToString()] = (Keys)Enum.Parse(typeof(Keys), c.ToString());
            }
            for (int i = 0; i <= 9; i++)
            {
                keyMap[i.ToString()] = (Keys)Enum.Parse(typeof(Keys), "D" + i);
            }
            for (int i = 1; i <= 12; i++)
            {
                keyMap["F" + i] = (Keys)Enum.Parse(typeof(Keys), "F" + i);
            }
            for (int i = 0; i <= 9; i++)
            {
                keyMap["NumPad" + i] = (Keys)Enum.Parse(typeof(Keys), "NumPad" + i);
            }
        }
    }
}