using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public static class TileMathHelper
    {
        /// <summary>
        /// Converts invalid tile indices to valid tile indices
        /// </summary>
        /// <param name="index">row or column index</param>
        /// <param name="count">row or column count</param>
        /// <returns>Returns a valid tile index</returns>
        public static int FixTileIndex(int index, int count)
        {
            return (index >= count ? index - ((int)Math.Floor((double)index / (double)count) * count) : (index < 0 ? (((int)Math.Floor((double)index / (double)count) * count) * -1) + index : index));
        }

        /// <summary>
        /// Calculates the distance between two points
        /// </summary>
        /// <param name="x1">source position x</param>
        /// <param name="y1">source position y</param>
        /// <param name="x2">destination position x</param>
        /// <param name="y2">destination position y</param>
        /// <returns>Returns the distance between the given points</returns>
        public static int GetDistance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;
            return (int)Math.Round(Math.Sqrt((x * x) + (y * y)));
        }

        /// <summary>
        /// Checks whether a tile position is out of range or not
        /// </summary>
        /// <param name="row">tile row index</param>
        /// <param name="column">tile column index</param>
        /// <param name="rowCount">tile rows per grid</param>
        /// <param name="columnCount">tile columns per grid</param>
        /// <returns>Returns true when the tile position is out of range</returns>
        public static bool IsOutOfRange(int row, int column, int rowCount, int columnCount)
        {
            return (row < 0 || row >= rowCount || column < 0 || column >= columnCount);
        }

        /// <summary>
        /// Returns a tile index
        /// </summary>
        /// <param name="row">tile row index</param>
        /// <param name="column">tile column index</param>
        /// <param name="columnCount">tile columns per grid</param>
        /// <returns>Returns a tile index</returns>
        public static int ToIndex(int row, int column, int columnCount)
        {
            return (row * columnCount) + column;
        }

        /// <summary>
        /// Returns a new map index based on the current tile position
        /// </summary>
        /// <param name="row">tile row</param>
        /// <param name="column">tile column</param>
        /// <param name="rowCount">tile rows per grid</param>
        /// <param name="columnCount">tile columns per grid</param>
        /// <param name="mapIndex">current map index</param>
        /// <returns>Returns a new map index based on the current tile position</returns>
        public static int GetMapIndex(int row, int column, int rowCount, int columnCount, int mapIndex)
        {
            int translationY = row < 0 ? (int)Math.Floor((double)row / (double)rowCount) : (row >= rowCount ? (int)Math.Floor((double)row / (double)rowCount) : 0);
            int translationX = column < 0 ? (int)Math.Floor((double)column / (double)columnCount) : (column >= columnCount ? (int)Math.Floor((double)column / (double)columnCount) : 0);

            int mapIndexX = mapIndex % 3;
            int mapIndexY = (int)Math.Floor(mapIndex / 3f);
            mapIndexX += translationX;
            mapIndexY += translationY;

            if (mapIndexX >= 0 && mapIndexX < 3 && mapIndexY >= 0 && mapIndexY < 3)
            {
                return mapIndexX + (mapIndexY * 3);
            }
            return -1;
        }

        /// <summary>
        /// Calculates grid id based on a translation of an other grid
        /// </summary>
        /// <param name="translationX">Translation on the X axis (Column)</param>
        /// <param name="translationY">Translation one the Y axis (Row)</param>
        /// <param name="sourceX">Source grid column</param>
        /// <param name="sourceY">Source grid row</param>
        /// <param name="gridRowCount">Number of grid rows</param>
        /// <param name="gridColumnCount">Number of grid columns</param>
        /// <returns></returns>
        public static int CalculateGridTranslation(int translationX, int translationY, int sourceX, int sourceY, int gridRowCount, int gridColumnCount)
        {
            int gridNumX = FixTileIndex(sourceX + translationX, gridColumnCount);
            int gridNumY = FixTileIndex(sourceY + translationY, gridRowCount);
            return ToIndex(gridNumY, gridNumX, gridColumnCount);
        }

        /// <summary>
        /// Merges two flatten arrays
        /// </summary>
        /// <param name="baseMap">Base map to override</param>
        /// <param name="overrides">override information</param>
        /// <returns></returns>
        public static Tile[] MergeMaps(Tile[] baseMap, Tile[] overrides)
        {
            Tile[] result = new Tile[baseMap.Length];
            for (int r = 0; r < result.Length; r++)
            {
                if (overrides[r].Id != 0)
                    result[r] = overrides[r];
                else
                    result[r] = baseMap[r];
            }    
            
            return result;
        }

        /// <summary>
        /// Converts a tile array index to a tile position
        /// </summary>
        /// <param name="index">Tile index in flatten array</param>
        /// <param name="columnCount">Column count of tilemap</param>
        /// <param name="row">Returns the row index of the tile</param>
        /// <param name="column">Returns the column index of the tile</param>
        public static void ToPosition(int index, int columnCount, out int row, out int column)
        {
            row = (int)Math.Floor(index / (double)columnCount);
            column = (index % columnCount);
        }

        public static int FixPositionIndex(int index, int gridCount, int countPerGrid)
        {
            if (index < 0)
                return (gridCount * countPerGrid) - index;
            else if(index > gridCount * countPerGrid)
                return (index - (gridCount * countPerGrid));

            return index;
        }
    }
}
