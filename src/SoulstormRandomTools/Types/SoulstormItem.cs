namespace SoulstormRandomTools.Types
{
    public class SoulstormItem
    {
        public string Key { get; }

        public string RussianName { get; }
        public string EnglishName { get; }

        public SoulstormItemType ItemType { get; }
        private SoulstormItem(SoulstormItemType itemType, string key, string rusName, string engName)
        {
            ItemType = itemType;

            Key = key;

            RussianName = rusName;
            EnglishName = engName;
        }

        public static SoulstormItem NewRace(string key, string rusName, string engName)
        {
            return new SoulstormItem(SoulstormItemType.Race, key, rusName, engName);
        }

        public static SoulstormItem NewMap(string key, string rusName, string engName)
        {
            return new SoulstormItem(SoulstormItemType.Map, key, rusName, engName);
        }
    }
}
