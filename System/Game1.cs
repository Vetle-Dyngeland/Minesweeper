using Apos.Camera;

namespace Minesweeper.System
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Vector2 screenSize = new(1920, 1080);
        private readonly bool isFullScreen = true;

        private GameManager gameManager;

        private IVirtualViewport defaultViewport;
        private Camera camera;

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
            defaultViewport = new DefaultViewport(GraphicsDevice, Window);
            camera = new(defaultViewport) {
                XY = screenSize / 2
            };

            gameManager = new(screenSize, camera);
        }

        protected override void LoadContent()
        {
            InputHelper.Setup(this);
            spriteBatch = new(GraphicsDevice);

            gameManager.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            InputHelper.UpdateSetup();
            if(InputHelper.NewKeyboard.IsKeyDown(Keys.Escape))
                Exit();

            gameManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
            InputHelper.UpdateCleanup();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(132, 132, 132, 255));

            camera.SetViewport();
            spriteBatch.Begin(transformMatrix: camera.View, samplerState: SamplerState.PointClamp);

            gameManager.Draw(spriteBatch);

            spriteBatch.End();
            camera.ResetViewport();

            base.Draw(gameTime);
        }
    }
}