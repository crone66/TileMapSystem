/* 
 * Purpose: Generates tilemap or partial tilemap
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */
using System;

namespace TileMapSystem
{
    public class TileMapGenerator
    {
        private Random random;
        private int seed;
        public TileMapGenerator()
        {

        }

        public TileMapGenerator(int seed)
        {
            this.seed = seed;
            random = new Random(seed);
        }

        public int[,] GenerateMap(GeneratorSettings settings, AreaSpread area)
        {
            if (settings.Seed >= 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            int[,] map = PopulateMap(settings);
            int rowCount = map.GetUpperBound(0);
            int columnCount = map.GetUpperBound(1);

            CreateLayerOne(map, rowCount, columnCount, settings.TileSize, settings.MeterPerTile, area, settings.RadiusOfCylinder);

            return map;
        }

        public int[,] GenerateMap(GeneratorSettings settings, AreaSpread[] areas)
        {
            if (settings.Seed >= 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            int[,] map = PopulateMap(settings);
            int rowCount = map.GetUpperBound(0);
            int columnCount = map.GetUpperBound(1);
            for (int i = 0; i < areas.Length; i++)
            {
                CreateLayerOne(map, rowCount, columnCount, settings.TileSize, settings.MeterPerTile, areas[i], settings.RadiusOfCylinder);
            }
            return map;
        }

        public StreamedTileMap GenerateMap(GeneratorSettings settings, AreaSpread[] areas, int startLocationX, int startLocationY)
        {
            if (settings.Seed > 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            double tilesPerGrid = Math.Round(settings.MeterPerGrid / settings.MeterPerTile);
            int gridsPerRow = (int)Math.Ceiling(GetMapSize(settings) / tilesPerGrid);

            int GridLocationX = (int)Math.Ceiling(startLocationX / tilesPerGrid);
            int GridLocationY = (int)Math.Ceiling(startLocationY / tilesPerGrid);

            int[][,] maps = new int[9][,];
            for (int i = 0; i < maps.Length; i++)
            {
                maps[i] = new int[(int)tilesPerGrid, (int)tilesPerGrid];
            }

            int[] suroundingGrids = new int[maps.Length];
            //Line 1
            suroundingGrids[0] = CalculateGridTranslation(-1, -1, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            suroundingGrids[1] = CalculateGridTranslation(0, -1, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            suroundingGrids[2] = CalculateGridTranslation(1, -1, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            //Line 2
            suroundingGrids[3] = CalculateGridTranslation(-1, 0, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            suroundingGrids[4] = CalculateGridTranslation(0, 0, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow); //Center
            suroundingGrids[5] = CalculateGridTranslation(1, 0, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            //Line 3
            suroundingGrids[6] = CalculateGridTranslation(1, 1, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            suroundingGrids[7] = CalculateGridTranslation(0, 1, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);
            suroundingGrids[8] = CalculateGridTranslation(-1, 1, GridLocationX, GridLocationY, gridsPerRow, gridsPerRow);

            StreamedTileMap streamedTileMap = new StreamedTileMap(startLocationY, startLocationX);


            for (int i = 0; i < suroundingGrids.Length; i++)
            {
                random = new Random(suroundingGrids[i] * settings.Seed);
                for (int j = 0; j < areas.Length; j++)
                {
                    CreateLayerOne(i, maps, (int)tilesPerGrid, (int)tilesPerGrid, settings.TileSize, settings.MeterPerTile, areas[j], settings.RadiusOfCylinder);
                }


            }

            DefragmentMaps(maps);
            for (int i = 0; i < maps.Length; i++)
            {
#warning "GridLocation muss korrekt berechnen-"
                TileMapPart part = new TileMapPart(maps[i], null, null, GridLocationX, GridLocationY);
                streamedTileMap.Add(part);
            }


            return streamedTileMap;
        }

        private int[,] PopulateMap(GeneratorSettings settings)
        {
            if (settings.RadiusOfCylinder)
            {
                int radiusInMeter = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter);

                float u = 2f * Convert.ToSingle(Math.PI) * radiusInMeter;
                int size = (int)Math.Round(u / settings.MeterPerTile);
                return new int[size, size];
            }
            else
            {
                int height = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter + 1);
                int width = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter + 1);
                return new int[height, width];
            }
        }

        private int GetMapSize(GeneratorSettings settings)
        {
            int radiusInMeter = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter);

            float u = 2f * Convert.ToSingle(Math.PI) * radiusInMeter;
            return (int)Math.Round(u / settings.MeterPerTile);
        }

        private void CreateLayerOne(int mapIndex, int[][,] maps, int rowCount, int columnCount, int tileSize, float meterPerTiles, AreaSpread area, bool allowEdgeOverflow)
        {
            int tilesChanged = 0;
            int tileCount = rowCount * columnCount;
            while (((float)tilesChanged) / ((float)tileCount) < area.Percentage)
            {
                int row = random.Next(0, rowCount);
                int column = random.Next(0, columnCount);
                int minRadius = random.Next(area.MinSizeInMeter, area.MaxSizeInMeter + 1);

                if (maps[mapIndex][row, column] == 0 || maps[mapIndex][row, column] == area.Id)
                {
                    if (area.SpreadType == SpreadOption.Circle)
                        tilesChanged += CreateCircleArea(mapIndex, maps, row, column, rowCount, columnCount, minRadius, tileSize, meterPerTiles, area.Id, allowEdgeOverflow, area.UseEdgeNoise);
                }
            }
        }

        private void CreateLayerOne(int[,] map, int rowCount, int columnCount, int tileSize, float meterPerTiles, AreaSpread area, bool allowEdgeManipulation)
        {
            int tilesChanged = 0;
            int tileCount = rowCount * columnCount;
            while (((float)tilesChanged) / ((float)tileCount) < area.Percentage)
            {
                int row = random.Next(0, rowCount);
                int column = random.Next(0, columnCount);
                int minRadius = random.Next(area.MinSizeInMeter, area.MaxSizeInMeter + 1);

                if(map[row, column] == 0 || map[row, column] == area.Id)
                {
                    if(area.SpreadType == SpreadOption.Circle)
                        tilesChanged += CreateCircleArea(map, row, column, rowCount, columnCount, minRadius, tileSize, meterPerTiles, area.Id, allowEdgeManipulation);
                }
            }
        }

        private int CreateCircleArea(int[,] map, int rowIndex, int columnIndex, int rowCount, int columnCount, int radius, int tileSize, float metersPerTile, int areaId, bool allowEdgeManipulation)
        {
            int modified = 0;
            int minRadiusTile = (int)Math.Ceiling(radius / metersPerTile);
            int minRadiusPx = minRadiusTile * tileSize;
            int x = columnIndex * tileSize;
            int y = rowIndex * tileSize;

            for (int r = rowIndex - minRadiusTile; r < rowIndex + minRadiusTile; r++)
            {
                for (int c = columnIndex - minRadiusTile; c < columnIndex + minRadiusTile; c++)
                {
                    int realRow = r;
                    int realColumn = c;

                    if(IsOutOfRange(r, c, rowCount, columnCount))
                    {
                        if (allowEdgeManipulation)
                        {
                            realRow = ConvertToTileIndex(r, rowCount);
                            realColumn = ConvertToTileIndex(c, columnCount);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (TryModifyField(x, y, r, c, realColumn, realRow, tileSize, minRadiusPx, map, areaId))
                        modified++;
                }
            }

            return modified;
        }

        private int CreateCircleArea(int mapIndex, int[][,] maps, int rowIndex, int columnIndex, int rowCount, int columnCount, int radius, int tileSize, float metersPerTile, int areaId, bool allowEdgeOverflow, bool useEdgeNoise)
        {
            int modified = 0;
            int minRadiusTile = (int)Math.Ceiling(radius / metersPerTile);
            int minRadiusPx = minRadiusTile * tileSize;
            int x = columnIndex * tileSize;
            int y = rowIndex * tileSize;

            for (int r = rowIndex - minRadiusTile; r < rowIndex + minRadiusTile; r++)
            {
                for (int c = columnIndex - minRadiusTile; c < columnIndex + minRadiusTile; c++)
                {
                    int currentMapIndex = mapIndex;
                    int realRow = r;
                    int realColumn = c;

                    if (IsOutOfRange(r, c, rowCount, columnCount))
                    {
                        if (allowEdgeOverflow)
                        {
                            currentMapIndex = GetMapIndex(r, c, rowCount, columnCount, currentMapIndex, maps);
                            if (currentMapIndex >= 0)
                            {
                                realRow = ConvertToTileIndex(r, rowCount);
                                realColumn = ConvertToTileIndex(c, columnCount);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (TryModifyField(x, y, r, c, realColumn, realRow, tileSize, minRadiusPx, maps[currentMapIndex], areaId))
                    {
                        if (useEdgeNoise)
                        {
                            int[] allowedDirections = CheckDirections(r, c, rowCount, columnCount, maps[currentMapIndex]);
                            modified += 1 + AddEdgeNoise(r, c, rowCount, columnCount, areaId, 1f, allowedDirections, currentMapIndex, maps);
                        }
                    }
                }
            }

            return modified;
        }

        private bool TryModifyField(int x, int y, int row, int column, int realColumn, int realRow, int tileSize, int minRadiusPx, int[,] map, int areaId)
        {
            int distance = GetDistance(x, y, column * tileSize, row * tileSize);
            if (distance <= minRadiusPx)
            {
                if (map[realRow, realColumn] == 0)
                {
                    map[realRow, realColumn] = areaId;
                    return true;
                }
            }
            return false;
        }
    
        private int ConvertToTileIndex(int index, int count)
        {
            return (index >= count ? index - ((int)Math.Floor((double)index / (double)count) * count) : (index < 0 ? (((int)Math.Floor((double)index / (double)count) * count) * -1) + index : index));
        }

        private int GetDistance(int x1, int y1, int x2, int y2)
        {
            int x = x1 - x2;
            int y = y1 - y2;
            return (int)Math.Round(Math.Sqrt((x * x) + (y * y)));
        }

        private int CalculateGridTranslation(int translationX , int translationY, int sourceX, int sourceY, int gridRowCount, int gridColumnCount)
        {
            int gridNumX = ConvertToTileIndex((sourceX - 1) + translationX, gridColumnCount) + 1;
            int gridNumY = ConvertToTileIndex((sourceY - 1) + translationY, gridRowCount) + 1;
            return gridNumX + ((gridNumY - 1) * gridColumnCount);
        }

        private int AddEdgeNoise(int row, int column, int rowCount, int columnCount, int areaId, float percentage, int[] allowedDirection, int mapIndex, int[][,] maps)
        {
            int modified = 0;
            float calculatedPercentage = random.Next(0, 100) / 100f;
            if(percentage >= calculatedPercentage)
            {
                int result = random.Next(0, allowedDirection.Length);
                if (result <= 1)
                    row += allowedDirection[result];
                else
                    column += allowedDirection[result];

                if (percentage / 2f >= (random.Next(0, 100) / 100f))
                {
                    allowedDirection[result] = 0;

                    bool goAhead = false;
                    for (int i = 0; i < allowedDirection.Length; i++)
                    {
                        if (allowedDirection[i] != 0)
                        {
                            goAhead = true;
                            break;
                        }
                    }

                    if(!goAhead)
                    {
                        return modified;
                    }
                }

                int realRow = row;
                int realColumn = column;
                int currentMapIndex = mapIndex;
                if(IsOutOfRange(row, column, rowCount, columnCount))
                {
                    currentMapIndex = GetMapIndex(row, column, rowCount, columnCount, mapIndex, maps);
                    if (currentMapIndex >= 0)
                    {
                        realRow = ConvertToTileIndex(row, rowCount);
                        realColumn = ConvertToTileIndex(column, columnCount);
                    }
                    else
                    {
                        percentage -= 0.01f;
                        return modified + AddEdgeNoise(row, column, rowCount, columnCount, areaId, percentage, allowedDirection, mapIndex, maps);
                    }
                }
               
                if(maps[currentMapIndex][realRow, realColumn] == 0)
                {
                    maps[currentMapIndex][realRow, realColumn] = areaId;
                    modified++;
                }

                percentage -= 0.01f;
                return modified + AddEdgeNoise(row, column, rowCount, columnCount, areaId, percentage, allowedDirection, mapIndex, maps);
            }

            return modified;
        }

        private int GetMapIndex(int row, int column, int rowCount, int columnCount, int mapIndex, int[][,] maps)
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

        private bool IsOutOfRange(int row, int column, int rowCount, int columnCount)
        {
            return (row < 0 || row >= rowCount || column < 0 || column >= columnCount);
        }

        private int[] CheckDirections(int row, int column, int rowCount, int columnCount, int[,] map)
        {
            int[] directions = new int[4];

            directions[0] = CheckDirection(row + 1, column, rowCount, columnCount, map);
            directions[1] = CheckDirection(row - 1, column, rowCount, columnCount, map) * -1;
            directions[2] = CheckDirection(row, column + 1, rowCount, columnCount, map);
            directions[3] = CheckDirection(row, column - 1, rowCount, columnCount, map) * -1;

            return directions;
        }

        private int[] GetSuroundings(int distance, int row, int column, int rowCount, int columnCount, int[,] map, int value)
        {
            int[] suroundings = new int[(int)Math.Pow(((distance * 2) + 1), 2)];
            int index = 0;
            bool sameAsValue = true;

            for (int r = row - distance; r < row + distance + 1; r++)
            {
                for (int c = column - distance; c < column + distance + 1; c++)
                {
                    if (IsOutOfRange(r, c, rowCount, columnCount))
                    {
                        suroundings[index++] = -1;
                    }
                    else
                    {
                        if (map[r, c] != value)
                            sameAsValue = false;

                        suroundings[index++] = map[r, c];
                    }
                }
            }

            if (sameAsValue)
                return null;
            else
                return suroundings;
        }

        private int CheckDirection(int row, int column, int columnCount, int rowCount, int[,] map)
        {
            if (IsOutOfRange(row, column, rowCount, columnCount))
            {
                return 1;
            }
            return map[row, column] == 0 ? 1 : 0;
        }

        private void DefragmentMaps(int[][,] maps)
        {
            int distance = 5;
            for (int i = 0; i < maps.Length; i++)
            {
                int rowCount = maps[i].GetUpperBound(0);
                int columnCount = maps[i].GetUpperBound(1);

                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < columnCount; c++)
                    {
                        int value = maps[i][r, c];
                        if (value != 0)
                        {
                            TryConnect(r, c, rowCount, columnCount, maps[i], value, distance, false);
                            TryConnect(r, c, rowCount, columnCount, maps[i], value, distance, true);
                        }
                    }
                }
            }
        }


        private void TryConnect(int row, int column, int rowCount, int columnCount, int[,] map, int value, int distance, bool columnCheck)
        {
            bool connectPos = false;
            bool connectNeg = false;
            for (int i = distance; i > 0; i--)
            {
                int subR = 0;
                int subC = 0;

                if (columnCheck)
                    subC = i;
                else
                    subR = i;

                if (!IsOutOfRange(row + subR, column + subC, rowCount, columnCount))
                {
                    if (!connectPos)
                    {
                        if (map[row + subR, column + subC] == value)
                        {
                            connectPos = true;
                        }
                    }
                    else
                    {
                        if (map[row + subR, column + subC] == 0)
                        {
                            map[row + subR, column + subC] = value;
                        }
                    }
                }

                if (!IsOutOfRange(row - subR, column - subC, rowCount, columnCount))
                {
                    if (!connectNeg)
                    {
                        if (map[row - subR, column - subC] == value)
                        {
                            connectNeg = true;
                        }
                    }
                    else
                    {
                        if (map[row - subR, column - subC] == 0)
                        {
                            map[row - subR, column - subC] = value;
                        }
                    }
                }
            }
        }

        private void CreateLayerTwo()
        {

        }

        private void LayerThreeZoneCreation()
        {
            
        }

        private void PopulateLayerThree()
        {

        }
    }
}
