namespace SadConsoleGame;

internal class Door : GameObject
{
    private bool open;
    public Door(Point position, IScreenSurface hostingSurface)
        : base(new ColoredGlyph(Color.Gray, Color.Black, '/'), position, hostingSurface)
    {
        open = false;
    }

    public bool IsOpen()
    {
        return open;
    }

    public void Open()
    {
        open = true;
    }

    public void Close()
    {
        open = false;
    }

    public override bool Touched(GameObject source, Map map)
    {
        //return base.Touched(source, map);
        if (open)
            return true;
        else
            return false;
    }
}