using SoulstormRandomTools.Types;
using System;
using System.Linq;

namespace SoulstormRandomTools
{
    public class Randomizer
    {
        private readonly Random _random = new Random();
        public ISoulstormItemsProvider ItemsProvider { get; }

        public Randomizer(ISoulstormItemsProvider soulstormItemsProvider = null)
        {
            ItemsProvider = soulstormItemsProvider ?? new VanillaSoulstormItemsProvider();
        }


        private SoulstormItem[] GenerateSoulstormItems(SoulstormItemType itemsType, uint count = 1, SoulstormItem[] items = null)
        {
            if (count < 1)
                count = 1;

            if (items == null || items.Length == 0)
                items = itemsType == SoulstormItemType.Race ? ItemsProvider.Races: ItemsProvider.Maps;
            
            var returnItems = new SoulstormItem[count];
            for (var i = 0; i < count; i++)
            {
                returnItems[i] = items[_random.Next(items.Length)];
            }
            return returnItems;
        }


        private SoulstormItem[] ShuffleSoulstormItems(SoulstormItemType itemsType, SoulstormItem[] items = null)
        {
            if (items == null || items.Length == 0)
                items = itemsType == SoulstormItemType.Race ? ItemsProvider.Races : ItemsProvider.Maps;

            items.Shuffle();
            return items;
        }

        public SoulstormItem[] GenerateRandomRaces(uint racesCount, string[] racesFrom = null)
        {
            var races = racesFrom?.Distinct().Where(x => ItemsProvider.RacesDict.ContainsKey(x)).Select(y => ItemsProvider.RacesDict[y]).ToArray();
            return GenerateSoulstormItems(SoulstormItemType.Race, racesCount, races);
        }

        public SoulstormItem[] GenerateRandomMaps(uint mapsCount, string[] mapsFrom = null)
        {
            var maps = mapsFrom?.Distinct().Where(x => ItemsProvider.MapsDict.ContainsKey(x)).Select(y => ItemsProvider.MapsDict[y]).ToArray();
            return GenerateSoulstormItems(SoulstormItemType.Map, mapsCount, maps);
        }


        public SoulstormItem[] ShuffleRaces(string[] racesFrom)
        {
            var races = racesFrom?.Distinct().Where(x => ItemsProvider.RacesDict.ContainsKey(x)).Select(y => ItemsProvider.RacesDict[y]).ToArray();
            return ShuffleSoulstormItems(SoulstormItemType.Race, races);
        }

        public SoulstormItem[] ShuffleMaps(string[] mapsFrom)
        {
            var maps = mapsFrom?.Distinct().Where(x => ItemsProvider.MapsDict.ContainsKey(x)).Select(y => ItemsProvider.MapsDict[y]).ToArray();
            return ShuffleSoulstormItems(SoulstormItemType.Map, maps);
        }

    }
}
