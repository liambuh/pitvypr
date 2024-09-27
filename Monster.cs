namespace SadConsoleGame;

internal class Monster : Actor
{

    public Monster(Point position, IScreenSurface hostingSurface)
        : base(new ColoredGlyph(Color.Red, Color.Black, 'M'), position, hostingSurface)
    {

    }

    public override Action TakeTurn(Map m)
    {
        //very VERY simple, for now:
        //move in random direction:

        Random random = new Random();
        int dir = random.Next(0, 4);
        int x = this.Position.X;
        int y = this.Position.Y;

        if(dir == 1)
        {
            x += 1;
        }
        else if(dir == 2)
        {
            x -= 1;
        }
        else if (dir == 3)
        {
            y += 1;
        }
        else if (dir == 4)
        {
            y -= 1;
        }
        Point point = new Point(x, y);
        return new ActionMove(point);
    }

    public override bool Touched(GameObject source, Map map)
    {
        return base.Touched(source, map);
    }
}