using System.Collections.Generic;
using SoulstormRandomTools.Types;

namespace SoulstormRandomTools
{
    public interface ISoulstormItemsProvider
    {
        SoulstormItem[] Races { get; }

        SoulstormItem[] Maps { get; }
        Dictionary<string, SoulstormItem> RacesDict { get; }

        Dictionary<string, SoulstormItem> MapsDict { get; }

    }
}
