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
        private StreamedTileMap currentLevel;
        private GeneratorSettings settings;
        private AreaSpread[] spreads;
        private TileMapGenerator generator;
        private List<StreamedTileMap> maps;

        public StreamedTileMap CurrentLevel
        {
            get
            {
                return currentLevel;
            }
        }

        public TileMapManager(GeneratorSettings settings, AreaSpread[] spreads, int x, int y)
        {
            this.settings = settings;
            this.spreads = spreads;
            maps = new List<StreamedTileMap>();
            generator = new TileMapGenerator();
            currentLevel = generator.GenerateMap(settings, spreads, x, y);
        }

        public void Changelevel(GeneratorSettings settings, AreaSpread[] spreads, int x, int y, bool disposeCurrentMap)
        {
            this.settings = settings;
            this.spreads = spreads;
            if (!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(CurrentLevel);
            }
            generator = new TileMapGenerator();
            currentLevel = generator.GenerateMap(settings, spreads, x, y);
        }

        public void Changelevel(GeneratorSettings settings, int x, int y, bool disposeCurrentMap)
        {
            this.settings = settings;
            if (!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(CurrentLevel);
            }
            generator = new TileMapGenerator();
            currentLevel = generator.GenerateMap(settings, spreads, x, y);
        }

        public void Changelevel(int x, int y, bool disposeCurrentMap)
        {
            if (!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(CurrentLevel);
            }
            currentLevel = generator.GenerateMap(x, y);
        }

        public void Changelevel(StreamedTileMap map, bool disposeCurrentMap)
        {
            if(!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(currentLevel);
            }
            currentLevel = map;
        }

        public void Update(int currentTileRow, int currentTileColumn)
        {
            if (CurrentLevel != null)
            {
                CurrentLevel.Update(currentTileRow, currentTileColumn);
            }
        }
    }
}
