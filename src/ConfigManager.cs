using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SaveBackup.src
{
	public class ConfigManager
	{
		private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, Texts.configFile);
		public ConfigManager()
		{
		}

		[DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
        string key,
        string def,
        StringBuilder retVal,
        int size,
        string filePath);

		const uint MOD_ALT = 0x0001;
		const uint MOD_CONTROL = 0x0002;
		const uint MOD_SHIFT = 0x0004;

		public uint GetHotKeys(string section)
		{
			uint modifier = 0;
			if (File.Exists(_filePath))
			{
				if (HasHotKey(section, Texts.ALT))
				{
					modifier |= MOD_ALT;
				}
				if (HasHotKey(section, Texts.CTRL))
				{
					modifier |= MOD_CONTROL;
				}
				if (HasHotKey(section, Texts.SHIFT))
				{
					modifier |= MOD_SHIFT;
				}
				if (modifier == 0)
				{
					Console.WriteLine(Texts.FileFoundNoConfig);
				}
			}
			if (modifier == 0)
			{
				switch (section)
				{
					case "Save":
						Console.WriteLine(Texts.SaveNotloaded, Texts.SaveHotkey);
						break;
					case "Restore":
						Console.WriteLine(Texts.RestoreNotloaded, Texts.RestoreHotkey);
						break;
					case "Quit":
						Console.WriteLine(Texts.QuitNotloaded, Texts.QuitHotkey);
						break;
					default:
						Console.WriteLine(Texts.SomethingWrongDefaultHotKey);
						Console.WriteLine(Texts.SaveHotkey);
						Console.WriteLine(Texts.RestoreHotkey);
						Console.WriteLine(Texts.QuitHotkey);
						break;
				}
				modifier = MOD_CONTROL | MOD_SHIFT;
			}
			return modifier;
		}

		private Keys GetModKey(string section)
        {
            string modifier = GetModifierKey(section);
            if (File.Exists(_filePath) && modifier != "")
            {
                return KeyMap.GetKey(modifier);
            }
            switch (section)
            {
                case "Save":
                    Console.WriteLine(Texts.SaveNotloaded, Texts.SaveHotkey);
                    return Keys.S;
                case "Restore":
                    Console.WriteLine(Texts.RestoreNotloaded, Texts.RestoreHotkey);
                    return Keys.Z;
                case "Quit":
                    Console.WriteLine(Texts.QuitNotloaded, Texts.QuitHotkey);
                    return Keys.Q;
                default:
                    Console.WriteLine(Texts.SomethingWrongDefaultHotKey);
                    Console.WriteLine(Texts.SaveHotkey);
                    Console.WriteLine(Texts.RestoreHotkey);
                    Console.WriteLine(Texts.QuitHotkey);
                    break;
            }
            return Keys.None;
        }

		private bool HasHotKey(string section, string hotKey)
        {
            return Read(section, hotKey).Equals(Texts.True, StringComparison.OrdinalIgnoreCase);
        }


		private string GetModifierKey(string section)
        { 
            return Read(section, "MODIFIER");
        }

        private string GetSaveFolder()
        {
            string folderPath = Read(Texts.SourceFolder, Texts.Path);
            return string.IsNullOrEmpty(folderPath) ?
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) :
                folderPath;
        }

		private string Read(string section, string key)
        {
            StringBuilder SB = new(255);
            int result = GetPrivateProfileString(section, key, "", SB, 255, _filePath);
            if (result == 0)
            {
                throw new Exception(Texts.ErrorReadingIni);
            }
            else if (result == 255)
            {
                throw new Exception(Texts.BufferTooSmall);
            }
            return SB.ToString();
        }
		
	}
}