using System;
using System.Collections.Generic;
using System.Text;

namespace ThunderHawk.Core
{
    public class OptionsProvider : IOptionsService
    {
        public bool DisableFog { get; set; }
    }
}
