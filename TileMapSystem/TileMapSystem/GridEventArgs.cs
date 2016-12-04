using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    /// <summary>
    /// Event args for grid events
    /// </summary>
    public class GridEventArgs : EventArgs
    {
        public int NewGridRow;
        public int NewGridColumn;
        public int OldGridRow;
        public int OldGridColumn;
        public bool IsRecycledMap;

        /// <summary>
        /// Event args for grid events
        /// </summary>
        /// <param name="newGridRow">New grid row index</param>
        /// <param name="newGridColumn">New grid column index</param>
        /// <param name="oldGridRow">Old grid row index</param>
        /// <param name="oldGridColumn">Old grid column index</param>
        /// <param name="isRecycledMap">Map is recycled when the new map was already loaded</param>
        public GridEventArgs(int newGridRow, int newGridColumn, int oldGridRow, int oldGridColumn, bool isRecycledMap)
        {
            NewGridColumn = newGridColumn;
            NewGridRow = newGridRow;
            OldGridColumn = oldGridColumn;
            OldGridRow = oldGridRow;
            IsRecycledMap = isRecycledMap;
        }
    }
}
