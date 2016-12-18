using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public struct ObjectTile
    {
        public int TileIndex;
        public int ObjectId;

        public ObjectTile(int tileIndex, int objectId)
        {
            TileIndex = tileIndex;
            ObjectId = objectId;
        }
    }
}
