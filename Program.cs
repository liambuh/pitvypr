using SadConsole.Configuration;
using SadConsoleGame;
using SadConsoleGame.Scenes;

Settings.WindowTitle = "PITVYPR";

Builder configuration = new Builder()
    .SetScreenSize(100, 60)
    .SetStartingScreen<RootScreen>()
    .IsStartingScreenFocused(true)
    .ConfigureFonts("..\\..\\..\\assets\\fontMain.font")
    //.OnStart(Startup)
    ;

Game.Create(configuration);
Game.Instance.Run();
Game.Instance.Dispose();

static void Startup(object? sender, GameHost host)
{
    ScreenObject container = new ScreenObject();
    Game.Instance.Screen = container;
    Color cTransparent = new Color(0, 0, 0, 0);

    //First console
    Console console1 = new(60, 14);
    console1.Position = (3, 2);
    console1.Surface.DefaultBackground = Color.AnsiCyan;
    console1.Clear();
    console1.Print(1, 1, "Type on me!");
    console1.Cursor.Position = (1, 2);
    console1.Cursor.IsEnabled = true;
    console1.Cursor.IsVisible = true;
    console1.Cursor.MouseClickReposition = true;
    console1.IsFocused = true;

    console1.FocusOnMouseClick = true;
    console1.MoveToFrontOnMouseClick = true;

    container.Children.Add(console1);

    //Add Child Surface
    ScreenSurface surfaceObject = new ScreenSurface(5, 3);
    surfaceObject.Surface.DrawBox(new Rectangle(0, 0, 5, 3), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin,
                                                    new ColoredGlyph(Color.Violet, cTransparent)));
    surfaceObject.Position = console1.Surface.Area.Center - (surfaceObject.Surface.Area.Size / 2);
    surfaceObject.UseMouse = false;

    console1.Children.Add(surfaceObject);

    //Second console
    Console console2 = new Console(90, 12);
    console2.Position = (0, 18);
    console2.Surface.DefaultBackground = Color.Black;
    console2.Clear();
    console2.Print(1, 1, "Type on me!");
    console2.DrawBox(new Rectangle(0, 0, 90, 12), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick,
                                                    new ColoredGlyph(Color.Gray, cTransparent)));
    //console2.Cursor.Position = (1, 2);
    //console2.Cursor.IsEnabled = true;
    //console2.Cursor.IsVisible = true;
    console2.FocusOnMouseClick = true;
    console2.MoveToFrontOnMouseClick = true;

    container.Children.Add(console2);
    container.Children.MoveToBottom(console2);
}