/* 
 * Purpose: Can be used for huge tilemaps that doesnt fit into memory. (Loads and manages mapparts)
 * Author: Marcel Croonenbroeck
 * Date: 13.11.2016
 */
using System.Collections.Generic;

namespace TileMapSystem
{
    public class StreamedTileMap : TileMap
    {
        private List<TileMapPart> maps;
        private int GridRow;
        private int GridColumn;

        public List<TileMapPart> Maps
        {
            get
            {
                return maps;
            }
        }

        public StreamedTileMap(int SpawnTileRow, int SpawnTileColumn)
        {
            maps = new List<TileMapPart>();
            //calc gridrow and column
        }

        public void Add(TileMapPart mapPart)
        {
            maps.Add(mapPart);
        }

        public void Update(int currentTileRow, int currentTileColumn, int screenWidth, int screenHeight)
        {
            //Calculate Grid
            //if Grid changed
                //Load, Shrink and resize (threaded?)
        }

        public void Draw()
        {

            //if close to another grid
                //Draw including other grids
            //else
                //draw
        }

        public void Resize(int x, int y, int widht, int height)
        {

        }

        private void Shrink()
        {
            
        }
    }

    public struct TileMapPart
    {
        public int[,] MapSurface;
        public int[,] PathPlacement;
        public int[,] ObjectPlacement;
        public int GridColumn;
        public int GridRow;

        public TileMapPart(int[,] mapSurface, int[,] pathPlactment, int[,] objectPlacement, int gridColumn, int gridRow)
        {
            MapSurface = mapSurface;
            PathPlacement = pathPlactment;
            ObjectPlacement = objectPlacement;
            GridColumn = gridColumn;
            GridRow = gridRow;
        }
    }
}
