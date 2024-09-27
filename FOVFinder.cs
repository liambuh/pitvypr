using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    internal class FOVFinder
    {
        public FOVFinder() { }

        public static List<Point> Raycast(Point start, Point end, Map m, bool setVisible = true, bool useDisplayedPos = false)
        {
            List<Point> points = new List<Point>();

            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            // Bresenham's line algorithm
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                points.Add(new Point(x0, y0));

                GameObject obj = null;
                // Check if there's a static object at the current point
                if (m.TryGetMapObject(new Point(x0, y0), out obj, useDisplayedPos))
                {
                    //if we are setting Visibility in this function call, set it now:
                    if (setVisible)
                    {
                        obj.SetVisible(true);
                    }

                    //if this check is not being performed on the origin point:
                    if (!(x0 == start.X && y0 == start.Y))
                    {
                        break; // Stop the ray if it hits a wall or static object
                    }
                }

                // If the start point has reached the end point, break
                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return points;
        }

        public static List<Point> RaycastInCircle(Point origin, int radius, Map map, bool setVisible = true, bool useDisplayedPos = false)
        {
            HashSet<Point> visiblePoints = new HashSet<Point>();

            // Get the perimeter points
            List<Point> perimeterPoints = GetCirclePerimeterPoints(origin, radius);

            foreach (Point perimeterPoint in perimeterPoints)
            {
                // Cast a ray from the perimeter point to the origin
                List<Point> ray = Raycast(origin, perimeterPoint, map, setVisible, useDisplayedPos);

                foreach (Point p in ray)
                {
                    visiblePoints.Add(p);  // Add each point in the ray to the visible points set
                }
            }

            return new List<Point>(visiblePoints);
        }

        public static List<Point> RaycastInRings(Point origin, int radius, Map map, bool setVisible = true, bool useDisplayedPos = false)
        {
            HashSet<Point> visiblePoints = new HashSet<Point>();

            //Get perimeter points
            List<Point> perimeterPoints = GetSquareRadiusPoints(origin, radius);

            foreach (Point perimeterPoint in perimeterPoints)
            {
                // Cast a ray from the perimeter point to the origin
                List<Point> ray = Raycast(origin, perimeterPoint, map, setVisible, useDisplayedPos);

                foreach (Point p in ray)
                {
                    visiblePoints.Add(p);  // Add each point in the ray to the visible points set
                }
            }

            return new List<Point>(visiblePoints);
        }

        private static List<Point> GetSquareRadiusPoints(Point origin, int radius)
        {
            List<Point> points = new List<Point>();

            if(radius > 0)
            {
                for (int x = origin.X - radius; x <= origin.X + radius; x++)
                {
                    points.Add(new Point(x, origin.Y - radius));
                    points.Add(new Point(x, origin.Y + radius));
                }

                for (int y = (origin.Y - radius) + 1; y <= (origin.Y + radius) - 1; y++)
                {
                    points.Add(new Point(origin.X - radius, y));
                    points.Add(new Point(origin.X + radius, y));
                }
            }
            else
            {
                points.Add(origin);
            }

            return points;
        }

        private static List<Point> GetCirclePerimeterPoints(Point origin, int radius)
        {
            List<Point> perimeterPoints = new List<Point>();
            int ox = origin.X;
            int oy = origin.Y;

            // Using Bresenham's circle algorithm to get perimeter points
            int x = radius;
            int y = 0;
            int err = 0;

            while (x >= y)
            {
                AddCirclePoints(ox, oy, x, y, perimeterPoints);
                y += 1;
                if (err <= 0)
                {
                    err += 2 * y + 1;
                }
                else
                {
                    x -= 1;
                    err += 2 * (y - x + 1);
                }
            }

            return perimeterPoints;
        }

        private static void AddCirclePoints(int ox, int oy, int x, int y, List<Point> points)
        {
            points.Add(new Point(ox + x, oy + y));
            points.Add(new Point(ox + y, oy + x));
            points.Add(new Point(ox - y, oy + x));
            points.Add(new Point(ox - x, oy + y));
            points.Add(new Point(ox - x, oy - y));
            points.Add(new Point(ox - y, oy - x));
            points.Add(new Point(ox + y, oy - x));
            points.Add(new Point(ox + x, oy - y));
        }

        /*
        private bool TestIntersect(Vector2 entry, Vector2 exit, Point p, Map m)
        {
            GameObject obj = null;
            bool foundObject = m.TryGetMapObject(p, out obj);
            if (obj == null)
            {
                //empty space, return true:
                return true;
            }
            else
            {
                if(obj.IsStatic())
                {
                    //wall:

                    //get sloped sides:
                    bool[] sideWall = [false, false, false, false];
                    Point[] sides = [new Point(p.X - 1, p.Y), new Point(p.X + 1, p.Y), new Point(p.X, p.Y - 1), new Point(p.X, p.Y + 1)];
                    int trueCount = 0;
                    for(int i = 0; i < sides.Length; i++)
                    {
                        sideWall[i] = m.TryGetMapObject(sides[i], out var sideObj);
                        if (sideWall[i])
                        {
                            trueCount++;
                        }
                    }

                    if(trueCount > 0)
                    {
                        //Not Diamond:

                        //Full square, intersection guaranteed:
                        if(trueCount == 4)
                        {
                            return true;
                        }
                        else
                        {
                            //Line intersections using slopes to make shape:
                            List<Vector2> points = new List<Vector2>();
                            if(sideWall[0])
                            {
                                //left side is closed:
                                points.Add(new Vector2(-0.5f, -0.5f));
                                points.Add(new Vector2(-0.5f, 0.5f));
                            }
                            else
                            {
                                if (!sideWall[2])
                                {
                                    points.Add(new Vector2(-0.5f, 0.0f));
                                    points.Add(new Vector2(0.0f, -0.5f));
                                }
                            }
                            if(sideWall[1])
                            {
                                //right side is closed:
                                points.Add(new Vector2(0.5f, -0.5f));
                                points.Add(new Vector2(0.5f, 0.5f));
                            }
                        }
                    }
                    else
                    {
                        //Diamond, use rotated bb to determine intersection:
                        
                    }

                }
                else
                {
                    //dynamic objects. area 0.5 in center of tile.
                    Vector2 diff = exit - entry;
                    Vector2 empty = new Vector2(1 - 0.5f, 1 - 0.5f);
                    if (diff.X >= empty.X/2 || diff.Y >= empty.Y/2)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }
        */
    }
}
