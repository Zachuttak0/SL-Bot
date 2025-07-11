using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL_Bot
{
    public class Config
    {
        public string Token { get; set; } = "";

        public int ServerNumber { get; set; } = 1;

        public HashSet<KeyValuePair<ulong, int>> Numbers { get; set; } = new();
    }
}
