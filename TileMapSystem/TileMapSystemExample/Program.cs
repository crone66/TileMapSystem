using System;
using System.Collections.Generic;
using System.Text;
using TileMapSystem;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace TileMapMangerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            double sum = 0;
            double sum2 = 0;
            double sum3 = 0;
            long inUse = 0;
            int iterations = 1;
            int updateIterations = 1;
            int startX = -627;
            int startY = -1;
            int endX = -627;
            int endY = 1;
            TileMapManager tileMapManager;

            for (int b = 0; b < iterations; b++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                GeneratorSettings settings = new GeneratorSettings(1, 50, 1.5f, 1000000, 10000000, true, 1000f);
                AreaSpread[] spreads = new AreaSpread[2] { new AreaSpread(1, 1, 0.30f, 20, 250, true, true, 5, SpreadOption.Circle, LayerType.Height), new AreaSpread(2, 1, 0.125f, 20, 200, true, true, 5, SpreadOption.Circle, LayerType.Height) };

                tileMapManager = new TileMapManager(settings, spreads);
                tileMapManager.Changelevel(settings, startX, startY, false);
                StreamedTileMap map = tileMapManager.CurrentLevel;
                watch.Stop();
                double seconds = watch.Elapsed.TotalSeconds;

                tileMapManager.Update(startY, startX);
                Tile[] screenMap = map.GetTileMapInScreen(800, 600);
                SaveToFile(screenMap, "screenMap"+b.ToString(), 800 / 50);


                watch.Restart();
                int counter = 0;
                for (int k = 0; k < 9; k++)
                {
                    counter += SaveToFile(map.Maps[k].MapSurface, "map" + b.ToString() + "_" + k.ToString(), map.TileColumnCount);
                }

                watch.Stop();

                watch.Restart();
                tileMapManager.Update(endY, endX);
                watch.Stop();
                double update1 = watch.Elapsed.TotalSeconds;
                Thread.Sleep(5000); //wait for generation thread (only for debugging)


                watch.Restart();
                tileMapManager.Update(endY, endX);
                watch.Stop();
                double update2 = watch.Elapsed.TotalSeconds;

                for (int k = 0; k < 9; k++)
                {
                    counter += SaveToFile(map.Maps[k].MapSurface, "map2_" + b.ToString() + "_" + k.ToString(), map.TileColumnCount);
                }

                Log("Tiles generated: " + counter.ToString());
                Log("Generation time: " + seconds.ToString() + " seconds");
                Log("Write time: " + watch.Elapsed.TotalSeconds.ToString() + " seconds");
                Log("Update1 time: " + update1.ToString() + " seconds");
                Log("Update2 time: " + update2.ToString() + " seconds");

                screenMap = map.GetTileMapInScreen(800, 600);
                SaveToFile(screenMap, "screenMap2_" + b.ToString(), 800 / 50);

                for (int i = 0; i < updateIterations; i++)
                {
                    watch.Restart();
                    tileMapManager.Update(endY - i, endX - i);
                    Tile[] data = map.GetTileMapInScreen(800, 600);
                    watch.Stop();
                    update2 = watch.Elapsed.TotalMilliseconds;
                    sum += update2;
                    Log("Update time: " + update2.ToString() + " ms");
                    inUse += Process.GetCurrentProcess().PrivateMemorySize64;
                    Log("MemoryInUse: " + Process.GetCurrentProcess().PrivateMemorySize64.ToString() + " byte");
                }

                watch.Restart();
                tileMapManager.Update(startY, startX);
                watch.Stop();
                update2 = watch.Elapsed.TotalMilliseconds;
                sum2 += update2;
                Log("Update time: " + update2.ToString() + " ms");
                Thread.Sleep(2000);
                watch.Restart();
                tileMapManager.Update(startY, startX);
                watch.Stop();
                update2 = watch.Elapsed.TotalMilliseconds;
                sum3 += update2;
                Log("Update time: " + update2.ToString() + " ms");
                Log("Done");

            }
            Console.Clear();
            Log("Iterations: " + iterations.ToString());
            Log("UpdateIterations: " + (iterations * updateIterations).ToString());
            Log("Avg Update Time: " + (sum / (iterations * updateIterations)).ToString() + " ms");
            Log("Avg Update Time (Pre GridChange):" + (sum2 / iterations).ToString() + " ms");
            Log("Avg Update Time (GridChange):" + (sum3 / iterations).ToString() + " ms");
            Log("Avg MemoryInUse: " +(inUse / (iterations * updateIterations)).ToString() + " byte");
            Log("Final MemoryInUse: " + Process.GetCurrentProcess().PrivateMemorySize64.ToString() + " byte");

            MatchFiles();
            Console.ReadKey();
        }

        private static void MatchFiles()
        {
            string[] files = Directory.GetFiles(@"C:\Users\marce\Source\Repos\TileMapSystem\TileMapSystem\TileMapSystemExample\bin\Debug", "map*");
            string[] maps = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                maps[i] = File.ReadAllText(files[i]);
            }

            for (int i = 0; i < maps.Length; i++)
            {
                int matchCount = 0;
                for (int j = i + 1; j < maps.Length; j++)
                {
                    int mapLength = maps[i].Length;
                    matchCount = 0;
                    for (int l = 0; l < mapLength; l++)
                    {
                        if (maps[i][l] == maps[j][l])
                            matchCount++;
                    }
                    Console.WriteLine((i+6==j ? "(Match)" : "")+  "Map " + i.ToString() + " To " + j.ToString() + ": " + (((double)matchCount / (double)mapLength) * 100).ToString());
                }
            }
        }

        private static int SaveToFile(Tile[] map, string fileName, int columnCount)
        {
            int counter = -1;
            List<string> lines = new List<string>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < map.Length; i++)
            {
                int mod = (i % columnCount);
                if (mod == 0)
                {
                    counter++;
                    if (sb.Length > 0)
                    {
                        lines.Add(sb.ToString());
                        sb.Clear();
                    }
                }

                sb.Append(Convert.ToInt32(map[(counter * columnCount) + mod].Id.ToString()));
            }

            System.IO.File.WriteAllLines(fileName + ".txt", lines.ToArray());
            return map.Length;
        }

        private static string path = "log.txt";
        private static void Log(string text)
        {
            Console.WriteLine(text);
            System.IO.File.AppendAllText(path, text + Environment.NewLine);
        }
    }
}
