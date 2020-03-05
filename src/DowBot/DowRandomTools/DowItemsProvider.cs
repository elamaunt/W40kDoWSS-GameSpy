using System.Collections.Generic;
using System.Linq;
using RandomTools.Types;

namespace RandomTools
{
    public class DowItemsProvider
    {
        public readonly Dictionary<DowItemType, Dictionary<string, DowItem>> Items;

        public DowItemsProvider(IDowItemsProvider dowItems)
        {
            Items = new Dictionary<DowItemType, Dictionary<string, DowItem>>
            {
                {DowItemType.Race, dowItems.Races.CreateDict()},
                {DowItemType.Map2p, dowItems.Maps.Where(x => x.ItemType == DowItemType.Map2p).CreateDict()},
                {DowItemType.Map4p, dowItems.Maps.Where(x => x.ItemType == DowItemType.Map4p).CreateDict()},
                {DowItemType.Map6p, dowItems.Maps.Where(x => x.ItemType == DowItemType.Map6p).CreateDict()},
                {DowItemType.Map8p, dowItems.Maps.Where(x => x.ItemType == DowItemType.Map8p).CreateDict()}
            };
        }
    }
}