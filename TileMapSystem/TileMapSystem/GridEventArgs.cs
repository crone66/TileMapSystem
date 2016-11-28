using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileMapSystem
{
    public class GridEventArgs : EventArgs
    {
        public int NewGridRow;
        public int NewGridColumn;
        public int OldGridRow;
        public int OldGridColumn;
        public bool IsRecycledMap;

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
