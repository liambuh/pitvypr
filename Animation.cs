using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    internal abstract class Animation
    {
        public bool pause;
        public bool finished;
        public bool started;
        public abstract void Perform(Map map, float time);

        public static int GetFrame(int totalFrames, float runtime, float time)
        {
            //retrieves the current "frame" of animation as an integer, for Perform calculations:
            return (int)Math.Floor((totalFrames * time) / runtime);
        }

        public Animation(bool pause = false)
        {
            finished = false;
            started = false;
            this.pause = pause;
        }
    }

    class AnimMove : Animation
    {
        Point end;
        bool relative;
        float runtime;
        GameObject obj;
        Point[] points; //all cells in line towards the endPoint. Create on startup.
        public AnimMove(GameObject obj, Point end, bool relative, float runtime)
        {
            this.obj = obj;
            this.end = end;
            this.relative = relative;
            this.runtime = runtime;

            //ToDo: calculate the point array from the obj position and the endpoint:
            

            //for now, just adding start & end as the only two points in the line:
            points = new Point[] { obj.Position, end };
        }

        public override void Perform(Map map, float time)
        {
            
            if (time <= 0.0f)
            {
                //instantly perform if time <= 0.
                obj.UpdateDisplayedPosition(points[points.Length - 1], map);
                return;
            }
            if(time >= runtime)
            {
                finished = true;
                return;
            }

            //Get the current tile to be rendered on from the calculation of the "frame":
            int frame = GetFrame(points.Length, runtime, time);
            if (obj.Position != points[frame])
            {
                obj.UpdateDisplayedPosition(points[frame], map);
            }
            if(time == runtime)
            {
                finished = true;
            }
        }
    }
}
