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

        public GeneratorSettings(int seed, int tileSize, float meterPerTile, int minSizeInMeter, int maxSizeInMeter, bool radiusOfSphere, float meterPerGrid)
        {
            Seed = seed;
            TileSize = tileSize;
            MeterPerTile = meterPerTile;
            MinSizeInMeter = minSizeInMeter;
            MaxSizeInMeter = maxSizeInMeter;
            RadiusOfCylinder = radiusOfSphere;
            MeterPerGrid = meterPerGrid;
        }
    }
}
