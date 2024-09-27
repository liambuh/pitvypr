namespace SadConsoleGame;

internal class Wall : GameObject
{

    public Wall(Point position, IScreenSurface hostingSurface)
        : base(new ColoredGlyph(Color.White, Color.Black, '#'), position, hostingSurface)
    {
        //need to set Appearance to a wall tile, not just #. Given some kind of parameters in creation
    }

    public override bool Touched(GameObject source, Map map)
    {
        return base.Touched(source, map);
    }
}