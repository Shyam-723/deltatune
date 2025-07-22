using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeltaTune.Display
{
    public interface IDisplayService
    {
        void LoadContent();
        void BeginRun();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}