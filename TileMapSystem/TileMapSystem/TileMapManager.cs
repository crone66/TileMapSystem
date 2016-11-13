/* 
 * Purpose: Manages loaded tileMaps (i.e. dungounes, world, inside buildings)
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */

using System.Collections.Generic;

namespace TileMapSystem
{
    public class TileMapManager
    {
        private List<TileMap> loadedMap;
        private TileMap currentLevel;

        public TileMapManager()
        {
            loadedMap = new List<TileMap>();
        }

        public TileMap Changelevel(int id)
        {
            int index = -1;
            index = loadedMap.FindIndex(m => m.Id == id);
            if(index == -1)
            {
                currentLevel = loadedMap[index];
                return currentLevel;
            }
            return GenerateLevel(id);
        }

        private TileMap GenerateLevel(int id)
        {
            return null;
        }

        private void LoadTileMap(int id)
        {

        }
        
        private void SaveTileMap(TileMap tileMap)
        {

        }

        private void SaveCurrentMap()
        {

        }
    }
}
