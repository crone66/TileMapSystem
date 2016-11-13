using System;
using System.Collections.Generic;
using System.Text;
using TileMapSystem;
using System.Diagnostics;

namespace TileMapMangerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            TileMapGenerator generator = new TileMapGenerator();
            StreamedTileMap map = generator.GenerateMap(new GeneratorSettings(1, 10, 1.5f, 1000000, 10000000, true, 1000f, LayerDepth.One),
                new AreaSpread[2] { new AreaSpread(1, 0.30f, 0, 20, 250, true, SpreadOption.Circle), new AreaSpread(2, 0.125f, 0, 20, 200, true, SpreadOption.Circle) }, 1, 1);
            watch.Stop();
            double seconds = watch.Elapsed.TotalSeconds;

            watch.Restart();
            int counter = 0;
            for (int k = 0; k < 9; k++)
            {
                int row = map.Maps[k].MapSurface.GetUpperBound(1);
                int col = map.Maps[k].MapSurface.GetUpperBound(1);
                counter += row * col;
                List<string> lines = new List<string>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        sb.Append(map.Maps[k].MapSurface[i, j]);
                    }
                    lines.Add(sb.ToString());
                    sb.Clear();
                }

                System.IO.File.WriteAllLines("map"+k.ToString()+".txt", lines.ToArray());
            }
            watch.Stop();
            Console.WriteLine("Tiles generated: " + counter.ToString());
            Console.WriteLine("Generation time: " + seconds.ToString() + " seconds");
            Console.WriteLine("Write time: " + watch.Elapsed.TotalSeconds.ToString() + " seconds");
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
