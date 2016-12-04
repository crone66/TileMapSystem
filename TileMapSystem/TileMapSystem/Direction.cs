using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    /// <summary>
    /// Holds tile shift information
    /// </summary>
    public struct Direction
    {
        public int Value;
        public DirectionAxis Axis;

        /// <summary>
        /// Holds tile shift information
        /// </summary>
        /// <param name="axis">direction shift</param>
        /// <param name="value">tile shift</param>
        public Direction(DirectionAxis axis, int value)
        {
            Axis = axis;
            Value = value;
        }
    }

    /// <summary>
    /// Direction enumeration
    /// </summary>
    public enum DirectionAxis
    {
        Horizontal,
        Vertical
    }
}
