/* 
 * Purpose: Can be used for huge tilemaps that doesnt fit into memory. (Loads and manages mapparts)
 * Author: Marcel Croonenbroeck
 * Date: 13.11.2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
namespace TileMapSystem
{
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

        public StreamedTileMap(int spawnTileRow, int spawnTileColumn, int gridColumnCount, int gridRowCount, int tileRowCount, int tileColumnCount, int tileSize)
        {
            maps = new List<TileMapPart>();
            this.gridColumnCount = gridColumnCount;
            this.gridRowCount = gridRowCount;
            this.tileSize = tileSize;
            this.tileRowCount = tileRowCount;
            this.tileColumnCount = tileColumnCount;
            tileRow = spawnTileRow;
            tileColumn = spawnTileColumn;
            gridRow = (int)Math.Floor((double)spawnTileRow / (double)tileRowCount);
            gridColumn = (int)Math.Floor((double)spawnTileColumn / (double)tileColumnCount);
        }

        public StreamedTileMap(int spawnTileRow, int spawnTileColumn, int gridColumnCount, int gridRowCount, int tileRowCount, int tileColumnCount, int tileSize, List<TileMapPart> maps)
        {
            this.maps = maps;
            this.gridColumnCount = gridColumnCount;
            this.gridRowCount = gridRowCount;
            this.tileSize = tileSize;
            this.tileRowCount = tileRowCount;
            this.tileColumnCount = tileColumnCount;
            tileRow = spawnTileRow;
            tileColumn = spawnTileColumn;
            gridRow = (int)Math.Floor((double)spawnTileRow / (double)tileRowCount);
            gridColumn = (int)Math.Floor((double)spawnTileColumn / (double)tileColumnCount);

            Update(tileRow, tileColumn);
        }

        public void Add(TileMapPart mapPart)
        {
            if(!maps.Contains(mapPart))
                maps.Add(mapPart);
        }

        public void Update(int currentTileRow, int currentTileColumn)
        {
            int newGridRow = (int)Math.Floor((double)currentTileRow / (double)tileRowCount);
            int newGridColumn = (int)Math.Floor((double)currentTileColumn / (double)tileColumnCount);
            if (TileMathHelper.IsOutOfRange(newGridRow, newGridColumn, gridRowCount, gridColumnCount))
            {
                newGridRow = TileMathHelper.ConvertToTileIndex(newGridRow, gridRowCount);
                newGridColumn = TileMathHelper.ConvertToTileIndex(newGridColumn, gridColumnCount);
            }
            currentMapId = TileMathHelper.ToId(newGridRow, newGridColumn, gridColumnCount);
            tileRow = TileMathHelper.ConvertToTileIndex(currentTileRow, tileRowCount);
            tileColumn = TileMathHelper.ConvertToTileIndex(currentTileColumn, tileColumnCount);
            currentMapIndex = maps.FindIndex(m => m.Id == currentMapId);

            if (newGridRow != gridRow || newGridColumn != gridColumn)
            {
                Resize();
                Shrink();
                //load new Maps
                //Load, Shrink and resize (threaded?)
            }
        }

        public int[,] GetTileMapInScreen(int screenWidth, int screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            int screenColumnCount = (screenWidth / tileSize) + 1;
            int screenRowCount = (screenHeight / tileSize) + 1;
            int[,] tilesInScreen = new int[screenRowCount, screenColumnCount];

            int posRow = tileRow + (screenRowCount / 2);
            int negRow = tileRow - (screenRowCount / 2);
            int posColumn = tileColumn + (screenColumnCount / 2);
            int negColumn = tileColumn - (screenColumnCount / 2);

            int rowIndex = 0;
            for (int r = negRow; r < posRow + 1; r++)
            {
                int columnIndex = 0;
                for (int c = negColumn; c < posColumn + 1; c++)
                {
                    int newMapIndex = currentMapIndex;
                    int realRow = r;
                    int realColumn = c;
                    if (TileMathHelper.IsOutOfRange(r, c, tileRowCount, tileColumnCount))
                    {
                        newMapIndex = TileMathHelper.GetMapIndex(r, c, tileRowCount, tileColumnCount, newMapIndex);
                        realRow = TileMathHelper.ConvertToTileIndex(r, tileRowCount);
                        realColumn = TileMathHelper.ConvertToTileIndex(c, tileColumnCount);
                    }

                    //GetValues and merge
                    tilesInScreen[rowIndex, columnIndex++] = maps[newMapIndex].MapSurface[realRow, realColumn]; 
                }
                rowIndex++;
            }

            return tilesInScreen;
        }

        public int GetTile(int row, int column)
        {
            int newGridRow = (int)Math.Floor((double)row / (double)tileRowCount);
            int newGridColumn = (int)Math.Floor((double)column / (double)tileColumnCount);
            if (TileMathHelper.IsOutOfRange(newGridRow, newGridColumn, gridRowCount, gridColumnCount))
            {
                newGridRow = TileMathHelper.ConvertToTileIndex(newGridRow, gridRowCount);
                newGridColumn = TileMathHelper.ConvertToTileIndex(newGridColumn, gridColumnCount);
            }

            int tileMapId = TileMathHelper.ToId(newGridRow, newGridColumn, gridColumnCount);
            int tileRow = TileMathHelper.ConvertToTileIndex(row, tileRowCount);
            int tileColumn = TileMathHelper.ConvertToTileIndex(column, tileColumnCount);
            int currentMapIndex = maps.FindIndex(m => m.Id == tileMapId);
            return maps[currentMapIndex].MapSurface[tileRow, tileColumn];
        }


        private void Resize()
        {

        }

        private void Shrink()
        {
            
        }
    }

    public struct TileMapPart
    {
        public int[,] MapSurface;
        public int[,] PathPlacement;
        public int[,] ObjectPlacement;
        public int GridColumn;
        public int GridRow;
        public int Id;

        public TileMapPart(int id, int[,] mapSurface, int[,] pathPlactment, int[,] objectPlacement, int gridColumn, int gridRow)
        {
            Id = id;
            MapSurface = mapSurface;
            PathPlacement = pathPlactment;
            ObjectPlacement = objectPlacement;
            GridColumn = gridColumn;
            GridRow = gridRow;
        }
    }
}
