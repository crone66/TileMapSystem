using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public static class TileMathHelper
    {
        public static int ConvertToTileIndex(int index, int count)
        {
            return (index >= count ? index - ((int)Math.Floor((double)index / (double)count) * count) : (index < 0 ? (((int)Math.Floor((double)index / (double)count) * count) * -1) + index : index));
        }

        public static int GetDistance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;
            return (int)Math.Round(Math.Sqrt((x * x) + (y * y)));
        }

        public static bool IsOutOfRange(int row, int column, int rowCount, int columnCount)
        {
            return (row < 0 || row >= rowCount || column < 0 || column >= columnCount);
        }

        public static int ToId(int row, int column, int columnCount)
        {
            return (row * columnCount) + column + 1;
        }

        public static void ToLocation(int id, out int row, out int column, int columnCount)
        {
            column = (int)Math.Floor((double)id / (double)columnCount);
            row = id % columnCount;
        }

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

        public static int CalculateGridTranslation(int translationX, int translationY, int sourceX, int sourceY, int gridRowCount, int gridColumnCount)
        {
            int gridNumX = ConvertToTileIndex((sourceX - 1) + translationX, gridColumnCount) + 1;
            int gridNumY = ConvertToTileIndex((sourceY - 1) + translationY, gridRowCount) + 1;
            return gridNumX + ((gridNumY - 1) * gridColumnCount);
        }

    }
}
