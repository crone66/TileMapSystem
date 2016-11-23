using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public struct AreaSpread
    {
        public byte Flag;
        public float Percentage;
        public float Temperature;
        public int MinSizeInMeter;
        public int MaxSizeInMeter;
        public bool UseEdgeNoise;
        public SpreadOption SpreadType;
        public LayerType Layer;
        public bool ConnectEqualFlags;
        public int ConnectDistance;

        public AreaSpread(byte flag, float percentage, float temperature, int minSizeInMeter, int maxSizeInMeter, bool useEdgeNoise, bool connectEqualFlags, int connectDistance, SpreadOption spreadType, LayerType layer)
        {
            Flag = flag;
            Percentage = percentage;
            Temperature = temperature;
            MinSizeInMeter = minSizeInMeter;
            MaxSizeInMeter = maxSizeInMeter;
            UseEdgeNoise = useEdgeNoise;
            SpreadType = spreadType;
            Layer = layer;
            ConnectEqualFlags = connectEqualFlags;
            ConnectDistance = connectDistance;
        }
    }

    public enum SpreadOption
    {
        Line,
        Circle,
        Rectangle
    }

    public enum LayerType
    {
        Height = 1,
        Biome = 2,
        PointsOfInterest = 3,
        Paths = 4
    }
}
