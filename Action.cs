using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    class ActionResult
    {
        public bool success;

        public ActionResult(bool success)
        {
            this.success = success;
        }

        public static ActionResult Success()
        {
            return new ActionResult(true);
        }

        public static ActionResult Failure()
        {
            return new ActionResult(false);
        }
    }

    internal abstract class Action
    {
        public abstract ActionResult Perform(Map map, GameObject obj);


    }

    class ActionMove : Action
    {
        Point newPosition;
        float animSpeed;
        public override ActionResult Perform(Map map, GameObject obj)
        {
            // Check new position is valid
            if (!map.SurfaceObject.Surface.IsValidCell(newPosition.X, newPosition.Y)) return ActionResult.Failure();

            // Check if other object is there
            if (map.TryGetMapObject(newPosition, out GameObject? foundObject))
            {
                // We touched the other object, but they won't allow us to move into the space
                if (!foundObject.Touched(obj, map))
                    return ActionResult.Failure();
            }

            //Update position in the logical/literal engine.
            obj.UpdateGamePosition(newPosition, map);

            //Setting an animation for the object to move on the screen:
            if(obj.IsVisible())
            {
                //enqueue animation if visible:
                map._animationQueue.Enqueue(new AnimMove(obj, newPosition, false, animSpeed));
            }
            else
            {
                //set animation time to 0.0f if not visible, to avoid needing to wait.
                AnimMove anim = new AnimMove(obj, newPosition, false, 0.0f);

                //perform immediately for now.
                anim.Perform(map, 0.0f);

                //testing performance when instant move anime is queued:
                //map._animationQueue.Enqueue(anim);
            }


            return ActionResult.Success();
        }

        public ActionMove(Point position, float animSpeed = 100.0f)
        {
            newPosition = position;
            this.animSpeed = animSpeed;
        }
    }

    class ActionShoot : Action
    {
        //Item gun;
        Point target;

        public override ActionResult Perform(Map map, GameObject obj)
        {
            //shoot gun:
            //gun's ammo count--;
            //perform any gun-specific heating management or whatever here, too.
            //bullet is created and added to the map's static objects
            //move animation is created and queued for the bullet based on its movement speed.
            //most bullets will have instantaneous bulletspeeds, though. So, their animations will need to instead be a particle effect of lines.
            //first: test line-of-sight between bullet & palyer, for drawing & animation purposes:
            bool visible = false;
            List<Point> ray = FOVFinder.Raycast(obj.Position, map._player.Position, map, false);
            if (ray.Last<Point>().Equals(map._player.Position)) visible = true;

            Bullet b = (Bullet) map.CreateObject(new Bullet(obj.Position, map.SurfaceObject, target, -1), parent: obj);
            b.SetVisible(visible);

            b.TakeTurn(map);
            return ActionResult.Success();
        }

        public ActionShoot(Point target)
        {
            this.target = target;
        }
    }
}
