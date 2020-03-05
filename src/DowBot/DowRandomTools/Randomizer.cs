using System;
using System.Collections.Generic;
using System.Linq;
using RandomTools.Types;

namespace RandomTools
{
    public class Randomizer
    {
        private readonly Random _random = new Random();
        private DowItemsProvider ItemsProvider { get; }

        public Randomizer(IDowItemsProvider dowItemsProvider = null)
        {
            var items =  dowItemsProvider ?? new DefaultDowItemsProvider();
            ItemsProvider = new DowItemsProvider(items);
        }
        public DowItem[] GenerateRandomItems(DowItemType itemType, uint itemsCount, IEnumerable<string> inItems = null)
        {
            var items = inItems?.Distinct().Where(x => 
                ItemsProvider.Items[itemType].ContainsKey(x)).Select(y => ItemsProvider.Items[itemType][y]).ToArray();

            if (items == null || items.Length == 0)
                items = ItemsProvider.Items[itemType].Values.ToArray();
                
            if (itemsCount < 1)
                itemsCount = 1;
            var returnItems = new DowItem[itemsCount];
            for (var i = 0; i < itemsCount; i++)
            {
                var item = _random.Next(items.Length);
                returnItems[i] = items[item];
            }

            return returnItems;
        }


        public DowItem[] ShuffleItems(DowItemType itemType, IEnumerable<string> inItems)
        {
            var items = inItems?.Where(x => 
                ItemsProvider.Items[itemType].ContainsKey(x)).Select(y => ItemsProvider.Items[itemType][y]).ToArray();
            
            if (items == null || items.Length == 0)
                return new DowItem[] { };

            items.Shuffle();
            return items;
        }
    }
}
