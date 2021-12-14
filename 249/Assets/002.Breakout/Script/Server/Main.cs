using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakout.Server
{
    class Main : Gamnet.Util.Singleton<Main>
    {
        public Dictionary<uint, Room> rooms = new Dictionary<uint, Room>();
    }
}
