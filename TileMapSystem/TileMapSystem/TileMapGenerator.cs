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
    /// <summary>
    /// The tile map generator can be used to generate random tile maps
    /// </summary>
    public class TileMapGenerator
    {
        private Random random;
        private int seed;
        private GeneratorSettings settings;
        private AreaSpread[] spreads;

        /// <summary>
        /// Initzializes tile map generator
        /// </summary>
        public TileMapGenerator()
        {
            random = null;
            seed = -1;
        }

        /// <summary>
        /// Initzializes tile map generator with a given seed
        /// </summary>
        /// <param name="seed">Seed for the random generator</param>
        public TileMapGenerator(int seed)
        {
            this.seed = seed;
            random = new Random(seed);
        }

        /// <summary>
        /// Generates a map based on the generator settings and area spreads
        /// </summary>
        /// <param name="settings">Generator settings to modify the generation process</param>
        /// <param name="area">Area that should be placed on the map</param>
        /// <returns>Returns the maps as a flatten byte array</returns>
        public Tile[] GenerateMap(GeneratorSettings settings, AreaSpread area)
        {
            if (settings.Seed >= 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            Tile[] map = PopulateMap(settings);
            int rowCount = map.GetUpperBound(0);
            int columnCount = map.GetUpperBound(1);

            CreateLayerOne(map, rowCount, columnCount, settings.TileSize, settings.MeterPerTile, area, settings.RadiusOfCylinder);

            return map;
        }

        /// <summary>
        /// Generates a map based on the generator settings and area spreads
        /// </summary>
        /// <param name="settings">Generator settings to modify the generation process</param>
        /// <param name="areas">Areas that should be placed on the map</param>
        /// <returns>Returns the maps as a flatten byte array</returns>
        public Tile[] GenerateMap(GeneratorSettings settings, AreaSpread[] areas)
        {
            if (settings.Seed >= 0)
                random = new Random(settings.Seed);
            else if (random == null)
                throw new ArgumentOutOfRangeException();

            Tile[] map = PopulateMap(settings);
            int rowCount = map.GetUpperBound(0);
            int columnCount = map.GetUpperBound(1);
            for (int i = 0; i < areas.Length; i++)
            {
                CreateLayerOne(map, rowCount, columnCount, settings.TileSize, settings.MeterPerTile, areas[i], settings.RadiusOfCylinder);
            }
            return map;
        }

        /// <summary>
        /// Generates a map based on the given tile location, generator settings and area spreads
        /// </summary>
        /// <param name="settings">Generator settings to modify the generation process</param>
        /// <param name="areas">Areas that should be placed on the map</param>
        /// <param name="tileColumn">Tile column index</param>
        /// <param name="tileRow">Tile row index</param>
        /// <returns>Returns a StreamedTileMap</returns>
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

            Tile[][] edgeOverrides = new Tile[9][];
            Tile[][] maps = new Tile[9][];
            for (int i = 0; i < maps.Length; i++)
            {
                edgeOverrides[i] = new Tile[size];
                maps[i] = new Tile[size];
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

        /// <summary>
        /// Generates a map based on the current tile location
        /// </summary>
        /// <param name="tileColumn">Tile column index</param>
        /// <param name="tileRow">Tile row index</param>
        /// <returns>Returns a StreamedTileMap</returns>
        public StreamedTileMap GenerateMap(int tileColumn, int tileRow)
        {
            return GenerateMap(settings, spreads, tileColumn, tileRow);
        }

        /// <summary>
        /// Calculate all surrounding grid id's
        /// </summary>
        /// <param name="gridCount">Number of grids</param>
        /// <param name="gridColumn">Grid column index</param>
        /// <param name="gridRow">Grid row index</param>
        /// <param name="gridsPerRow">Number of grids per row</param>
        /// <returns>Returns grid id's of all surrounding grids</returns>
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

        /// <summary>
        /// Create the StreamedTileMap from the generated grids
        /// </summary>
        /// <param name="maps">Current grid with all it's surrounding grids</param>
        /// <param name="suroundingGrids">Ids of all surrounding grids</param>
        /// <param name="gridsPerRow">Number grids per row</param>
        /// <param name="streamedTileMap">A reference to a streamedTileMap</param>
        private void CreateTileMapParts(Tile[][] maps, int[] suroundingGrids, int gridsPerRow, StreamedTileMap streamedTileMap)
        {
            //create TileMapPart
            for (int i = 0; i < maps.Length; i++)
            {
                int gridId = suroundingGrids[i];
               
                int gridRow;
                int gridColumn;
                TileMathHelper.ToPosition(gridId, gridsPerRow, out gridRow, out gridColumn);

                gridRow = TileMathHelper.FixTileIndex(gridRow, gridsPerRow);
                gridColumn = TileMathHelper.FixTileIndex(gridColumn, gridsPerRow);

                TileMapPart part = new TileMapPart(gridId, maps[i], null, null, gridColumn, gridRow);
                streamedTileMap.Add(part);
            }

        }

        /// <summary>
        /// Generates an empty map based on the generator settings
        /// </summary>
        /// <param name="settings">Settings which descibes the map</param>
        /// <returns>Returns an empty map</returns>
        private Tile[] PopulateMap(GeneratorSettings settings)
        {
            if (settings.RadiusOfCylinder)
            {
                int radiusInMeter = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter);

                float u = 2f * Convert.ToSingle(Math.PI) * radiusInMeter;
                int size = (int)Math.Round(u / settings.MeterPerTile);
                return new Tile[size * size];
            }
            else
            {
                int height = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter + 1);
                int width = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter + 1);
                return new Tile[height * width];
            }
        }

        /// <summary>
        /// Calculates the map size in tiles
        /// </summary>
        /// <param name="settings">Settings which descibes the map</param>
        /// <returns>Returns the map size</returns>
        private int GetMapSize(GeneratorSettings settings)
        {
            int radiusInMeter = random.Next(settings.MinSizeInMeter, settings.MaxSizeInMeter);

            float u = 2f * Convert.ToSingle(Math.PI) * radiusInMeter;
            return (int)Math.Round(u / settings.MeterPerTile);
        }

        /// <summary>
        /// Create the first (height) layer with circular areas
        /// </summary>
        /// <param name="mapIndex">Index of the map</param>
        /// <param name="maps">Current grids</param>
        /// <param name="edgeOverrides">Current edge override maps</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        /// <param name="tileSize">Size of a tile in pixel</param>
        /// <param name="meterPerTile">Size of a tile in meter</param>
        /// <param name="area">Area that should be generated and placed on the given map</param>
        /// <param name="allowEdgeOverflow">Allows areas to overlap tiles from other maps</param>
        private void CreateLayerOne(int mapIndex, Tile[][] maps, Tile[][] edgeOverrides, int rowCount, int columnCount, int tileSize, float meterPerTiles, AreaSpread area, bool allowEdgeOverflow)
        {
            int tilesChanged = 0;
            int tileCount = rowCount * columnCount;
            while (((float)tilesChanged) / ((float)tileCount) < area.Percentage)
            {
                int row = random.Next(0, rowCount);
                int column = random.Next(0, columnCount);
                int fieldIndex = TileMathHelper.ToIndex(row, column, columnCount);
                int minRadius = random.Next(area.MinSizeInMeter, area.MaxSizeInMeter + 1);

                if (maps[mapIndex][fieldIndex].Id == 0 || maps[mapIndex][fieldIndex].Id == area.Id)
                {
                    if (area.SpreadType == SpreadOption.Circle)
                        tilesChanged += CreateCircleArea(mapIndex, maps, edgeOverrides, row, column, rowCount, columnCount, minRadius, tileSize, meterPerTiles, area.Id, area.Flag, allowEdgeOverflow, area.UseEdgeNoise);
                }
            }
        }

        /// <summary>
        /// Create the first (height) layer with circular areas
        /// </summary>
        /// <param name="map">The map where the area should be placed on</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        /// <param name="tileSize">Size of a tile in pixel</param>
        /// <param name="meterPerTile">Size of a tile in meter</param>
        /// <param name="area">Area that should be generated and placed on the given map</param>
        /// <param name="allowEdgeManipulation">Allows areas to overlap tiles from other maps</param>
        private void CreateLayerOne(Tile[] map, int rowCount, int columnCount, int tileSize, float meterPerTiles, AreaSpread area, bool allowEdgeManipulation)
        {
            float tilesChanged = 0;
            float tileCount = rowCount * columnCount;
            while (tilesChanged / tileCount < area.Percentage)
            {
                int row = random.Next(0, rowCount);
                int column = random.Next(0, columnCount);
                int fieldIndex = TileMathHelper.ToIndex(row, column, columnCount);
                int minRadius = random.Next(area.MinSizeInMeter, area.MaxSizeInMeter + 1);

                if(map[fieldIndex].Id == 0 || map[fieldIndex].Id == area.Id)
                {
                    if(area.SpreadType == SpreadOption.Circle)
                        tilesChanged += CreateCircleArea(map, row, column, rowCount, columnCount, minRadius, tileSize, meterPerTiles, area.Id, area.Flag, allowEdgeManipulation);
                }
            }
        }

        /// <summary>
        /// Create a circular area to a given map
        /// </summary>
        /// <param name="map">The map where the area should be placed on</param>
        /// <param name="rowIndex">Tile row index</param>
        /// <param name="columnIndex">Tile column index</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="tileSize">Size of a tile in pixel</param>
        /// <param name="metersPerTile">Size of a tile in meter</param>
        /// <param name="tileIdValue">Tile id value for the circular area</param>
        /// <param name="tileFlag">Tile flag</param>
        /// <param name="allowEdgeManipulation">Allows areas to overlap tiles from other maps</param>
        /// <returns>Returns the number of changed tiles</returns>
        private int CreateCircleArea(Tile[] map, int rowIndex, int columnIndex, int rowCount, int columnCount, int radius, int tileSize, float metersPerTile, ushort tileIdValue, byte tileFlag, bool allowEdgeManipulation)
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
                            realRow = TileMathHelper.FixTileIndex(r, rowCount);
                            realColumn = TileMathHelper.FixTileIndex(c, columnCount);
                        }
                        else
                        {
                            continue;
                        }
                    }


                    int distance = TileMathHelper.GetDistance(x, y, c * tileSize, r * tileSize);
                    if (distance <= minRadiusPx)
                    {
                        int id = TileMathHelper.ToIndex(realRow, realColumn, columnCount);
                        if (map[id].Id == 0)
                        {
                            map[id] = new Tile(tileIdValue, tileFlag);
                            modified++;
                        }
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Create a circular area to a given map
        /// </summary>
        /// <param name="mapIndex">Index of the map</param>
        /// <param name="maps">Current grids</param>
        /// <param name="edgeOverrides">Current edge override maps</param>
        /// <param name="rowIndex">Tile row index</param>
        /// <param name="columnIndex">Tile column index</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="tileSize">Size of a tile in pixel</param>
        /// <param name="metersPerTile">Size of a tile in meter</param>
        /// <param name="tileIdValue">Tile id value for the circular area</param>
        /// <param name="tileFlags">Tile flags</param>
        /// <param name="allowEdgeOverflow">Allows areas to overlap tiles from other maps</param>
        /// <param name="useEdgeNoise">Adds edge noise to the generated area</param>
        /// <returns>Returns the number of changed tiles</returns>
        private int CreateCircleArea(int mapIndex, Tile[][] maps, Tile[][] edgeOverrides, int rowIndex, int columnIndex, int rowCount, int columnCount, int radius, int tileSize, float metersPerTile, ushort tileIdValue, byte tileFlags, bool allowEdgeOverflow, bool useEdgeNoise)
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
                                realRow = TileMathHelper.FixTileIndex(r, rowCount);
                                realColumn = TileMathHelper.FixTileIndex(c, columnCount);
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
                        int id = TileMathHelper.ToIndex(realRow, realColumn, columnCount);
                        if (mapIndex == currentMapIndex)
                        {
                            if(maps[currentMapIndex][id].Id == 0)
                            {
                                maps[currentMapIndex][id] = new Tile(tileIdValue, tileFlags);
                                modified++;
                            }
                        }
                        else
                        {
                            if (edgeOverrides[currentMapIndex][id].Id == 0)
                            {
                                edgeOverrides[currentMapIndex][id] = new Tile(tileIdValue, tileFlags);
                            }
                        }
                    }
                }
            }

            return modified;
        }

        /// <summary>
        /// Adds edge noise to next to a give tile position
        /// </summary>
        /// <param name="row">Tile row index</param>
        /// <param name="column">Tile column index</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        /// <param name="tileIdValue">Edge noise tile id value</param>
        /// <param name="tileFlags">Tile flags</param>
        /// <param name="percentage">Percentage to add edge noise</param>
        /// <param name="allowedDirection">Direction of free tiles</param>
        /// <param name="map">A Representation of a grid</param>
        private void AddEdgeNoise(int row, int column, int rowCount, int columnCount, ushort tileIdValue, byte tileFlags, float percentage, List<Direction> allowedDirection, Tile[] map)
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
                    int id = TileMathHelper.ToIndex(row, column, columnCount);
                    if (map[id].Id == 0)
                    {
                        map[id] = new Tile(tileIdValue, tileFlags);
                    }

                    percentage -= 0.01f;
                    calculatedPercentage = random.Next(0, 100) / 100f;
                }
            }
        }

        /// <summary>
        /// Checks if the surrounding tile are free or not 
        /// </summary>
        /// <param name="row">Row index of the tile</param>
        /// <param name="column">Column index of the tile</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        /// <param name="map">A representation of a grid</param>
        /// <returns>Returns all directions with free tiles</returns>
        private List<Direction> CheckDirections(int row, int column, int rowCount, int columnCount, Tile[] map)
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

        /// <summary>
        /// Checks whenther a tile position is free or out of range
        /// </summary>
        /// <param name="row">Row index of the tile</param>
        /// <param name="column">Column index of the tile</param>
        /// <param name="columnCount">Number of column per grid</param>
        /// <param name="rowCount">Number of rows per grid</param>
        /// <param name="map">A representation of a grid</param>
        /// <returns>Retruns 0 if the tile is out of range or has already a flag, otherwise 1</returns>
        private int CheckDirection(int row, int column, int columnCount, int rowCount, Tile[] map)
        {
            if (TileMathHelper.IsOutOfRange(row, column, rowCount, columnCount))
            {
                return 0;
            }
            return map[TileMathHelper.ToIndex(row, column, columnCount)].Id == 0 ? 1 : 0;
        }

        /// <summary>
        /// Deframgment maps and adds edge noise
        /// </summary>
        /// <param name="maps">Current gird and it's surrounding grids</param>
        /// <param name="areas">Area spread information to add edge noise if neccessary</param>
        /// <param name="suroundingGrids">Grid ids of all surrounding grids</param>
        /// <param name="rowCount">Number of row per grid</param>
        /// <param name="columnCount">Number of columns per grid</param>
        private void DefragmentMaps(Tile[][] maps, AreaSpread[] areas, int[] suroundingGrids, int rowCount, int columnCount)
        {
            for (int i = 0; i < maps.Length; i++)
            {
                random = new Random(suroundingGrids[i] * settings.Seed);

                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < columnCount; c++)
                    {
                        ushort id = maps[i][TileMathHelper.ToIndex(r, c, columnCount)].Id;
                        if (id != 0)
                        {
                            for (int j = 0; j < areas.Length; j++)
                            {
                                if (id == areas[j].Id)
                                {
                                    //EdgeNoise
                                    if (areas[j].UseEdgeNoise)
                                    {
                                        List<Direction> allowedDirections = CheckDirections(r, c, rowCount, columnCount, maps[i]);
                                        AddEdgeNoise(r, c, rowCount, columnCount, areas[j].Id, areas[j].Flag, 0.3f, allowedDirections, maps[i]);
                                    }

                                    if (areas[j].ConnectEqualFlags)
                                    {
                                        TryConnect(r, c, rowCount, columnCount, maps[i], id, areas[j].Flag, areas[j].ConnectDistance, DirectionAxis.Vertical);
                                        TryConnect(r, c, rowCount, columnCount, maps[i], id, areas[j].Flag, areas[j].ConnectDistance, DirectionAxis.Horizontal);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to connects close areas with the same flag
        /// </summary>
        /// <param name="row">Tile row index</param>
        /// <param name="column">Tile column index</param>
        /// <param name="rowCount">Grid row count</param>
        /// <param name="columnCount">Grid column count</param>
        /// <param name="map">Current map as flatten array</param>
        /// <param name="tileIdValue">Tile id value</param>
        /// <param name="tileFlags">Tile flags</param>
        /// <param name="distance">Maximum tile distance between areas</param>
        /// <param name="axis">Direction of connection</param>
        private void TryConnect(int row, int column, int rowCount, int columnCount, Tile[] map, ushort tileIdValue, byte tileFlags, int distance, DirectionAxis axis)
        {
            bool connectPos = false;
            bool connectNeg = false;

            int realRow = row;
            int realColumn = column;
            for (int i = distance; i > 0; i--)
            {
                if (axis == DirectionAxis.Horizontal)
                    realColumn = column + i;
                else
                    realRow = row + i;

                if (!TileMathHelper.IsOutOfRange(realRow, realColumn, rowCount, columnCount))
                {
                    int id = TileMathHelper.ToIndex(realRow, realColumn, columnCount);
                    if (!connectPos)
                    {
                        if (map[id].Id == tileIdValue)
                        {
                            connectPos = true;
                        }
                    }
                    else
                    {
                        if (map[id].Id == 0)
                        {
                            map[id] = new Tile(tileIdValue, tileFlags);
                        }
                    }
                }

                if (axis == DirectionAxis.Horizontal)
                    realColumn = column - i;
                else
                    realRow = row - i;
                if (!TileMathHelper.IsOutOfRange(realRow, realColumn, rowCount, columnCount))
                {
                    int id = TileMathHelper.ToIndex(realRow, realColumn, columnCount);
                    if (!connectNeg)
                    {
                        if (map[id].Id == tileIdValue)
                        {
                            connectNeg = true;
                        }
                    }
                    else
                    {
                        if (map[id].Id == 0)
                        {
                            map[id] = new Tile(tileIdValue, tileFlags);
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
