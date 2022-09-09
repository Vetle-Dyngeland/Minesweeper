namespace Minesweeper.Managers
{
    public class GameManager
    {
        public Board board;

        private readonly Vector2 screenSize;

        public List<Texture2D> spriteSheets = new();
        public int tileSize = 48;

        public ICondition revealTileCondition = new MouseCondition(MouseButton.LeftButton);
        public ICondition flagTileCondition = new MouseCondition(MouseButton.RightButton);
        public ICondition replayCondition = new KeyboardCondition(Keys.R);
        public ICondition revealAllCondition = new AllCondition(
            new KeyboardCondition(Keys.E), new KeyboardCondition(Keys.LeftControl));

        private bool hasFirstBeenPressed;
        private int generationAttempts;

        public Vector2 boardSize;
        public int mineCount;

        public GameManager(Vector2 screenSize)
        {
            this.screenSize = screenSize;

            boardSize = new(20, 20);
            mineCount = 128;
        }

        public void LoadContent(ContentManager content)
        {
            LoadTexture(content, spriteSheets, "tiles");

            CreateClasses();
            NewGame();
        }

        private static void LoadTexture(ContentManager content, List<Texture2D> textureList, string name)
        {
            textureList.Add(content.Load<Texture2D>("Sprites/" + name));
        }

        private void CreateClasses()
        {
            board = new(spriteSheets[0], tileSize, 16, mineCount) {
                width = (int)boardSize.X, height = (int)boardSize.Y,
            };
        }

        private void NewGame()
        {
            hasFirstBeenPressed = false;
            board.Generate();
        }

        public void Update()
        {
            board.Update();

            if(revealAllCondition.Pressed()) {
                for(int x = 0; x < board.width; x++) {
                    for(int y = 0; y < board.height; y++) {
                        board.tiles[x, y].revealed = !board.tiles[x, y].revealed;
                    }
                }
            }
            if(replayCondition.Pressed()) {
                NewGame();
            }
            if(revealTileCondition.Pressed()) {
                int[] tilePos = board.PositionToTile(InputHelper.NewMouse.Position.ToVector2());
                if(hasFirstBeenPressed) {
                    board.RevealTileAt(tilePos[0], tilePos[1]);
                }
                else {
                    generationAttempts = 0;
                    while(board.tiles[tilePos[0], tilePos[1]].number > 0) {
                        board.Generate();
                        generationAttempts++;
                    }
                    board.RevealTileAt(tilePos[0], tilePos[1]);
                    hasFirstBeenPressed = true;
                }
            }
            if(flagTileCondition.Pressed()) {
                int[] tilePos = board.PositionToTile(InputHelper.NewMouse.Position.ToVector2());
                board.FlagTileAt(tilePos[0], tilePos[1]);
            }
            board.Regenerate();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            board.Draw(spriteBatch);
        }
    }
}