namespace SadConsoleGame.Scenes;

using SadConsole.Input;
using SadConsole.UI;
using SadConsole.UI.Controls;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

class RootScreen : ScreenObject
{
    private Map _map;
    private bool playerTurnInput;
    private Keys playerTurnKey;
    private List<Keys> KeysPressed;
    private int actorIndex;
    private bool renderUpdateFlag;
    private bool animationPlaying;

    private double elapsedTime;
    private double previousUpdateTime;
    private double animationTime;

    public bool isPlayerTurn;

    private ScreenSurface UISurf;
    private ControlHost controls;

    private SadConsole.Console MapConsole;
    private SadConsole.Console UIConsole;

    public RootScreen()
    {
        MapConsole = new SadConsole.Console(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
        //_map = new Map(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY - 5);
        _map = new Map(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
        //MapConsole.Children.Add(_map.SurfaceObject);
        Children.Add(_map.SurfaceObject);

        UIConsole = new SadConsole.Console(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
        UISurf = new ScreenSurface(Game.Instance.ScreenCellsX, Game.Instance.ScreenCellsY);
        UISurf.UseMouse = false;
        //UIConsole.Children.Add(UISurf);
        Children.Add(UISurf);
        UIConsole.FocusOnMouseClick = false;
        UISurf.FocusOnMouseClick = false;
        //UISurf.Surface.DrawBox(new Rectangle(0, 0, 20, 30), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin,
        //                                            new ColoredGlyph(Color.Violet, Color.Transparent)));

        //Colors.Default.ControlBackgroundNormal.SetColor(Color.Transparent);
        //Colors.Default.RebuildAppearances();

        controls = new ControlHost();
        controls.ClearOnAdded = false;
        UISurf.SadComponents.Add(controls);
        controls.DisableControlFocusing = true;
        

        ListBox list = new ListBox(18, 17) { Name = "list"};
        list.DrawBorder = true;
        list.Items.Add("John1");
        list.Items.Add("John2");
        list.Items.Add("John3");
        list.Items.Add("John4");
        list.Items.Add("John5");
        list.Position = new Point(1, 1);
        list.SelectedIndex = 0;

        Colors listCols = Colors.Default;
        listCols.Appearance_ControlSelected = new ColoredGlyph(Color.Blue, Color.Black);
        list.SetThemeColors(listCols);
        
        controls.Add(list);

        playerTurnInput = false;
        playerTurnKey = Keys.None;
        actorIndex = 0;
        isPlayerTurn = false;
        renderUpdateFlag = true;
        animationPlaying = false;

        KeysPressed = new List<Keys>();
    }

    private void PerformTurns()
    {
        //dynamic objects turns:
        _map.TakeTurn(actorIndex, this);

        //increment index if not player's turn:
        if(!isPlayerTurn)
        {
            animationPlaying = true;
            renderUpdateFlag = true;
            actorIndex++;
        }
        

        //player turn:
        if (isPlayerTurn)
        {
            SadConsoleGame.Action action = null;
            bool gotKey = GetPlayerAction(out action);
            if (action != null && gotKey)
            {
                ActionResult ar = action.Perform(_map, _map._player);
                actorIndex++;
                renderUpdateFlag = true;
                isPlayerTurn = false;
            }
        }

        if (actorIndex >= _map.GetDynamicObjects().Count)
        {
            actorIndex = 0;
        }
        if (actorIndex < 0)
        {
            actorIndex = _map.GetDynamicObjects().Count - 1;
        }

    }

    private void PerformAnimationQueue(TimeSpan delta)
    {
        if(animationPlaying)
        {
            //update the animationTime:
            animationTime += delta.TotalMilliseconds;

            if(_map._animationQueue.TryPeek(out Animation? anim))
            {
                if (!anim.started)
                {
                    anim.started = true;
                    animationTime = 0.0;
                }
                
                anim.Perform(_map, (float) animationTime);

                if (anim.finished)
                {
                    _map._animationQueue.Dequeue();
                }
            }
            else
            {
                animationPlaying = false;
                
            }
        }
    }

    public override void Update(TimeSpan delta)
    {
        //_map.SurfaceObject.IsFocused = true;
        //UISurf.IsFocused = false;

        //total elapsed time, just in case we need it.
        elapsedTime += delta.TotalMilliseconds;

        //base.Update(delta);
        if(!animationPlaying)
        {
            PerformTurns();
        }

        PerformAnimationQueue(delta);

        //update vision of objects on screen:
        if(renderUpdateFlag)
        {
            _map.UpdateVisibility();
            renderUpdateFlag = false;
        }

        //same as elapsedTime: defined just in case it is needed.
        previousUpdateTime = elapsedTime;
    }


    private bool GetPlayerAction([NotNullWhen(true)] out SadConsoleGame.Action? action)
    {
        Player p = _map._player;
        playerTurnInput = true;

        if (playerTurnKey == Keys.None)
        {
            action = null;
            return false;
        }
        else
        {
            if (KeysPressed.Contains(Keys.LeftControl))
            {
                foreach (var key in KeysPressed)
                {
                    switch (key)
                    {
                        case Keys.Up:
                            action = new ActionMove(new Point(p.Position.X, p.Position.Y - 1));
                            return true;
                        case Keys.Down:
                            action = new ActionMove(new Point(p.Position.X, p.Position.Y + 1));
                            return true;
                        case Keys.Left:
                            action = new ActionMove(new Point(p.Position.X - 1, p.Position.Y));
                            return true;
                        case Keys.Right:
                            action = new ActionMove(new Point(p.Position.X + 1, p.Position.Y));
                            return true;
                        case Keys.Space:
                            action = new ActionShoot(new Point(p.Position.X - 3, p.Position.Y - 1));
                            return true;
                        case Keys.None:
                            action = null;
                            return false;
                        default:
                            action = new ActionMove(new Point(p.Position.X, p.Position.Y));
                            return false;
                    }
                }

                //no actions found in quick commands:
                action = null;
                return false;
            }
            else
            {
                //Non-Control: Menu commands
                foreach(var key in KeysPressed)
                {
                    switch(key)
                    {
                        case Keys.Down:
                            //Update menu's index.
                            if(((ListBox)(controls.GetNamedControl("list"))).SelectedIndex != 0)
                            {
                                ((ListBox)(controls.GetNamedControl("list"))).SelectedIndex--;
                            }
                            else
                            {
                                ((ListBox)(controls.GetNamedControl("list"))).SelectedIndex = ((ListBox)(controls.GetNamedControl("list"))).Items.Count - 1;
                            }
                            action = null;
                            return false;
                        case Keys.Up:
                            //Update menu's index.
                            ((ListBox)(controls.GetNamedControl("list"))).SelectedIndex++;
                            action = null;
                            return false;
                        case Keys.Space:
                            //Proceed on menu.
                            //This one we will check the state of the Menu object
                            //if we are currently at a valid index of an action, proceed with the action.
                            //if the menu then returns an Action, we set action = returned action & true.
                            action = null;
                            return false;
                        case Keys.Back:
                            //Return from current Menu.
                            //Destroys Menu object currently selected.
                            action = null;
                            return false;
                        default:
                            action = null;
                            return false;
                    }
                }
                action = null;
                return false;
            }
        }
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        bool handled = false;
        Keys[] keys = { Keys.Up, Keys.Down, Keys.Right, Keys.Left, Keys.Space, Keys.LeftControl};
        KeysPressed.Clear();

        if (playerTurnInput)
        {
            foreach(Keys k in keys)
            {
                if(keyboard.IsKeyDown(k))
                {
                    playerTurnKey = k;
                    KeysPressed.Add(k);
                    playerTurnInput = false;
                    handled = true;
                    //break;
                }
            }
            if(!handled)
            {
                playerTurnKey = Keys.None;
                KeysPressed.Clear();
                handled = true;
            }
        }
        else
        {
            playerTurnKey = Keys.None;
            KeysPressed.Clear();
            handled = true;
        }
        

        return handled;
    }
}
