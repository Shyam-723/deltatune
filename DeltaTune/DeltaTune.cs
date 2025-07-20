using DeltaTune.DependencyManagement;
using DeltaTune.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;

namespace DeltaTune
{
    public class DeltaTune : Game
    {
        private SpriteBatch spriteBatch;
        private BitmapFont musicTitleFont;

        public DeltaTune()
        {
            GlobalServices.ServiceRegistry = new ServiceRegistry();
            
            GraphicsDeviceManager graphicsDeviceManagerInstance = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            IMediaInfoProvider mediaInfoProvider = new SystemMediaInfoProvider();
            GlobalServices.Register<IMediaInfoProvider>(mediaInfoProvider);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            musicTitleFont = BitmapFont.FromFile(GraphicsDevice, "Content/Fonts/MusicTitleFont.fnt");
            musicTitleFont.FallbackCharacter = '▯';
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            IMediaInfoProvider mediaInfoProvider = GlobalServices.Get<IMediaInfoProvider>();

            if (mediaInfoProvider.Status == PlaybackStatus.Playing)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
                spriteBatch.DrawString(musicTitleFont, $"♪~   {mediaInfoProvider.Title}", new Vector2(20, 20), Color.White, 0, Vector2.Zero, 3f, SpriteEffects.None, 0);
                spriteBatch.End();
            }
            
            base.Draw(gameTime);
        }
    }
}