using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayHologramFiles
{
    class HologramListItem
    {
        public string id { get; set; }
        public string name { get; set; }

        public bool runningItem { get; set; }

        public HologramListItem()
        {
            runningItem = false;
        }
    }
}
