namespace RandomTools.Types
{
    public class DowItem
    {
        public string Key { get; }

        public string RussianName { get; }
        public string EnglishName { get; }

        public DowItemType ItemType { get; }
        private DowItem(DowItemType itemType, string key, string rusName, string engName)
        {
            ItemType = itemType;

            Key = key;

            RussianName = rusName;
            EnglishName = engName;
        }

        public static DowItem AddRace(string key, string rusName, string engName)
        {
            return new DowItem(DowItemType.Race, key, rusName, engName);
        }

        public static DowItem AddMap2p(string key, string rusName, string engName)
        {
            return new DowItem(DowItemType.Map2p, key, rusName, engName);
        }
        
        public static DowItem AddMap4p(string key, string rusName, string engName)
        {
            return new DowItem(DowItemType.Map4p, key, rusName, engName);
        }
        
        public static DowItem AddMap6p(string key, string rusName, string engName)
        {
            return new DowItem(DowItemType.Map6p, key, rusName, engName);
        }
        
        public static DowItem AddMap8p(string key, string rusName, string engName)
        {
            return new DowItem(DowItemType.Map8p, key, rusName, engName);
        }
    }
}
