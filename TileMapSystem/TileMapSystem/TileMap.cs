/* 
 * Purpose: Stores a complete Tilemap
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */

namespace TileMapSystem
{
    public class TileMap
    {
        private int id;
        private string name;

        private int[,] mapHeight; 
        private int[,] pathPlacement;
        private int[,] objectPlacement;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public TileMap()
        {

        }

        public TileMap(int id, string name, int[,] mapHeight, int[,] pathPlacement, int[,] objectPlacement)
        {
            this.id = id;
            this.name = name;
            this.mapHeight = mapHeight;
            this.pathPlacement = pathPlacement;
            this.objectPlacement = objectPlacement;
        }
    }
}