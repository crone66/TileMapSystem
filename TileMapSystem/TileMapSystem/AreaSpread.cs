using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    /// <summary>
    /// 
    /// </summary>
    public struct AreaSpread
    {
        public ushort Id;
        public byte Flag;
        public float Percentage;
        public int MinSizeInMeter;
        public int MaxSizeInMeter;
        public bool UseEdgeNoise;
        public SpreadOption SpreadType;
        public LayerType Layer;
        public bool ConnectEqualFlags;
        public int ConnectDistance;

        /// <summary>
        /// AreaSpread describes areas that should be placed on the map
        /// </summary>
        /// <param name="id">Mapped tile id</param>
        /// <param name="flag">Tile flags</param>
        /// <param name="percentage">Percentage of map space covered by this flag</param>
        /// <param name="minSizeInMeter">Minimum size of area in meter</param>
        /// <param name="maxSizeInMeter">Maximum size of area in meter</param>
        /// <param name="useEdgeNoise">Can be used to add aliasing to avoid symmetric areas</param>
        /// <param name="connectEqualFlags">Areas with the same flags will be connected, all free tiles within a give distance will be set to the flag value</param>
        /// <param name="connectDistance">Maximum distance between two areas with the same flag</param>
        /// <param name="spreadType">Geometrical form of the area</param>
        /// <param name="layer">Dicides which generation algorithm will be used</param>
        public AreaSpread(ushort id, byte flag, float percentage, int minSizeInMeter, int maxSizeInMeter, bool useEdgeNoise, bool connectEqualFlags, int connectDistance, SpreadOption spreadType, LayerType layer)
        {
            Id = id;
            Flag = flag;
            Percentage = percentage;
            MinSizeInMeter = minSizeInMeter;
            MaxSizeInMeter = maxSizeInMeter;
            UseEdgeNoise = useEdgeNoise;
            SpreadType = spreadType;
            Layer = layer;
            ConnectEqualFlags = connectEqualFlags;
            ConnectDistance = connectDistance;
        }
    }

    /// <summary>
    /// Enumaration of spread options to choose the form of the Spread
    /// </summary>
    public enum SpreadOption
    {
        None,
        Line,
        Circle,
        Rectangle
    }

    /// <summary>
    /// Enumeration of layer types to choose the generation algorithm
    /// </summary>
    public enum LayerType
    {
        Height = 1,
        Biome = 2,
        PointsOfInterest = 3,
        Paths = 4
    }
}
