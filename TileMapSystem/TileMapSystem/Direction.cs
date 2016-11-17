using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public struct Direction
    {
        public int Value;
        public DirectionAxis Axis;

        public Direction(DirectionAxis axis, int value)
        {
            Axis = axis;
            Value = value;
        }
    }

    public enum DirectionAxis
    {
        Horizontal,
        Vertical
    }
}
