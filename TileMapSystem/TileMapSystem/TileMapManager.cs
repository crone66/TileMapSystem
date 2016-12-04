/* 
 * Purpose: Manages loaded tileMaps (i.e. dungounes, world, inside buildings)
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */

using System;
using System.Collections.Generic;

namespace TileMapSystem
{
    /// <summary>
    /// The tile map manager holds all loaded maps and generator settings
    /// </summary>
    public class TileMapManager
    {
        private StreamedTileMap currentLevel;
        private GeneratorSettings settings;
        private AreaSpread[] spreads;
        private TileMapGenerator generator;
        private List<StreamedTileMap> maps;

        public event EventHandler<GridEventArgs> GridChangeRequested;
        public event EventHandler<GridEventArgs> GridChanged;
        public event EventHandler<GridEventArgs> GridGenerationIsSlow;

        public StreamedTileMap CurrentLevel
        {
            get
            {
                return currentLevel;
            }
        }

        /// <summary>
        /// initzilzes a new tile map manager
        /// </summary>
        /// <param name="settings">Generator settings for the tile map generator</param>
        /// <param name="spreads">Areas that should be placed during the map generation</param>
        public TileMapManager(GeneratorSettings settings, AreaSpread[] spreads)
        {
            this.settings = settings;
            this.spreads = spreads;
            maps = new List<StreamedTileMap>();
            generator = new TileMapGenerator();
        }

        /// <summary>
        /// Subscribe to all streamed tile map events
        /// </summary>
        private void Sub()
        {
            currentLevel.GridChanged += GridChanged;
            currentLevel.GridChangeRequested += GridChangeRequested;
            currentLevel.GridGenerationIsSlow += GridGenerationIsSlow;
        }

        /// <summary>
        /// Unsubscribe all streamed tile map events
        /// </summary>
        private void UnSub()
        {
            if (currentLevel != null)
            {
                currentLevel.GridChanged -= GridChanged;
                currentLevel.GridChangeRequested -= GridChangeRequested;
                currentLevel.GridGenerationIsSlow -= GridGenerationIsSlow;
            }
        }

        /// <summary>
        /// Changes to level to map that will be generated based on the current position
        /// </summary>
        /// <param name="settings">Setting to generate the new map</param>
        /// <param name="spreads">Areas that should be placed on the map</param>
        /// <param name="tileColumn">Tile column index</param>
        /// <param name="tileRow">Tile row index</param>
        /// <param name="disposeCurrentMap">Disposes the current level when true</param>
        public void Changelevel(GeneratorSettings settings, AreaSpread[] spreads, int tileColumn, int tileRow, bool disposeCurrentMap)
        {
            this.settings = settings;
            this.spreads = spreads;
            if (!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(CurrentLevel);
            }
            generator = new TileMapGenerator();
            UnSub();
            currentLevel = generator.GenerateMap(settings, spreads, tileColumn, tileRow);
            Sub();
        }

        /// <summary>
        /// Changes to level to map that will be generated based on the current position
        /// </summary>
        /// <param name="settings">Setting to generate the new map</param>
        /// <param name="tileColumn">Tile column index</param>
        /// <param name="tileRow">Tile row index</param>
        /// <param name="disposeCurrentMap">Disposes the current level when true</param>
        public void Changelevel(GeneratorSettings settings, int tileColumn, int tileRow, bool disposeCurrentMap)
        {
            this.settings = settings;
            if (!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(CurrentLevel);
            }
            generator = new TileMapGenerator();
            UnSub();
            currentLevel = generator.GenerateMap(settings, spreads, tileColumn, tileRow);
            Sub();
        }

        /// <summary>
        /// Changes to level to map that will be generated based on the current position
        /// </summary>
        /// <param name="tileColumn">Tile column index</param>
        /// <param name="tileRow">Tile row index</param>
        /// <param name="disposeCurrentMap">Disposes the current level when true</param>
        public void Changelevel(int tileColumn, int tileRow, bool disposeCurrentMap)
        {
            if (!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(CurrentLevel);
            }

            UnSub();
            currentLevel = generator.GenerateMap(tileColumn, tileRow);
            Sub();
        }

        /// <summary>
        /// Changes to level to a given map
        /// </summary>
        /// <param name="map">New streamed tile map</param>
        /// <param name="disposeCurrentMap">Disposes the current level when true</param>
        public void Changelevel(StreamedTileMap map, bool disposeCurrentMap)
        {
            if(!disposeCurrentMap && currentLevel != null)
            {
                maps.Add(currentLevel);
            }
            currentLevel = map;
        }

        /// <summary>
        /// Updates the current level
        /// </summary>
        /// <param name="currentTileRow">Tile row index</param>
        /// <param name="currentTileColumn">Tile column index</param>
        public void Update(int currentTileRow, int currentTileColumn)
        {
            if (CurrentLevel != null)
            {
                CurrentLevel.Update(currentTileRow, currentTileColumn);
            }
        }
    }
}
