// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Graphics
{
    /// <summary>
    /// Provides extension methods for the <see cref="SpriteBatch"/> class.
    /// </summary>
    public static class SpriteBatchExtensions
    {

        #region ----------------------------Texture2D-----------------------------

        /// <summary>
        /// Draws a texture to the sprite batch with optional clipping.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="sourceRectangle">The source rectangle.</param>
        /// <param name="destinationRectangle">The destination rectangle.</param>
        /// <param name="color">The color to tint the texture.</param>
        /// <param name="clippingRectangle">An optional clipping rectangle.</param>
        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Rectangle sourceRectangle, Rectangle destinationRectangle, Color color, Rectangle? clippingRectangle)
        {
            if (!ClipRectangles(ref sourceRectangle, ref destinationRectangle, clippingRectangle))
                return;

            if (destinationRectangle.Width > 0 && destinationRectangle.Height > 0)
            {
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, color);
            }
        }

        #endregion -------------------------Texture2D-----------------------------

        #region ----------------------------TextureRegion-----------------------------

        /// <summary>
        /// Draws a texture region to the sprite batch.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="textureRegion">The texture region to draw.</param>
        /// <param name="position">The position to draw the texture region.</param>
        /// <param name="color">The color to tint the texture region.</param>
        /// <param name="clippingRectangle">An optional clipping rectangle.</param>
        public static void Draw(this SpriteBatch spriteBatch, Texture2DRegion textureRegion, Vector2 position, Color color, Rectangle? clippingRectangle = null)
        {
            Draw(spriteBatch, textureRegion, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0, clippingRectangle);
        }

        /// <summary>
        /// Draws a texture region to the sprite batch with specified parameters.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="textureRegion">The texture region to draw.</param>
        /// <param name="position">The position to draw the texture region.</param>
        /// <param name="color">The color to tint the texture region.</param>
        /// <param name="rotation">The rotation of the texture region.</param>
        /// <param name="origin">The origin of the texture region.</param>
        /// <param name="scale">The scale of the texture region.</param>
        /// <param name="effects">The sprite effects to apply.</param>
        /// <param name="layerDepth">The layer depth.</param>
        /// <param name="clippingRectangle">An optional clipping rectangle.</param>
        public static void Draw(this SpriteBatch spriteBatch, Texture2DRegion textureRegion, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, Rectangle? clippingRectangle = null)
        {
            var sourceRectangle = textureRegion.Bounds;

            if (clippingRectangle.HasValue)
            {
                var x = (int)(position.X - origin.X);
                var y = (int)(position.Y - origin.Y);
                var width = (int)(textureRegion.Width * scale.X);
                var height = (int)(textureRegion.Height * scale.Y);
                var destinationRectangle = new Rectangle(x, y, width, height);

                if (!ClipRectangles(ref sourceRectangle, ref destinationRectangle, clippingRectangle))
                {
                    // Clipped rectangle is empty, nothing to draw
                    return;
                }

                position.X = destinationRectangle.X + origin.X;
                position.Y = destinationRectangle.Y + origin.Y;
            }

            spriteBatch.Draw(textureRegion.Texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <summary>
        /// Draws a texture region to the sprite batch.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch.</param>
        /// <param name="textureRegion">The texture region to draw.</param>
        /// <param name="destinationRectangle">The destination rectangle.</param>
        /// <param name="color">The color to tint the texture region.</param>
        /// <param name="clippingRectangle">An optional clipping rectangle.</param>
        public static void Draw(this SpriteBatch spriteBatch, Texture2DRegion textureRegion, Rectangle destinationRectangle, Color color, Rectangle? clippingRectangle = null)
        {
            Draw(spriteBatch, textureRegion.Texture, textureRegion.Bounds, destinationRectangle, color, clippingRectangle);
        }

        #endregion -------------------------TextureRegion-----------------------------

        #region ----------------------------Utilities-----------------------------
        private static bool ClipRectangles(ref Rectangle sourceRectangle, ref Rectangle destinationRectangle, Rectangle? clippingRectangle)
        {
            if (!clippingRectangle.HasValue)
                return true;

            var originalDestination = destinationRectangle;
            destinationRectangle = destinationRectangle.Clip(clippingRectangle.Value);

            if (destinationRectangle == Rectangle.Empty)
                return false; // Clipped rectangle is empty, nothing to draw

            var scaleX = (float)sourceRectangle.Width / originalDestination.Width;
            var scaleY = (float)sourceRectangle.Height / originalDestination.Height;

            int leftDiff = destinationRectangle.Left - originalDestination.Left;
            int topDiff = destinationRectangle.Top - originalDestination.Top;

            sourceRectangle.X += (int)(leftDiff * scaleX);
            sourceRectangle.Y += (int)(topDiff * scaleY);
            sourceRectangle.Width = (int)(destinationRectangle.Width * scaleX);
            sourceRectangle.Height = (int)(destinationRectangle.Height * scaleY);

            return true;
        }

        private static Rectangle ClipSourceRectangle(Rectangle sourceRectangle, Rectangle destinationRectangle, Rectangle clippingRectangle)
        {
            var left = (float)(clippingRectangle.Left - destinationRectangle.Left);
            var right = (float)(destinationRectangle.Right - clippingRectangle.Right);
            var top = (float)(clippingRectangle.Top - destinationRectangle.Top);
            var bottom = (float)(destinationRectangle.Bottom - clippingRectangle.Bottom);
            var x = left > 0 ? left : 0;
            var y = top > 0 ? top : 0;
            var w = (right > 0 ? right : 0) + x;
            var h = (bottom > 0 ? bottom : 0) + y;

            var scaleX = (float)destinationRectangle.Width / sourceRectangle.Width;
            var scaleY = (float)destinationRectangle.Height / sourceRectangle.Height;
            x /= scaleX;
            y /= scaleY;
            w /= scaleX;
            h /= scaleY;

            return new Rectangle((int)(sourceRectangle.X + x), (int)(sourceRectangle.Y + y), (int)(sourceRectangle.Width - w), (int)(sourceRectangle.Height - h));
        }

        private static Rectangle ClipDestinationRectangle(Rectangle destinationRectangle, Rectangle clippingRectangle)
        {
            var left = clippingRectangle.Left < destinationRectangle.Left ? destinationRectangle.Left : clippingRectangle.Left;
            var top = clippingRectangle.Top < destinationRectangle.Top ? destinationRectangle.Top : clippingRectangle.Top;
            var bottom = clippingRectangle.Bottom < destinationRectangle.Bottom ? clippingRectangle.Bottom : destinationRectangle.Bottom;
            var right = clippingRectangle.Right < destinationRectangle.Right ? clippingRectangle.Right : destinationRectangle.Right;
            return new Rectangle(left, top, right - left, bottom - top);
        }

        #endregion -------------------------Utilities-----------------------------

    }
}
