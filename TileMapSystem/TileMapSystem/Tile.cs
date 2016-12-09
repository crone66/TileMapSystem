using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public struct Tile
    {
        public ushort Id;
        public byte Flags;

        public Tile(ushort id, byte flags)
        {
            Id = id;
            Flags = flags;
        }
    }
}
