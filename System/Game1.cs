namespace Minesweeper.System
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Vector2 screenSize = new(1600, 900);
        private readonly bool isFullScreen = false;

        private GameManager gameManager;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ApplyScreenSettings();
            CreateClasses();

            base.Initialize();
        }

        private void ApplyScreenSettings()
        {
            int w = (int)screenSize.X;
            int h = (int)screenSize.Y;
            if(isFullScreen) {
                w = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                h = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                screenSize = new(w, h);
            }

            graphics.PreferredBackBufferWidth = w;
            graphics.PreferredBackBufferHeight = h;
            graphics.IsFullScreen = isFullScreen;
            graphics.ApplyChanges();
        }

        private void CreateClasses()
        {
            gameManager = new(screenSize);
        }

        protected override void LoadContent()
        {
            InputHelper.Setup(this);
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gameManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            InputHelper.UpdateSetup();
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            gameManager.Update();

            base.Update(gameTime);
            InputHelper.UpdateCleanup();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            gameManager.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}