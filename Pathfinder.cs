using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    internal class Pathfinder
    {
        public Pathfinder() { }

        public static int GetDjikstraDistance(Point a, int bx, int by)
        {
            return Math.Min(Math.Abs(a.X - bx), Math.Abs(a.Y - by));
        }

        public static int GetDjikstraDistance(Point a, Point b)
        {
            return Math.Min(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
        }

        public static float[,] CreateDjikstraMap(Map m, List<(GameObject, int)> sources)
        {
            //creates a Djikstra Map using GameObjects as sources.
            float[,] map = new float[m.width, m.height];
            if (sources != null)
            {
                if (sources.Count > 0)
                {
                    NewDjikstraMap(map, sources[0].Item1.Position, sources[0].Item2);
                    for (int i = 1; i < sources.Count; ++i)
                    {
                        AddSource(map, sources[i].Item1.Position, sources[i].Item2);
                    }
                }
            }

            return map;
        }

        public static void NewDjikstraMap(float[,] map, Point source, float sourceVal)
        {
            //overwriting function for djikstra map
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = GetDjikstraDistance(source, x, y) + sourceVal;
                }
            }
        }

        public static void AddSource(float[,] map, Point source, float sourceVal)
        {
            //additive function for djikstra map
            for(int x = 0; x < map.GetLength(0); x++)
            {
                for(int y = 0;  y < map.GetLength(1); y++)
                {
                    map[x, y] += GetDjikstraDistance(source, x, y) + sourceVal;
                }
            }
        }

        public static void MultiplyMap(float[,] map, float value)
        {
            //multiply all cells in map:
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] *= value;
                }
            }
        }

        public static float[,] CombineDjikstraMaps(float[,] m1, float[,] m2)
        {
            float[,] newMap = new float[m1.GetLength(0), m1.GetLength(1)];
            for(int x = 0;x < newMap.GetLength(0);x++)
            {
                for( int y = 0;y < newMap.GetLength(1);y++)
                {
                    newMap[x,y] = m1[x,y] + m2[x,y];
                }
            }
            return newMap;
        }

        public static void AddStaticImpasses(float[,] map, GameObject[,] statics)
        {
            //Sets tiles which contain static objects to Positive Infinity, our "solid" value
            //A great benefit of using positive infinity with our tiles is that even if we add a source value to that, it remains = Positive Infinity.
            //We only need to be careful for if it becomes negative through multiplication, so we must always check for a wall with the float.IsInfinity(value) function
            for(int x = 0; x < map.GetLength(0); x++)
            {
                for(int y = 0; y < map.GetLength(1); y++)
                {
                    if (statics[x,y] != null)
                    {
                        //Assuming all statics are solid.
                        map[x, y] = float.PositiveInfinity;
                    }
                }
            }
        }

        public static Point GetDjikstraLowest(float[,] map, int x, int y)
        {
            //Get lowest number in surrounding 8 tiles:
            Point north = new Point(x, y - 1);
            Point south = new Point(x, y + 1);
            Point east = new Point(x + 1, y);
            Point west = new Point(x - 1, y);
            Point northeast = new Point(x + 1, y - 1);
            Point northwest = new Point(x - 1, y - 1);
            Point southeast = new Point(x + 1, y + 1);
            Point southwest = new Point(x - 1, y + 1);
            Point[] points = { north, south, east, west, northeast, northwest, southeast, southwest };

            float val = map[x, y];
            Point point = new Point(x, y);

            foreach (Point p in points)
            {
                if(p.X < 0 || p.Y < 0 || p.X >= map.GetLength(0) || p.Y >= map.GetLength(1))
                {
                    continue;
                }
                if(val >= map[p.X, p.Y])
                {
                    val = map[p.X, p.Y];
                    point = p;
                }
            }

            return point;
        }

        public static Point[] GetDjikstraPath(float[,] map, Point a, Point b)
        {
            List<Point> pathList = new List<Point>();
            pathList.Add(a);
            int x = a.X;
            int y = a.Y;
            while (true)
            {
                if(x != b.X && y != b.Y)
                {
                    pathList.Add(GetDjikstraLowest(map, x, y));
                }
                else
                {
                    break;
                }
            }
            pathList.Add(b);
            return pathList.ToArray();
        }

    }
}
