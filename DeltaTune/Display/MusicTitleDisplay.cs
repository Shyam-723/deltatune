using System;
using DeltaTune.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;

namespace DeltaTune.Display
{
    public class MusicTitleDisplay : IMusicTitleDisplay
    {
        public MediaInfo Content
        {
            get => content;
            set
            {
                content = value;
                UpdateText();
            }
        }

        public Vector2 Position { get; set; } = Vector2.Zero;
        public DisplayAnchor Anchor { get; set; }
        public int ScaleFactor { get; set; } = 1;

        public MusicTitleDisplayState State
        {
            get => state;
            set
            {
                state = value;
                animationTimer = 0;
            }
        }

        public bool DisappearAutomatically { get; set; } = true;

        private const float AppearDelayLength = 0.5f;
        private const float AppearAnimationLength = 0.75f;
        private const float DisappearAnimationLength = 0.75f;
        private const float StayTime = 2f;
        private const float SlideInDistance = 24;
        private const float SlideOutDistance = 24;

        private readonly BitmapFont font;
        
        private MediaInfo content;
        private MusicTitleDisplayState state = MusicTitleDisplayState.Hidden;

        private string text = string.Empty;
        private SizeF textSize;
        private Vector2 positionOffset;
        private float opacity;
        private double animationTimer;
        
        public MusicTitleDisplay(BitmapFont font)
        {
            this.font = font;
        }

        public void Update(GameTime gameTime)
        {
            float progress = 0f;
            switch (State)
            {
                case MusicTitleDisplayState.AppearingDelay:
                    if (animationTimer == 0)
                    {
                        opacity = 0;
                        positionOffset.X = 0;
                    }
                    
                    if(animationTimer >= AppearDelayLength) State = MusicTitleDisplayState.Appearing;
                    break;
                case MusicTitleDisplayState.Appearing:
                    if (animationTimer == 0)
                    {
                        opacity = 0;
                        positionOffset.X = 0;
                    }

                    progress = (float)(animationTimer / AppearAnimationLength);
                    
                    opacity = MathHelper.Clamp(progress * 1.5f - 0.25f, 0, 1);
                    positionOffset.X = InterpolateQuadratic(SlideInDistance * ScaleFactor, 0, progress);

                    if (animationTimer >= AppearAnimationLength)
                    {
                        State = MusicTitleDisplayState.Visible;
                    }
                    break;
                
                case MusicTitleDisplayState.Visible:
                    if (animationTimer == 0)
                    {
                        opacity = 1;
                        positionOffset.X = 0;
                    }
                    
                    if(DisappearAutomatically && animationTimer >= StayTime) State = MusicTitleDisplayState.Disappearing;
                    break;
                
                case MusicTitleDisplayState.Disappearing:
                    if (animationTimer == 0)
                    {
                        opacity = 1;
                        positionOffset.X = 0;
                    }
                    
                    progress = (float)(animationTimer / DisappearAnimationLength);
                    opacity = 1 - progress;
                    positionOffset.X = InterpolateQuadratic(-SlideOutDistance * ScaleFactor, 0, 1 - progress);
                    
                    if (animationTimer >= DisappearAnimationLength)
                    {
                        State = MusicTitleDisplayState.Hidden;
                    }
                    break;
            }
            
            if (State != MusicTitleDisplayState.Hidden)
            {
                animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (State != MusicTitleDisplayState.Hidden)
            {
                Vector2 finalPosition = Position + positionOffset;
                if (Anchor == DisplayAnchor.Right)
                {
                    finalPosition.X -= textSize.Width;
                }
                
                Color finalColor = Color.White;
                finalColor.A = (byte)MathHelper.Clamp((int)Math.Round(opacity * 255), 0, 255);
                
                spriteBatch.DrawString(font, text, finalPosition, finalColor, 0, Vector2.Zero, ScaleFactor, SpriteEffects.None, 0);
            }
        }

        private void UpdateText()
        {
            switch (Content.Status)
            {
                case PlaybackStatus.Playing:
                    text = $"♪~   {Content.Artist} - {Content.Title}";
                    break;
                case PlaybackStatus.Paused:
                    text = $"⏸~   {Content.Artist} - {Content.Title}";
                    break;
            }
            
            textSize = font.MeasureString(text);
        }
        
        private static float InterpolateQuadratic(float a, float b, float t)
        {
            float oneMinusT = 1 - t;
            float progress = 1 - oneMinusT * oneMinusT;

            return MathHelper.Lerp(a, b, progress);
        }
    }
}