namespace SaveBackup.src
{
    public static class KeyMap
    {
        private static readonly ILookup<string, Keys> keyMap;

        static KeyMap()
        {
            var dictionary = new Dictionary<string, Keys>();

            for (char c = 'A'; c <= 'Z'; c++)
            {
                dictionary[c.ToString()] = (Keys)Enum.Parse(typeof(Keys), c.ToString());
            }
            for (int i = 0; i <= 9; i++)
            {
                dictionary[i.ToString()] = (Keys)Enum.Parse(typeof(Keys), "D" + i);
            }
            for (int i = 1; i <= 12; i++)
            {
                dictionary["F" + i] = (Keys)Enum.Parse(typeof(Keys), "F" + i);
            }
            for (int i = 0; i <= 9; i++)
            {
                dictionary["NumPad" + i] = (Keys)Enum.Parse(typeof(Keys), "NumPad" + i);
            }

            keyMap = dictionary.ToLookup(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Keys GetKey(string keyName)
        {
            return keyMap[keyName].FirstOrDefault();
        }
    }
}