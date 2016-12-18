/* 
 * Purpose: Can be used for huge tilemaps that doesnt fit into memory. (Loads and manages mapparts)
 * Author: Marcel Croonenbroeck
 * Date: 13.11.2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TileMapSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamedTileMap
    {
        private List<TileMapPart> maps;
        private int gridColumnCount;
        private int gridRowCount;
        private int gridRow;
        private int gridColumn;
        private int tileRowCount;
        private int tileColumnCount;
        private int screenWidth;
        private int screenHeight;
        private int tileSize;
        private int tileRow;
        private int tileColumn;
        private int currentMapId;
        private int currentMapIndex;
        private TileMapGenerator generator;
        private StreamedTileMap newMap;
        private bool newMapAvalible;
        private bool newMapRequested;

        public event EventHandler<GridEventArgs> GridChangeRequested;
        public event EventHandler<GridEventArgs> GridChanged;
        public event EventHandler<GridEventArgs> GridGenerationIsSlow;

        public List<TileMapPart> Maps
        {
            get
            {
                return maps;
            }
        }

        public int ScreenWidth
        {
            get
            {
                return screenWidth;
            }

            set
            {
                if (value > 0)
                    screenWidth = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int ScreenHeight
        {
            get
            {
                return screenHeight;
            }

            set
            {
                if (value > 0)
                    screenHeight = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }

        public int TileRowCount
        {
            get
            {
                return tileRowCount;
            }
        }

        public int TileColumnCount
        {
            get
            {
                return tileColumnCount;
            }
        }

        public int GridRow
        {
            get
            {
                return gridRow;
            }
        }

        public int GridColumn
        {
            get
            {
                return gridColumn;
            }
        }

        public int GridColumnCount
        {
            get
            {
                return gridColumnCount;
            }
        }

        public int GridRowCount
        {
            get
            {
                return gridRowCount;
            }
        }

        public int CurrentMapIndex
        {
            get
            {
                return currentMapIndex;
            }

            private set
            {
                currentMapIndex = value;
            }
        }

        /// <summary>
        /// Initzializes a new streamed tile map
        /// </summary>
        /// <param name="generator">Tile map generator</param>
        /// <param name="spawnTileRow">Tile row index</param>
        /// <param name="spawnTileColumn">Tile column index</param>
        /// <param name="gridColumnCount">Number of grid columns</param>
        /// <param name="gridRowCount">Number of grid rows</param>
        /// <param name="tileRowCount">Number of tile rows per grid</param>
        /// <param name="tileColumnCount">Number of tile columns per grid</param>
        /// <param name="tileSize">Tile size in pixel</param>
        public StreamedTileMap(TileMapGenerator generator, int spawnTileRow, int spawnTileColumn, int gridColumnCount, int gridRowCount, int tileRowCount, int tileColumnCount, int tileSize)
        {
            maps = new List<TileMapPart>();
            this.gridColumnCount = gridColumnCount;
            this.gridRowCount = gridRowCount;
            this.tileSize = tileSize;
            this.tileRowCount = tileRowCount;
            this.tileColumnCount = tileColumnCount;
            this.generator = generator;
            currentMapIndex = 4;
            tileRow = spawnTileRow;
            tileColumn = spawnTileColumn;
            gridRow = (int)Math.Floor((double)spawnTileRow / (double)tileRowCount);
            gridColumn = (int)Math.Floor((double)spawnTileColumn / (double)tileColumnCount);

            gridRow = TileMathHelper.FixTileIndex(gridRow, gridRowCount);
            gridColumn = TileMathHelper.FixTileIndex(gridColumn, gridColumnCount);
        }

        /// <summary>
        /// Initzializes a new streamed tile map
        /// </summary>
        /// <param name="spawnTileRow">Tile row index</param>
        /// <param name="spawnTileColumn">Tile column index</param>
        /// <param name="gridColumnCount">Number of grid columns</param>
        /// <param name="gridRowCount">Number of grid rows</param>
        /// <param name="tileRowCount">Number of tile rows per grid</param>
        /// <param name="tileColumnCount">Number of tile columns per grid</param>
        /// <param name="tileSize">Tile size in pixel</param>
        /// <param name="maps">Loaded grids</param>
        public StreamedTileMap(int spawnTileRow, int spawnTileColumn, int gridColumnCount, int gridRowCount, int tileRowCount, int tileColumnCount, int tileSize, List<TileMapPart> maps)
        {
            this.maps = maps;
            this.gridColumnCount = gridColumnCount;
            this.gridRowCount = gridRowCount;
            this.tileSize = tileSize;
            this.tileRowCount = tileRowCount;
            this.tileColumnCount = tileColumnCount;
            currentMapIndex = 4;
            tileRow = spawnTileRow;
            tileColumn = spawnTileColumn;
            gridRow = (int)Math.Floor((double)spawnTileRow / (double)tileRowCount);
            gridColumn = (int)Math.Floor((double)spawnTileColumn / (double)tileColumnCount);
            gridRow = TileMathHelper.FixTileIndex(gridRow, gridRowCount);
            gridColumn = TileMathHelper.FixTileIndex(gridColumn, gridColumnCount);
        }

        /// <summary>
        /// Adds a new grid to the streamed tile map
        /// </summary>
        /// <param name="mapPart"></param>
        public void Add(TileMapPart mapPart)
        {
            if(!maps.Contains(mapPart))
                maps.Add(mapPart);
        }

        /// <summary>
        /// Updates the grids based one the given tile location
        /// </summary>
        /// <param name="currentTileRow">Tile row index</param>
        /// <param name="currentTileColumn">Tile column index</param>
        public void Update(int currentTileRow, int currentTileColumn)
        {
            int newGridRow;
            int newGridColumn;
            ConvertTileToGridPosition(currentTileRow, currentTileColumn, out newGridRow, out newGridColumn);

            currentMapId = TileMathHelper.ToIndex(newGridRow, newGridColumn, GridColumnCount);
            tileRow = TileMathHelper.FixTileIndex(currentTileRow, TileRowCount);
            tileColumn = TileMathHelper.FixTileIndex(currentTileColumn, TileColumnCount);

            TryMapUpdate(newGridRow, newGridColumn);

            CurrentMapIndex = maps.FindIndex(m => m.Id == currentMapId);

            if ((newGridRow != GridRow || newGridColumn != GridColumn) && !newMapRequested)
            {
                GridChangeRequested?.Invoke(this, new GridEventArgs(GridRow, GridColumn, maps[4].GridRow, maps[4].GridColumn, false));
                gridRow = newGridRow;
                gridColumn = newGridColumn;
                Resize(currentTileRow, currentTileColumn);
            }
        }

        /// <summary>
        /// Determine all tiles on screen based on the given viewport
        /// </summary>
        /// <param name="viewportWidth">Viewport width in pixel</param>
        /// <param name="viewportHeight">Viewport height in pixel</param>
        /// <returns>Returns a flatten byte array which represants the tiles</returns>
        public Tile[] GetTileMapInScreen(int viewportWidth, int viewportHeight)
        {
            ScreenWidth = viewportWidth;
            ScreenHeight = viewportHeight;

            int screenColumnCount = (viewportWidth / tileSize);
            int screenRowCount = (viewportHeight / tileSize);
            Tile[] tilesInScreen = new Tile[screenRowCount * screenColumnCount];

            int posRow = tileRow + (screenRowCount / 2);
            int negRow = tileRow - (screenRowCount / 2);
            int posColumn = tileColumn + (screenColumnCount / 2);
            int negColumn = tileColumn - (screenColumnCount / 2);

            int rowIndex = 0;
            for (int r = negRow; r < posRow; r++)
            {
                int columnIndex = 0;
                for (int c = negColumn; c < posColumn; c++)
                {
                    int newMapIndex = CurrentMapIndex;
                    int realRow = r;
                    int realColumn = c;
                    if (TileMathHelper.IsOutOfRange(r, c, TileRowCount, TileColumnCount))
                    {
                        newMapIndex = TileMathHelper.GetMapIndex(r, c, TileRowCount, TileColumnCount, newMapIndex);
                        realRow = TileMathHelper.FixTileIndex(r, TileRowCount);
                        realColumn = TileMathHelper.FixTileIndex(c, TileColumnCount);
                    }

                    //GetValues and merge
                    if (newMapIndex < 0)
                    {
                        tilesInScreen[TileMathHelper.ToIndex(rowIndex, columnIndex++, screenColumnCount)] = new Tile(ushort.MaxValue, 255);
                        int newGridRow = (int)Math.Floor((double)r / (double)TileRowCount);
                        int newGridColumn = (int)Math.Floor((double)c / (double)TileColumnCount);
                        GridGenerationIsSlow?.Invoke(this, new GridEventArgs(newGridRow, newGridColumn, GridRow, GridColumn, false));
                    }
                    else
                        tilesInScreen[TileMathHelper.ToIndex(rowIndex, columnIndex++, screenColumnCount)] = maps[newMapIndex].MapSurface[TileMathHelper.ToIndex(realRow, realColumn, TileColumnCount)]; 
                }
                rowIndex++;
            }

            return tilesInScreen;
        }

        /// <summary>
        /// Converts a pixel position into a tile index
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>Returns a tile index from flatten map array</returns>
        public int GetTileIndex(int x, int y)
        {
            int column = x / tileSize;
            int row = y / tileSize;
            return TileMathHelper.ToIndex(row, column, GridColumnCount);
        }

        /// <summary>
        /// Determine the flag of a given tile location
        /// </summary>
        /// <param name="row">Tile row index</param>
        /// <param name="column">Tile column index</param>
        /// <returns>Returns the flag of a tile</returns>
        public bool GetTileValue(int row, int column, out Tile tile)
        {
            tile = new Tile();
            int newGridRow;
            int newGridColumn;
            ConvertTileToGridPosition(row, column, out newGridRow, out newGridColumn);

            int tileMapId = TileMathHelper.ToIndex(newGridRow, newGridColumn, GridColumnCount);
            int tileRow = TileMathHelper.FixTileIndex(row, TileRowCount);
            int tileColumn = TileMathHelper.FixTileIndex(column, TileColumnCount);
            int currentMapIndex = maps.FindIndex(m => m.Id == tileMapId);

            if (currentMapIndex != -1)
            {
                tile = maps[currentMapIndex].MapSurface[TileMathHelper.ToIndex(tileRow, tileColumn, TileColumnCount)];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets a tile value
        /// </summary>
        /// <param name="row">Tile row index</param>
        /// <param name="column">Tile column index</param>
        /// <param name="value">New tile value</param>
        public bool SetTileValue(int row, int column, Tile value)
        {
            int newGridRow;
            int newGridColumn;
            ConvertTileToGridPosition(row, column, out newGridRow, out newGridColumn);

            int tileMapId = TileMathHelper.ToIndex(newGridRow, newGridColumn, GridColumnCount);
            int tileRow = TileMathHelper.FixTileIndex(row, TileRowCount);
            int tileColumn = TileMathHelper.FixTileIndex(column, TileColumnCount);
            int currentMapIndex = maps.FindIndex(m => m.Id == tileMapId);
            if (currentMapIndex != -1)
            {
                maps[currentMapIndex].MapSurface[TileMathHelper.ToIndex(tileRow, tileColumn, TileColumnCount)] = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a grid position of a given tile position
        /// </summary>
        /// <param name="row">Tile row index</param>
        /// <param name="column">Tile column index</param>
        /// <param name="gridRow">Returns the grid row index</param>
        /// <param name="gridColumn">Returns the grid column index</param>
        public void ConvertTileToGridPosition(int row, int column, out int gridRow, out int gridColumn)
        {
            gridRow = (int)Math.Floor((double)row / (double)TileRowCount);
            gridColumn = (int)Math.Floor((double)column / (double)TileColumnCount);
            if (TileMathHelper.IsOutOfRange(gridRow, gridColumn, GridRowCount, GridColumnCount))
            {
                gridRow = TileMathHelper.FixTileIndex(gridRow, GridRowCount);
                gridColumn = TileMathHelper.FixTileIndex(gridColumn, GridColumnCount);
            }
        }

        /// <summary>
        /// Changes the map to a new streamed tile map (Tile map will be updated on the next update)
        /// </summary>
        /// <param name="map">New streamed tile map</param>
        public void ChangeMap(StreamedTileMap map)
        {
            newMap = map;
            newMapAvalible = true;
        }

        /// <summary>
        /// Starts new map generation 
        /// </summary>
        /// <param name="currentTileRow">Tile row index</param>
        /// <param name="currentTileColumn">tile column index</param>
        private void Resize(int currentTileRow, int currentTileColumn)
        {
            if (generator != null)
            {
                if (newMap != null)
                {
                    newMapAvalible = true;
                    if (TryMapUpdate(gridRow, gridColumn))
                        return;
                }

                newMapAvalible = false;
                newMapRequested = true;
                new Thread(() =>
                {
                    newMap = generator.GenerateMap(currentTileColumn, currentTileRow);
                    newMapAvalible = true;
                }).Start();
            }
        }

        /// <summary>
        /// Tries to update the map when the maps it's fully loaded
        /// </summary>
        /// <param name="currentGridRow">Grid row index</param>
        /// <param name="currentGridColumn">Grid column index</param>
        /// <returns>Returns true on success</returns>
        private bool TryMapUpdate(int currentGridRow, int currentGridColumn)
        {
            if (newMapAvalible)
            {
                if ((currentGridRow == newMap.maps[4].GridRow && currentGridColumn == newMap.maps[4].GridColumn) || newMapRequested)
                {
                    List<TileMapPart> oldMap = maps;
                    maps = newMap.maps;
                    newMap.maps = oldMap;
                    CurrentMapIndex = 4;
                    newMapAvalible = false;
                    newMapRequested = false;
                    GridChanged?.Invoke(this, new GridEventArgs(GridRow, GridColumn, newMap.maps[4].GridRow, newMap.maps[4].GridColumn, !newMapRequested));
                    return true;
                }
                newMapAvalible = false;
            }
            return false;
        }
    }
}
