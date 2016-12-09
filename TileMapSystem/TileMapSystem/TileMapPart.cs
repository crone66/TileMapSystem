/* 
 * Purpose: Reprensentation of a grid
 * Author: Marcel Croonenbroeck
 * Date: 04.12.2016
 */
namespace TileMapSystem
{
    /// <summary>
    /// Reprensentation of a grid
    /// </summary>
    public struct TileMapPart
    {
        public Tile[] MapSurface;
        public byte[] PathPlacement;
        public byte[] ObjectPlacement;
        public int GridColumn;
        public int GridRow;
        public int Id;

        /// <summary>
        /// Reprensentation of a grid
        /// </summary>
        /// <param name="id">Grid id</param>
        /// <param name="mapSurface">Height information of the grid</param>
        /// <param name="pathPlactment">Path information of the grid</param>
        /// <param name="objectPlacement">Object placement information of the grid</param>
        /// <param name="gridColumn">grid column index</param>
        /// <param name="gridRow">grid row index</param>
        public TileMapPart(int id, Tile[] mapSurface, byte[] pathPlactment, byte[] objectPlacement, int gridColumn, int gridRow)
        {
            Id = id;
            MapSurface = mapSurface;
            PathPlacement = pathPlactment;
            ObjectPlacement = objectPlacement;
            GridColumn = gridColumn;
            GridRow = gridRow;
        }
    }
}
