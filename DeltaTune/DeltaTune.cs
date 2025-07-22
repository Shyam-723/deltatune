using System;
using DeltaTune.Display;
using DeltaTune.Media;
using DeltaTune.Window;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeltaTune
{
    public class DeltaTune : Game
    {
        private GraphicsDeviceManager graphicsDeviceManagerInstance;
        private IWindowService windowService;
        private IMediaInfoProvider mediaInfoProvider;
        private IDisplayService displayService;
        
        public DeltaTune()
        {
            graphicsDeviceManagerInstance = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferredBackBufferWidth = 1,
                PreferredBackBufferHeight = 1
            };
            
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        { 
            windowService = new WindowService(this, graphicsDeviceManagerInstance);
            mediaInfoProvider = new SystemMediaInfoProvider();
            displayService = new DisplayService(mediaInfoProvider, GraphicsDevice);
            
            windowService.InitializeWindow();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            displayService.LoadContent();
            
            base.LoadContent();
        }

        protected override void BeginRun()
        {
            displayService.BeginRun();
            
            base.BeginRun();
        }

        protected override void Update(GameTime gameTime)
        {
            displayService.Update(gameTime);
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);
            displayService.Draw(gameTime);
            
            base.Draw(gameTime);
        }
    }
}