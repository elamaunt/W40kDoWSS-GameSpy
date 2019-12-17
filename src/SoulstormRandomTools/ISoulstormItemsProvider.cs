using SoulstormRandomTools.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoulstormRandomTools
{
    public interface ISoulstormItemsProvider
    {
        SoulstormItem[] Races { get; }
        SoulstormItem[] Maps { get; }
    }
}
