using System;
using Microsoft.Xna.Framework;

namespace R3
{
    public static class MonoGameObservableExtensions
    {
        /// <summary>
        /// Observe the current GameTime once.
        /// </summary>
        public static Observable<GameTime> GameTime(this Observable<Unit> source)
        {
            return new GameTimeObservable(source, MonoGameTimeProvider.Update);
        }

        /// <summary>
        /// Observes the current GameTime and the value of the source observable.
        /// </summary>
        public static Observable<(GameTime GameTime, T Item)> GameTime<T>(this Observable<T> source)
        {
            return new GameTimeObservable<T>(source,  MonoGameTimeProvider.Update);
        }
    }

    internal sealed class GameTimeObservable : Observable<GameTime>
    {
        private readonly Observable<Unit> source1;
        private readonly MonoGameTimeProvider frameProvider1;

        public GameTimeObservable(Observable<Unit> source, MonoGameTimeProvider frameProvider)
        {
            source1 = source;
            frameProvider1 = frameProvider;
        }

        protected override IDisposable SubscribeCore(Observer<GameTime> observer)
        {
            return source1.Subscribe(new _GameTime(observer, frameProvider1));
        }

        sealed class _GameTime : Observer<Unit>
        {
            private readonly Observer<GameTime> observer1;
            private readonly MonoGameTimeProvider timeProvider1;

            public _GameTime(Observer<GameTime> observer, MonoGameTimeProvider timeProvider)
            {
                observer1 = observer;
                timeProvider1 = timeProvider;
            }

            protected override void OnNextCore(Unit value)
            {
                observer1.OnNext(timeProvider1.GameTime);
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                observer1.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                observer1.OnCompleted(result);
            }
        }
    }

    internal sealed class GameTimeObservable<T> : Observable<(GameTime gameTime, T Item)>
    {
        private readonly Observable<T> source1;
        private readonly MonoGameTimeProvider timeProvider1;

        public GameTimeObservable(Observable<T> source, MonoGameTimeProvider timeProvider)
        {
            source1 = source;
            timeProvider1 = timeProvider;
        }

        protected override IDisposable SubscribeCore(Observer<(GameTime gameTime, T Item)> observer)
        {
            return source1.Subscribe(new _GameTime(observer, timeProvider1));
        }

        sealed class _GameTime : Observer<T>
        {
            private readonly Observer<(GameTime GameTime, T Item)> observer1;
            private readonly MonoGameTimeProvider timeProvider2;

            public _GameTime(Observer<(GameTime GameTime, T Item)> observer, MonoGameTimeProvider timeProvider)
            {
                observer1 = observer;
                timeProvider2 = timeProvider;
            }

            protected override void OnNextCore(T value)
            {
                observer1.OnNext((timeProvider2.GameTime, value));
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                observer1.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                observer1.OnCompleted(result);
            }
        }
    }
}