/* 
 * Purpose: Settings for generator and map areas
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */

namespace TileMapSystem
{
    public struct GeneratorSettings
    {
        public int Seed;
        public int TileSize;
        public float MeterPerTile;
        public int MinSizeInMeter;
        public int MaxSizeInMeter;
        public bool RadiusOfCylinder;
        public float MeterPerGrid;
        public LayerDepth Depth;

        public GeneratorSettings(int seed, int tileSize, float meterPerTile, int minSizeInMeter, int maxSizeInMeter, bool radiusOfSphere, float meterPerGrid, LayerDepth depth)
        {
            Seed = seed;
            TileSize = tileSize;
            MeterPerTile = meterPerTile;
            MinSizeInMeter = minSizeInMeter;
            MaxSizeInMeter = maxSizeInMeter;
            RadiusOfCylinder = radiusOfSphere;
            MeterPerGrid = meterPerGrid;
            Depth = depth;
        }
    }

    public struct AreaSpread
    {
        public int Id;
        public float Percentage;
        public float Temperature;
        public int MinSizeInMeter;
        public int MaxSizeInMeter;
        public bool UseEdgeNoise;
        public SpreadOption SpreadType;

        public AreaSpread(int id, float percentage, float temperature, int minSizeInMeter, int maxSizeInMeter, bool useEdgeNoise, SpreadOption spreadType)
        {
            Id = id;
            Percentage = percentage;
            Temperature = temperature;
            MinSizeInMeter = minSizeInMeter;
            MaxSizeInMeter = maxSizeInMeter;
            UseEdgeNoise = useEdgeNoise;
            SpreadType = spreadType;
        }
    }

    public enum SpreadOption
    {
        Line,
        Circle,
        Rectangle
    }

    public enum LayerDepth
    {
        One,
        Two,
        Three,
    }
}
