/* 
 * Purpose: Generates tilemap or partial tilemap
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace TileMapSystem
{
    public class TileMapGenerator
    {
        private Random random;
        private int seed;
        private GeneratorSettings settings;
        private AreaSpread[] spreads;

        public TileMapGenerator()
        {

        }

        public TileMapGenerator(int seed)
        {
            this.seed = seed;
            random = new Random(seed);
        }

        public byte[] GenerateMap(GeneratorSettings settings, AreaSpread area)
        {
            if (settings.Seed >= 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            byte[] map = PopulateMap(settings);
            int rowCount = map.GetUpperBound(0);
            int columnCount = map.GetUpperBound(1);

            CreateLayerOne(map, rowCount, columnCount, settings.TileSize, settings.MeterPerTile, area, settings.RadiusOfCylinder);

            return map;
        }

        public byte[] GenerateMap(GeneratorSettings settings, AreaSpread[] areas)
        {
            if (settings.Seed >= 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            byte[] map = PopulateMap(settings);
            int rowCount = map.GetUpperBound(0);
            int columnCount = map.GetUpperBound(1);
            for (int i = 0; i < areas.Length; i++)
            {
                CreateLayerOne(map, rowCount, columnCount, settings.TileSize, settings.MeterPerTile, areas[i], settings.RadiusOfCylinder);
            }
            return map;
        }

        public StreamedTileMap GenerateMap(GeneratorSettings settings, AreaSpread[] areas, int tileColumn, int tileRow)
        {
            if (settings.Seed > 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            this.settings = settings;
            spreads = areas;

            double tilesPerGrid = Math.Round(settings.MeterPerGrid / settings.MeterPerTile);
            int tilesPerColumn = (int)tilesPerGrid;
            int size = tilesPerColumn * tilesPerColumn;
            int gridsPerRow = (int)Math.Ceiling(GetMapSize(settings) / tilesPerGrid);

            int gridColumn = (int)Math.Floor(tileColumn / tilesPerGrid);
            int gridRow = (int)Math.Floor(tileRow / tilesPerGrid);

            byte[][] edgeOverrides = new byte[9][];
            byte[][] maps = new byte[9][];
            for (int i = 0; i < maps.Length; i++)
            {
                edgeOverrides[i] = new byte[size];
                maps[i] = new byte[size];
            }

            int[] suroundingGrids = CreateSuroundings(maps.Length, gridColumn, gridRow, gridsPerRow);

            StreamedTileMap streamedTileMap = new StreamedTileMap(this, tileRow, tileColumn, gridsPerRow, gridsPerRow, tilesPerColumn, tilesPerColumn, settings.TileSize);
            //Create Map
            foreach(LayerType currentLayerType in Enum.GetValues(typeof(LayerType)).Cast<LayerType>().OrderBy(k => (int)k))
            {
                for (int i = 0; i < suroundingGrids.Length; i++)
                {
                    AreaSpread[] layerAreas = areas.Where(a => a.Layer == currentLayerType).ToArray();
                    random = new Random(suroundingGrids[i] * settings.Seed);
                    for (int j = 0; j < layerAreas.Length; j++)
                    {
                        switch (layerAreas[j].Layer)
                        {
                            case LayerType.Height:
                                {
                                    if (layerAreas[j].MaxSizeInMeter <= settings.MeterPerGrid / 2f)
                                    {
                                        CreateLayerOne(i, maps, edgeOverrides, (int)tilesPerGrid, (int)tilesPerGrid, settings.TileSize, settings.MeterPerTile, layerAreas[j], settings.RadiusOfCylinder);
                                    }
                                    else
                                    {
                                        throw new Exception("AreaSpread MaxSize must be smaller then (MetersPerGrid / 2)!");
                                    }
                                    break;
                                }
                            case LayerType.Biome:
                                CreateLayerTwo();
                                break;
                            case LayerType.Paths:
                                CreateLayerFour();
                                break;
                            case LayerType.PointsOfInterest:
                                CreateLayerThree();
                                break;
                        }
                    }
                }
            }

            //DefragementMap and add edgenoise
            DefragmentMaps(maps, areas, suroundingGrids, (int)tilesPerGrid, (int)tilesPerGrid);

            //Merge edgeOverrides
            for (int k = 0; k < maps.Length; k++)
            {
                maps[k] = TileMathHelper.MergeMaps(maps[k], edgeOverrides[k]);
            }

            CreateTileMapParts(maps, suroundingGrids, gridsPerRow, streamedTileMap);

            return streamedTileMap;
        }

        public StreamedTileMap GenerateMap(int tileLocationX, int tileLocationY)
        {
            return GenerateMap(settings, spreads, tileLocationX, tileLocationY);
        }

        private int[] CreateSuroundings(int gridCount, int gridColumn, int gridRow, int gridsPerRow)
        {
            int[] suroundingGrids = new int[gridCount];
            //Line 1
            suroundingGrids[0] = TileMathHelper.CalculateGridTranslation(-1, -1, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            suroundingGrids[1] = TileMathHelper.CalculateGridTranslation(0, -1, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            suroundingGrids[2] = TileMathHelper.CalculateGridTranslation(1, -1, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            //Line 2
            suroundingGrids[3] = TileMathHelper.CalculateGridTranslation(-1, 0, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            suroundingGrids[4] = TileMathHelper.CalculateGridTranslation(0, 0, gridColumn, gridRow, gridsPerRow, gridsPerRow); //Center
            suroundingGrids[5] = TileMathHelper.CalculateGridTranslation(1, 0, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            //Line 3
            suroundingGrids[6] = TileMathHelper.CalculateGridTranslation(-1, 1, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            suroundingGrids[7] = TileMathHelper.CalculateGridTranslation(0, 1, gridColumn, gridRow, gridsPerRow, gridsPerRow);
            suroundingGrids[8] = TileMathHelper.CalculateGridTranslation(1, 1, gridColumn, gridRow, gridsPerRow, gridsPerRow);

            return suroundingGrids;
        }

        private void CreateTileMapParts(byte[][] maps, int[] suroundingGrids, int gridsPerRow, StreamedTileMap streamedTileMap)
        {
            //create TileMapPart
            for (int i = 0; i < maps.Length; i++)
            {
                int gridId = suroundingGrids[i];
                int gridRow = (int)Math.Floor((double)gridId / (double)gridsPerRow);
                int gridColumn = (gridId % gridsPerRow);

                gridRow = TileMathHelper.ConvertToTileIndex(gridRow, gridsPerRow);
                gridColumn = TileMathHelper.ConvertToTileIndex(gridColumn, gridsPerRow);

                TileMapPart part = new TileMapPart(gridId, maps[i], null, null, gridColumn, gridRow);
                streamedTileMap.Add(part);
            }

        }

        private byte[] PopulateMap(GeneratorSettings settings)
        {
            if (settings.RadiusOfCylinder)
            {
                int radiusInMeter = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter);

                float u = 2f * Convert.ToSingle(Math.PI) * radiusInMeter;
                int size = (int)Math.Round(u / settings.MeterPerTile);
                return new byte[size * size];
            }
            else
            {
                int height = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter + 1);
                int width = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter + 1);
                return new byte[height * width];
            }
        }

        private int GetMapSize(GeneratorSettings settings)
        {
            int radiusInMeter = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter);

            float u = 2f * Convert.ToSingle(Math.PI) * radiusInMeter;
            return (int)Math.Round(u / settings.MeterPerTile);
        }

        private void CreateLayerOne(int mapIndex, byte[][] maps, byte[][] edgeOverrides, int rowCount, int columnCount, int tileSize, float meterPerTiles, AreaSpread area, bool allowEdgeOverflow)
        {
            int tilesChanged = 0;
            int tileCount = rowCount * columnCount;
            while (((float)tilesChanged) / ((float)tileCount) < area.Percentage)
            {
                int row = random.Next(0, rowCount);
                int column = random.Next(0, columnCount);
                int fieldIndex = TileMathHelper.ToId(row, column, columnCount);
                int minRadius = random.Next(area.MinSizeInMeter, area.MaxSizeInMeter + 1);

                if (maps[mapIndex][fieldIndex] == 0 || maps[mapIndex][fieldIndex] == area.Flag)
                {
                    if (area.SpreadType == SpreadOption.Circle)
                        tilesChanged += CreateCircleArea(mapIndex, maps, edgeOverrides, row, column, rowCount, columnCount, minRadius, tileSize, meterPerTiles, area.Flag, allowEdgeOverflow, area.UseEdgeNoise);
                }
            }
        }

        private void CreateLayerOne(byte[] map, int rowCount, int columnCount, int tileSize, float meterPerTiles, AreaSpread area, bool allowEdgeManipulation)
        {
            float tilesChanged = 0;
            float tileCount = rowCount * columnCount;
            while (tilesChanged / tileCount < area.Percentage)
            {
                int row = random.Next(0, rowCount);
                int column = random.Next(0, columnCount);
                int fieldIndex = TileMathHelper.ToId(row, column, columnCount);
                int minRadius = random.Next(area.MinSizeInMeter, area.MaxSizeInMeter + 1);

                if(map[fieldIndex] == 0 || map[fieldIndex] == area.Flag)
                {
                    if(area.SpreadType == SpreadOption.Circle)
                        tilesChanged += CreateCircleArea(map, row, column, rowCount, columnCount, minRadius, tileSize, meterPerTiles, area.Flag, allowEdgeManipulation);
                }
            }
        }

        private int CreateCircleArea(byte[] map, int rowIndex, int columnIndex, int rowCount, int columnCount, int radius, int tileSize, float metersPerTile, byte flag, bool allowEdgeManipulation)
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

                    if(TileMathHelper.IsOutOfRange(r, c, rowCount, columnCount))
                    {
                        if (allowEdgeManipulation)
                        {
                            realRow = TileMathHelper.ConvertToTileIndex(r, rowCount);
                            realColumn = TileMathHelper.ConvertToTileIndex(c, columnCount);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    int distance = TileMathHelper.GetDistance(x, y, c * tileSize, r * tileSize);
                    if (distance <= minRadiusPx)
                    {
                        int id = TileMathHelper.ToId(realRow, realColumn, columnCount);
                        if (map[id] == 0)
                        {
                            map[id] = flag;
                            modified++;
                        }
                    }
                }
            }

            return modified;
        }

        private int CreateCircleArea(int mapIndex, byte[][] maps, byte[][] edgeOverrides, int rowIndex, int columnIndex, int rowCount, int columnCount, int radius, int tileSize, float metersPerTile, byte flag, bool allowEdgeOverflow, bool useEdgeNoise)
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

                    if (TileMathHelper.IsOutOfRange(r, c, rowCount, columnCount))
                    {
                        if (allowEdgeOverflow)
                        {
                            currentMapIndex = TileMathHelper.GetMapIndex(r, c, rowCount, columnCount, currentMapIndex);
                            if (currentMapIndex >= 0)
                            {
                                realRow = TileMathHelper.ConvertToTileIndex(r, rowCount);
                                realColumn = TileMathHelper.ConvertToTileIndex(c, columnCount);
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

                    int distance = TileMathHelper.GetDistance(x, y, c * tileSize, r * tileSize);
                    if (distance <= minRadiusPx)
                    {
                        int id = TileMathHelper.ToId(realRow, realColumn, columnCount);
                        if (mapIndex == currentMapIndex)
                        {
                            if(maps[currentMapIndex][id] == 0)
                            {
                                maps[currentMapIndex][id] = flag;
                                modified++;
                            }
                        }
                        else
                        {
                            if (edgeOverrides[currentMapIndex][id] == 0)
                            {
                                edgeOverrides[currentMapIndex][id] = flag;
                            }
                        }
                    }
                }
            }

            return modified;
        }

        private void AddEdgeNoise(int row, int column, int rowCount, int columnCount, byte flag, float percentage, List<Direction> allowedDirection, byte[] map)
        {
            if (allowedDirection.Count > 0)
            {
                float calculatedPercentage = random.Next(0, 100) / 100f;
                while (allowedDirection.Count > 0 && percentage >= calculatedPercentage)
                {
                    //replace allowedDirections
                    int result = random.Next(0, allowedDirection.Count);
                    if (allowedDirection[result].Axis == DirectionAxis.Horizontal)
                    {
                        column += allowedDirection[result].Value;
                    }
                    else
                    {
                        row += allowedDirection[result].Value;
                    }

                    if (TileMathHelper.IsOutOfRange(row, column, rowCount, columnCount))
                        return;

                    allowedDirection = CheckDirections(row, column, rowCount, columnCount, map);
                    int id = TileMathHelper.ToId(row, column, columnCount);
                    if (map[id] == 0)
                    {
                        map[id] = flag;
                    }

                    percentage -= 0.01f;
                    calculatedPercentage = random.Next(0, 100) / 100f;
                }
            }
        }

        private List<Direction> CheckDirections(int row, int column, int rowCount, int columnCount, byte[] map)
        {
            List<Direction> directions = new List<Direction>();
            int yPos = CheckDirection(row + 1, column, rowCount, columnCount, map);
            int yNeg = CheckDirection(row - 1, column, rowCount, columnCount, map) * -1;
            int xPos = CheckDirection(row, column + 1, rowCount, columnCount, map);
            int xNeg = CheckDirection(row, column - 1, rowCount, columnCount, map) * -1;

            if (yPos != 0)
                directions.Add(new Direction(DirectionAxis.Vertical, yPos));
            if (yNeg != 0)
                directions.Add(new Direction(DirectionAxis.Vertical, yNeg));
            if (xPos != 0)
                directions.Add(new Direction(DirectionAxis.Horizontal, xPos));
            if (xNeg != 0)
                directions.Add(new Direction(DirectionAxis.Horizontal, xNeg));

            return directions;
        }

        private int CheckDirection(int row, int column, int columnCount, int rowCount, byte[] map)
        {
            if (TileMathHelper.IsOutOfRange(row, column, rowCount, columnCount))
            {
                return 0;
            }
            return map[TileMathHelper.ToId(row, column, columnCount)] == 0 ? 1 : 0;
        }

        private void DefragmentMaps(byte[][] maps, AreaSpread[] areas, int[] suroundingGrids, int rowCount, int columnCount)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                random = new Random(suroundingGrids[i] * settings.Seed);

                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < columnCount; c++)
                    {
                        byte flag = maps[i][TileMathHelper.ToId(r, c, columnCount)];
                        if (flag != 0)
                        {
                            for (int j = 0; j < areas.Length; j++)
                            {
                                if (flag == areas[j].Flag)
                                {
                                    //EdgeNoise
                                    if (areas[j].UseEdgeNoise)
                                    {
                                        List<Direction> allowedDirections = CheckDirections(r, c, rowCount, columnCount, maps[i]);
                                        AddEdgeNoise(r, c, rowCount, columnCount, areas[j].Flag, 0.3f, allowedDirections, maps[i]);
                                    }

                                    if (areas[j].ConnectEqualFlags)
                                    {
                                        TryConnect(r, c, rowCount, columnCount, maps[i], flag, areas[j].ConnectDistance, false);
                                        TryConnect(r, c, rowCount, columnCount, maps[i], flag, areas[j].ConnectDistance, true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TryConnect(int row, int column, int rowCount, int columnCount, byte[] map, byte flag, int distance, bool columnCheck)
        {
            bool connectPos = false;
            bool connectNeg = false;

            int realRow = row;
            int realColumn = column;
            for (int i = distance; i > 0; i--)
            {
                if (columnCheck)
                    realColumn = column + i;
                else
                    realRow = row + i;

                if (!TileMathHelper.IsOutOfRange(realRow, realColumn, rowCount, columnCount))
                {
                    int id = TileMathHelper.ToId(realRow, realColumn, columnCount);
                    if (!connectPos)
                    {
                        if (map[id] == flag)
                        {
                            connectPos = true;
                        }
                    }
                    else
                    {
                        if (map[id] == 0)
                        {
                            map[id] = flag;
                        }
                    }
                }

                if (columnCheck)
                    realColumn = column - i;
                else
                    realRow = row - i;
                if (!TileMathHelper.IsOutOfRange(realRow, realColumn, rowCount, columnCount))
                {
                    int id = TileMathHelper.ToId(realRow, realColumn, columnCount);
                    if (!connectNeg)
                    {
                        if (map[id] == flag)
                        {
                            connectNeg = true;
                        }
                    }
                    else
                    {
                        if (map[id] == 0)
                        {
                            map[id] = flag;
                        }
                    }
                }
            }
        }
        private void CreateLayerTwo()
        {

        }

        private void CreateLayerThree()
        {
            
        }

        private void CreateLayerFour()
        {

        }

        private void PopulateLayerThree()
        {

        }
    }
}
