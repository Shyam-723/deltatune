using System;
using Microsoft.Xna.Framework;

namespace R3
{
    public class ObservableSystemComponent : GameComponent
    {
        private readonly Action<Exception> exceptionHandler1;

        public ObservableSystemComponent(Game game)
            : this(game, ex => System.Diagnostics.Trace.TraceError("R3 Unhandled Exception {0}", ex))
        {
        }

        public ObservableSystemComponent(Game game, Action<Exception> exceptionHandler) : base(game)
        {
            exceptionHandler1 = exceptionHandler;
        }

        public override void Initialize()
        {
            ObservableSystem.RegisterUnhandledExceptionHandler(exceptionHandler1);
            ObservableSystem.DefaultTimeProvider = MonoGameTimeProvider.Update;
            ObservableSystem.DefaultFrameProvider = MonoGameFrameProvider.Update;
        }

        public override void Update(GameTime gameTime)
        {
            MonoGameTimeProvider.Update.Tick(gameTime);
            MonoGameFrameProvider.Update.Tick();
        }
    }
}