using DeltaTune.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeltaTune.Display
{
    public interface IMusicTitleDisplay
    {
        MediaInfo Content { get; set; }
        Vector2 Position { get; set; }
        DisplayAnchor Anchor { get; set; }
        int ScaleFactor { get; set; }
        MusicTitleDisplayState State { get; set; }
        bool DisappearAutomatically { get; set; }
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}