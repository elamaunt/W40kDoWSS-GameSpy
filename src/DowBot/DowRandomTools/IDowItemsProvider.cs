﻿using System.Collections.Generic;
using RandomTools.Types;

namespace RandomTools
{
    public interface IDowItemsProvider
    {
        DowItem[] Races { get; }
        DowItem[] Maps { get; }
    }
}
