using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SadConsoleGame
{
    internal abstract class MenuOption
    {
        public string Label;
        public abstract bool Proceed(Menu Parent);
    }

    internal class MainActionOption : MenuOption
    {
        
        public override bool Proceed(Menu Parent)
        {
            //Performs Action set to main hand.
            //returns true iff action is executed on entry.
            //else (e.g aiming with a target cursor) returns false.
            
            //first, executing the melee actions:

            return true;
        }
    }
}
