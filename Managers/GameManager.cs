using Apos.Camera;
using MonoGame.Extended.Screens;

namespace Minesweeper.Managers
{
    public class GameManager
    {
        public Board board;
        public Vector2 screenSize;

        public List<Texture2D> spriteSheets = new();
        public int tileSize = 48;

        private UIManager uiManager;

        public ICondition revealTileCondition = new MouseCondition(MouseButton.LeftButton);
        public ICondition flagTileCondition = new MouseCondition(MouseButton.RightButton);
        public ICondition replayCondition = new KeyboardCondition(Keys.R);
        public ICondition revealAllCondition = new AllCondition(
            new KeyboardCondition(Keys.E), new KeyboardCondition(Keys.LeftControl));
        public ICondition chordCondition = new AllCondition(
            new MouseCondition(MouseButton.LeftButton), new MouseCondition(MouseButton.RightButton));
        public ICondition moveCameraCondition = new MouseCondition(MouseButton.MiddleButton);

        public ICondition[] moveConditions = new ICondition[] {
            new AnyCondition(new KeyboardCondition(Keys.S), new KeyboardCondition(Keys.Down)),
            new AnyCondition(new KeyboardCondition(Keys.W), new KeyboardCondition(Keys.Up)),
            new AnyCondition(new KeyboardCondition(Keys.D), new KeyboardCondition(Keys.Right)),
            new AnyCondition(new KeyboardCondition(Keys.A), new KeyboardCondition(Keys.Left))
        };

        public const float wasdCameraSpeed = 500f;

        private bool hasFirstBeenPressed;

        private const int zoomSpeed = 18;

        public Vector2 boardSize;
        public int mineCount;

        private readonly Camera camera;

        public GameManager(Vector2 screenSize, Camera camera)
        {
            this.screenSize = screenSize;
            this.camera = camera;

            boardSize = screenSize / tileSize + Vector2.One;
            mineCount = (int)((boardSize.X * boardSize.Y) / 3.75f);
        }

        public void LoadContent(ContentManager content)
        {
            LoadTexture(content, spriteSheets, "tiles");

            CreateClasses(content);
            NewGame();
        }

        private static void LoadTexture(ContentManager content, List<Texture2D> textureList, string name)
        {
            textureList.Add(content.Load<Texture2D>("Sprites/" + name));
        }

        private void CreateClasses(ContentManager content)
        {
            board = new(spriteSheets[0], tileSize, 16, mineCount) {
                width = (int)boardSize.X, height = (int)boardSize.Y,
            };

            uiManager = new(screenSize, camera, content);
        }

        private void NewGame()
        {
            hasFirstBeenPressed = false;
            board.Generate();
        }

        public void Update(float deltaTime)
        {;
            UpdateInput();
            UpdateCamera(deltaTime);
            uiManager.Update();

            board.Regenerate();
        }

        private int[] GetTileUnderMouse()
        {
            return board.PositionToTile(camera.ScreenToWorld(InputHelper.NewMouse.Position.ToVector2()));
        }

        private void UpdateInput()
        {
            if(revealAllCondition.Pressed())
                RevealAllTiles();
            if(replayCondition.Pressed())
                NewGame();
            if(revealTileCondition.Pressed())
                RevealTile();
            if(flagTileCondition.Pressed())
                FlagTile();
            if(chordCondition.Pressed())
                ChordTile();
        }

        private void RevealTile()
        {
            int[] tilePos = GetTileUnderMouse();

            if(hasFirstBeenPressed) board.RevealTileAt(tilePos[0], tilePos[1]);
            else GenerateFirstClick();
        }

        private void FlagTile()
        {
            int[] tilePos = GetTileUnderMouse();
            board.FlagTileAt(tilePos[0], tilePos[1]);
        }

        private void RevealAllTiles()
        {
            for(int x = 0; x < board.width; x++) {
                for(int y = 0; y < board.height; y++) {
                    board.tiles[x, y].revealed = !board.tiles[x, y].revealed;
                }
            }
        }

        private void GenerateFirstClick()
        {
            int[] tilePos = GetTileUnderMouse();
            while(board.tiles[tilePos[0], tilePos[1]].number > 0)
                board.Generate();
            board.RevealTileAt(tilePos[0], tilePos[1]);
            hasFirstBeenPressed = true;
        }

        private void ChordTile()
        {
            int[] tilePos = GetTileUnderMouse();
            board.ChordAt(tilePos[0], tilePos[1]);
        }

        private void UpdateCamera(float deltaTime)
        {
            Vector2 moveVector;

            moveVector.Y = Convert.ToInt16(moveConditions[0].Held()) - Convert.ToInt16(moveConditions[1].Held());
            moveVector.X = Convert.ToInt16(moveConditions[2].Held()) - Convert.ToInt16(moveConditions[3].Held());

            camera.XY += moveVector * deltaTime * wasdCameraSpeed;
            if(moveCameraCondition.Held()) camera.XY += 
                    camera.ScreenToWorld(InputHelper.OldMouse.Position.ToVector2()) - 
                    camera.ScreenToWorld(InputHelper.NewMouse.Position.ToVector2());

            int scrollWheelDelta = InputHelper.NewMouse.ScrollWheelValue - InputHelper.OldMouse.ScrollWheelValue;
            if(scrollWheelDelta != 0) {
                Vector2 prevMousePos = camera.ScreenToWorld(InputHelper.NewMouse.Position.ToVector2());
                float prevScale = camera.Scale.X;

                camera.Scale += Vector2.One * scrollWheelDelta / 10000 * zoomSpeed;
                if(camera.Scale.X <= 0) camera.Scale = Vector2.One * prevScale;

                camera.XY += prevMousePos - camera.ScreenToWorld(InputHelper.NewMouse.Position.ToVector2());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            board.Draw(spriteBatch);
            uiManager.Draw(spriteBatch);
        }
    }

    public class UIManager
    {
        private readonly Vector2 screenSize;
        private readonly Texture2D whitePixel;
        private readonly List<Texture2D> textures = new();
        private readonly Camera camera;

        public UIManager(Vector2 screenSize, Camera camera, ContentManager content)
        {
            this.screenSize = screenSize;
            this.camera = camera;

            textures.Add(content.Load<Texture2D>("Sprites/Smilies"));
            whitePixel = content.Load<Texture2D>("Sprites/whitePixel");
        }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle headerRect = new((int)(camera.X - screenSize.X / 2), (int)(camera.Y - screenSize.Y / 2), (int)screenSize.X, (int)(screenSize.Y / 10));
            spriteBatch.Draw(whitePixel, headerRect, new Color(192, 192, 192, 255));
        }
    }
}