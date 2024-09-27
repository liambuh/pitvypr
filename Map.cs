using SadConsoleGame.Scenes;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using MapRenderer;

namespace SadConsoleGame;

internal class Map
{
    private List<GameObject> _dynamicObjects;
    private ScreenSurface _mapSurface;

    private Color cTransparent = new Color(0, 0, 0, 0);

    public IReadOnlyList<GameObject> GameObjects => _dynamicObjects.AsReadOnly();
    public GameObject[,] _staticObjects;
    public ScreenSurface SurfaceObject => _mapSurface;
    public Player _player { get; set; }
    public int width { get; private set; }
    public int height { get; private set; }
    private bool isReady;

    public Queue<Animation> _animationQueue;

    public Map(int mapWidth, int mapHeight)
    {
        isReady = false;
        _dynamicObjects = new List<GameObject>();
        _staticObjects = new GameObject[mapWidth, mapHeight];

        _mapSurface = new ScreenSurface(mapWidth, mapHeight);
        _mapSurface.UseMouse = false;

        _animationQueue = new Queue<Animation>();

        width = mapWidth;
        height = mapHeight;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                _staticObjects[x, y] = null;
            }
        }

        //FillBackground();

        TunnelingGenerator tGen = new TunnelingGenerator(50);
        GenerateMap(tGen);

        _player = new Player(new ColoredGlyph(Color.White, Color.Black, 64),
            new Point(40, 30), _mapSurface);
        _dynamicObjects.Add(_player);

        for (int i = 0; i < 5; i++)
        {
            CreateTreasure();
            CreateMonster();
        }
        
        isReady = true;
    }

    public GameObject CreateObject(GameObject o, bool dynamic = true, GameObject parent = null)
    {
        if(dynamic)
        {
            if(parent == null)
            {
                _dynamicObjects.Add(o);
            }
            else
            {
                for(int i = 0; i < _dynamicObjects.Count; i++)
                {
                    if(_dynamicObjects[i].Equals(parent))
                    {
                        _dynamicObjects.Insert(i+1, o);
                        break;
                    }
                }
            }
            return o;
        }
        else
        {
            _staticObjects[o.Position.X, o.Position.Y] = o;
            return o;
        }
    }


    public void UpdateVisibility()
    {
        //calculate visible/non-visible objects using FOV algorithm
        FOVAlogirthm();

        //re-draw all objects with visibility changes:
        foreach(GameObject obj in _dynamicObjects)
        {
            if (obj != null)
            {
                if (obj.visibilityChanged)
                {
                    obj.RefreshRender(this);
                    obj.visibilityChanged = false;
                }
            }
                
        }
        foreach (GameObject obj in _staticObjects)
        {
            if(obj != null)
            {
                if (obj.visibilityChanged)
                {
                    obj.RefreshRender(this);
                    obj.visibilityChanged = false;
                }
            }
        }
    }

    public void FOVAlogirthm()
    {
        //set FOVs of all objects.
        
        //set visibility to false for all objects:
        foreach(GameObject obj in _dynamicObjects)
        {
            if(obj != null)
            {
                bool current = obj.IsVisible();
                obj.SetVisible(false);
            }
                
        }
        foreach (GameObject obj in _staticObjects)
        {
            if (obj != null)
            {
                bool current = obj.IsVisible();
                obj.SetVisible(false);
            }
        }

        //calling it on smaller concentric ring to try and catch outliers.
        //may need to call more times if this still happens
        List<Point> points = FOVFinder.RaycastInRings(_player.Position, 10, this, useDisplayedPos: true);
        points = FOVFinder.RaycastInRings(_player.Position, 9, this, useDisplayedPos: true);

    }

    private void GenerateMap(TunnelingGenerator gen)
    {
        int w = width;
        int h = height;
        int rmax = 20;
        int rmin = 8;
        int rcount = 15;
        int tmax = 0;
        int tmin = 0;
        double tchance = 0;

        int[,] mapInt = gen.CreateMap(w, h, rmax, rmin, rcount, tmax, tmin, tchance);

        //process ints into actual placed map:
        for(int x = 0; x < w; x++)
        {
            for(int y = 0; y < h; y++)
            {
                if (mapInt[x,y] == 1)
                {
                    CreateWall(new Point(x, y));
                }
            }
        }
    }

    public bool IsReady()
    {
        return isReady;
    }

    public List<GameObject> GetDynamicObjects()
    {
        return _dynamicObjects;
    }

    public void TakeTurns()
    {
        foreach(GameObject obj in _dynamicObjects)
        {
            if(obj != null)
            {
                if(obj.IsActor())
                {
                    Action action = ((Actor) obj).TakeTurn(this);
                    action.Perform(this, obj);
                }
            }
        }
    }

    public void TakeTurn(int index, RootScreen root)
    {
        if (index >= 0 && index < _dynamicObjects.Count)
        {
            GameObject obj = _dynamicObjects[index];
            if (obj != null)
            {
                if (obj.IsActor())
                {
                    if(obj.IsPlayer())
                    {
                        root.isPlayerTurn = true;
                        return;
                    }
                    else
                    {
                        Action action = ((Actor)obj).TakeTurn(this);
                        if(action != null)
                        {
                            action.Perform(this, obj);
                        }
                        
                    }
                }
            }
        }
    }

    public void CreateRoom(Rectangle rectangle, ScreenSurface surface, int[] borderStyle, Color foregroundColor, Color backgroundColor,
                           Direction doorSide, int doorDistance)
    {
        //for each piece of the wall, place a Wall object:
        for(int x = rectangle.X; x < rectangle.X + rectangle.Width - 1; x++)
        {
            CreateWall(new Point(x, rectangle.Y));
            CreateWall(new Point(x, rectangle.Y + rectangle.Height - 1));
        }
        for(int y = rectangle.Y; y < rectangle.Y + rectangle.Height - 1; y++)
        {
            CreateWall(new Point(rectangle.X, y));
            CreateWall(new Point(rectangle.X + rectangle.Width - 1, y));
        }

        CreateWall(new Point(rectangle.X + rectangle.Width - 1, rectangle.Y + rectangle.Height - 1));

        //Then, afterwards, draw the box ontop of it:
        surface.Surface.DrawBox(rectangle, ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin,
                                                    new ColoredGlyph(foregroundColor, backgroundColor)));

        //Adding Doors/Exits:
        //  Doors and Exits are noted by the follow Value set:
        //  [Side, Distance, Size]
        //  Side is noted by a Point, for the sake of readability: Direction.Up,Down,Left,Right.
        //    These correspond to the North, South, West, and East walls.
        //    Distance is the Door/Exit's distance from one end of the wall.
        //      The leftmost wall for the Horizonal Walls (North & South)
        //      The highest  wall for the Vertical  Walls (West & East)

        //  For now, we wont add in door Size, and just assume the Door is a size of 1:
        
        int doorX = rectangle.X;
        int doorY = rectangle.Y;
        if (doorSide == Direction.Left)
        {
            doorY += doorDistance;
        }
            
        if (doorSide == Direction.Right)
        {
            doorY += doorDistance;
            doorX += rectangle.Width - 1;
        }

        if (doorSide == Direction.Up)
        {
            doorX += doorDistance;
        }

        if (doorSide == Direction.Down)
        {
            doorX += doorDistance;
            doorY += rectangle.Height - 1;
        }
        Point doorPoint = new Point(doorX,doorY);

        if (TryGetMapObject(doorPoint, out GameObject? foundObject))
        {
            RemoveMapObject(foundObject);
        }

        CreateDoor(doorPoint);
    }

    bool ValidCoord(int x, int y)
    {
        if(x < 0 || x >= width || y < 0 || y >= width)
        {
            return false;
        }
        return true;
    }

    bool ValidCoord(Point p)
    {
        if (p.X < 0 || p.X >= width || p.Y < 0 || p.Y >= width)
        {
            return false;
        }
        return true;
    }

    public bool TryGetStaticObject(Point position, [NotNullWhen(true)] out GameObject? gameObject)
    {
        if (ValidCoord(position))
        {
            if (_staticObjects[position.X,position.Y] != null)
            {
                gameObject = _staticObjects[position.X, position.Y];
                return true;
            }
        }

        gameObject = null;
        return false;
    }

    public bool TryGetDynamicObject(Point position, [NotNullWhen(true)] out GameObject? gameObject, bool useDisplayedPos = false)
    {
        foreach (var otherGameObject in _dynamicObjects)
        {
            //displayed position
            if(useDisplayedPos && otherGameObject.DisplayedPosition == position)
            {
                gameObject = otherGameObject;
                return true;
            }

            //true position
            if (otherGameObject.Position == position)
            {
                gameObject = otherGameObject;
                return true;
            }
        }

        gameObject = null;
        return false;
    }

    public bool TryGetMapObject(Point position, [NotNullWhen(true)] out GameObject? gameObject, bool useDisplayedPos = false)
    {
        bool foundStatic = TryGetStaticObject(position, out GameObject? foundStaticObject);
        bool foundDynamic = false;
        if(!foundStatic)
        {
            foundDynamic = TryGetDynamicObject(position, out GameObject? foundDynamicObject, useDisplayedPos);
            if(foundDynamic)
            {
                gameObject = foundDynamicObject;
                return true;
            }
        }
        else
        {
            gameObject = foundStaticObject;
            return true;
        }

        gameObject = null;
        return false;
    }

    public bool CheckPositionFree(Point position)
    {
        // Check new position is valid
        if (!SurfaceObject.Surface.IsValidCell(position.X, position.Y)) return false;

        // Check if other object is there
        if (TryGetMapObject(position, out GameObject? foundObject))
        {
            // We found an object, but it has a property which makes it irreplacable:

        }

        return true;
    }

    public void CreateWall(Point position)
    {
        if(CheckPositionFree(position))
        {
            Wall wall = new Wall(position, _mapSurface);
            _staticObjects[position.X,position.Y] = wall;
        }
    }

    public void CreateDoor(Point position)
    {
        if (CheckPositionFree(position))
        {
            Door door = new Door(position, _mapSurface);
            _staticObjects[position.X, position.Y] = door;
        }
    }

    public void RemoveMapObject(GameObject mapObject)
    {
        if (_dynamicObjects.Contains(mapObject))
        {
            _dynamicObjects.Remove(mapObject);
            mapObject.RestoreMap(this);
        }
    }

    private void FillBackground()
    {
        Color[] colors = new[] { Color.LightGreen, Color.Coral, Color.CornflowerBlue, Color.DarkGreen };
        float[] colorStops = new[] { 0f, 0.35f, 0.75f, 1f };

        Algorithms.GradientFill(_mapSurface.FontSize,
                                _mapSurface.Surface.Area.Center,
                                _mapSurface.Surface.Width / 3,
                                45,
                                _mapSurface.Surface.Area,
                                new Gradient(colors, colorStops),
                                (x, y, color) => _mapSurface.Surface[x, y].Background = color);
    }

    private void CreateTreasure()
    {
        for (int i =0; i < 1000; i++)
        {
            Point randomPosition = new Point(Game.Instance.Random.Next(0, _mapSurface.Surface.Width),
                                             Game.Instance.Random.Next(0, _mapSurface.Surface.Height));

            if(TryGetMapObject(randomPosition, out var obj))
            {
                continue;
            }

            Treasure treasure = new Treasure(randomPosition, _mapSurface);
            _dynamicObjects.Add(treasure);
            break;
        }
    }

    private void CreateMonster()
    {
        for (int i = 0; i < 1000; i++)
        {
            Point randomPosition = new Point(Game.Instance.Random.Next(0, _mapSurface.Surface.Width),
                                             Game.Instance.Random.Next(0, _mapSurface.Surface.Height));

            if (TryGetMapObject(randomPosition, out var obj))
            {
                continue;
            }

            Monster monster = new Monster(randomPosition, _mapSurface);
            _dynamicObjects.Add(monster);
            break;
        }
    }
}