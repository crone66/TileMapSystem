/* 
 * Purpose: Settings for generator and map areas
 * Author: Marcel Croonenbroeck
 * Date: 12.11.2016
 */

namespace TileMapSystem
{
    /// <summary>
    /// Settings to affect the generation process
    /// </summary>
    public struct GeneratorSettings
    {
        public int Seed;
        public int TileSize;
        public float MeterPerTile;
        public int MinSizeInMeter;
        public int MaxSizeInMeter;
        public bool RadiusOfCylinder;
        public float MeterPerGrid;

        /// <summary>
        /// Settings to affect the generation process
        /// </summary>
        /// <param name="seed">Seed for the random generator</param>
        /// <param name="tileSize">Tile size in pixel</param>
        /// <param name="meterPerTile">Tile size in meter</param>
        /// <param name="minSizeInMeter">Minimum map size in meter</param>
        /// <param name="maxSizeInMeter">Maximum map size in meter</param>
        /// <param name="radiusOfCylinder">affects the size calculation</param>
        /// <param name="meterPerGrid">Size of grids in meter</param>
        public GeneratorSettings(int seed, int tileSize, float meterPerTile, int minSizeInMeter, int maxSizeInMeter, bool radiusOfCylinder, float meterPerGrid)
        {
            Seed = seed;
            TileSize = tileSize;
            MeterPerTile = meterPerTile;
            MinSizeInMeter = minSizeInMeter;
            MaxSizeInMeter = maxSizeInMeter;
            RadiusOfCylinder = radiusOfCylinder;
            MeterPerGrid = meterPerGrid;
        }
    }
}
