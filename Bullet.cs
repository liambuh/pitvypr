using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    internal class Bullet : Actor
    {
        int speed; //-1 = hitscan bullet.
        bool homing;
        Point target;

        private void MoveUntilSpeed(Map m, List<Point> path, int speed)
        {
            if(path.Count == 0) return;

            Point first = new Point(Position.X, Position.Y);
            for (int i = 0; i < speed; i++)
            {
                if (speed < path.Count)
                {
                    //speed is less than path's length, will need to offset target for next path once it has finished:

                    //perform a move to the next cell/tile in the path:
                    if(TryMove(path[i], m))
                    {
                        //if we hit something, break:
                        return;
                    }

                    if (i == speed-1)
                    {
                        //offset Target by distance travelled on path:
                        target = target.Add(path[path.Count - 1].Subtract(path[0]));

                        return;
                    }
                }
                else
                {
                    //speed is larger than the path size, either more empty space to travel through, or hit something:
                    if (i == path.Count)
                    {
                        //create new path ahead, by offsetting the old target:
                        target = target.Add(Position.Subtract(first));

                        //refresh path and continue going, nested:
                        List<Point> extendedPath = FOVFinder.Raycast(Position, target, m, false);
                        if (extendedPath != null) { if (extendedPath.Count != 0) { extendedPath.RemoveAt(0); } }
                        MoveUntilSpeed(m, extendedPath, speed - i);

                        //break out of for loop, since travel will be completed within the nest:
                        return;
                    }
                    else
                    {
                        //if still within path size, just continue along path:
                        if(TryMove(path[i], m))
                        {
                            //if we hit something, break.
                            return;
                        }
                    }
                }
            }
        }

        private bool TryMove(Point p, Map m)
        {
            if (Position.Equals(p)) return false;

            bool hit = m.TryGetMapObject(p, out var go);
            (new ActionMove(p, animSpeed: 20.0f)).Perform(m, this);
            
            if(hit)
            {
                //if wasn't out of bounds attempt:
                if (m.SurfaceObject.Surface.IsValidCell(p.X, p.Y))
                {
                    //hit object, we need to run hit-functionality code:
                    int a = 0;
                }
                else
                {
                    //out of bounds, delete object:
                    //this.Destroy(m);
                }
            }
            return hit;
        }

        public Bullet(Point position, IScreenSurface hostingSurface, Point target, int speed, bool homing = false)
        : base(new ColoredGlyph(Color.White, Color.Transparent, '-'), position, hostingSurface)
        {
            this.target = target;
            this.speed = speed;
            this.homing = homing;
        }

        public override Action TakeTurn(Map m)
        {
            
            //get a straight path from position to target.
            //Note: Raycast only generates a path until blocked by a solid object.
            List<Point> path = FOVFinder.Raycast(Position, target, m, false);

            if (path != null) { if (path.Count != 0) { path.RemoveAt(0); } }
            if (speed > 0)
            {
                //Moves through N cells, N = speed;
                MoveUntilSpeed(m, path, speed);
            }
            else
            {
                //hitscan turn performance, just move in direction until Object is hit, done by giving very large speed
                MoveUntilSpeed(m, path, 100);
            }

            return null;
        }
    }
}
