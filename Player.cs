
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole.Input;
using SadConsole.Instructions;

namespace SadConsoleGame
{
    internal class Player : Actor
    {
        private bool isWaitingForInput;
        private Keyboard keyboard;
        private Point target;
        public ActionEconomy actionEconomy;
        public Player(ColoredGlyph appearance, Point position, IScreenSurface hostingSurface) : base(appearance, position, hostingSurface, true)
        {
        }

        public override Action TakeTurn(Map m)
        {
            return new ActionMove(new Point(Position.X, Position.Y));
        }

        public void SetTarget(Point target)
        {
            this.target = target;
        }

        public Point GetTarget()
        {
            return this.target;
        }

        public void SetKeyboardState(Keyboard keyboard)
        {
            this.keyboard = keyboard;
        }

        public void SetInputFlag(bool val)
        {
            isWaitingForInput = val;
        }

        public bool IsWaitingForInput()
        {
            return isWaitingForInput;
        }
    }
}
