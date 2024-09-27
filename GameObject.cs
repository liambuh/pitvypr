namespace SadConsoleGame;

internal abstract class GameObject
{
    public Point Position { get; private set; }
    public Point DisplayedPosition { get; private set; }
    public ColoredGlyph Appearance { get; set; }
    private ColoredGlyph _mapAppearance = new ColoredGlyph();
    private bool isActor;
    private bool isPlayer;

    private bool visible;
    private bool explored;
    private Color col;
    private int standardGlyph;

    public bool visibilityChanged;
    public GameObject(ColoredGlyph appearance, Point position, IScreenSurface hostingSurface, bool isActor = false, bool isPlayer = false)
    {
        Appearance = appearance;
        Position = position;
        DisplayedPosition = position;

        visible = false;
        explored = false;
        col = appearance.Foreground;
        standardGlyph = appearance.Glyph;
        Appearance = new ColoredGlyph(Color.Transparent, Color.Transparent, 0);
        visibilityChanged = true;

        hostingSurface.Surface[position].CopyAppearanceTo(_mapAppearance);

        DrawGameObject(hostingSurface);
        this.isActor = isActor;
        this.isPlayer = isPlayer;
    }

    public void Destroy(Map m)
    {
        //remove object from designated gameobject data structure
        

        //remove object from display
        _mapAppearance.CopyAppearanceTo(m.SurfaceObject.Surface[Position]);
    }

    public void SetVisible(bool visible)
    {
        if(this.visible != visible) { visibilityChanged = true; }
        if(visible) { this.explored = true; }
        this.visible = visible;
        UpdateAppearance();
    }

    public bool IsVisible()
    {
        return visible;
    }

    public void UpdateAppearance()
    {
        if(visible)
        {
            Appearance = new ColoredGlyph(col, Color.Transparent, standardGlyph);
        }
        else
        {
            if(explored)
            {
                Appearance = new ColoredGlyph(Color.DarkGreen, Color.Transparent, standardGlyph);
            }
            else
            {
                Appearance = new ColoredGlyph(Color.Transparent, Color.Transparent, 0);
            }
        }
    }

    public bool IsActor()
    {
        return isActor;
    }

    public bool IsPlayer()
    {
        return isPlayer;
    }

    public virtual bool Touched(GameObject source, Map map)
    {
        return false;
    }

    public void RestoreMap(Map map) =>
        _mapAppearance.CopyAppearanceTo(map.SurfaceObject.Surface[DisplayedPosition]);

    public void RefreshRender(Map m)
    {
        DrawGameObject(m.SurfaceObject);
    }

    public void UpdateGamePosition(Point p, Map m)
    {
        Position = p;
    }

    public void UpdateDisplayedPosition(Point p, Map m)
    {
        // Restore the old cell
        _mapAppearance.CopyAppearanceTo(m.SurfaceObject.Surface[DisplayedPosition]);

        // Store the map cell of the new position
        m.SurfaceObject.Surface[p].CopyAppearanceTo(_mapAppearance);

        // Update Displayed Position:
        DisplayedPosition = p;

        // Draw to Displayed Cell:
        DrawGameObject(m.SurfaceObject);
    }

    public void UpdateDisplayedPosition()
    {
        DisplayedPosition = Position;
    }

    private void DrawGameObject(IScreenSurface screenSurface)
    {
        //Draw appearance to displayed position on surface:
        Appearance.CopyAppearanceTo(screenSurface.Surface[DisplayedPosition]);
        screenSurface.IsDirty = true;
    }
}