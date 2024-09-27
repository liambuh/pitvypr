using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    internal abstract class Actor : GameObject
    {
        protected Actor(ColoredGlyph appearance, Point position, IScreenSurface hostingSurface, bool isPlayer = false) : base(appearance, position, hostingSurface, true, isPlayer)
        {
        }

        public abstract Action TakeTurn(Map m);
    }
}
