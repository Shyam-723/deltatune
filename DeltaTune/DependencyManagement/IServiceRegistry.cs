namespace DeltaTune.DependencyManagement
{
    public interface IServiceRegistry
    {
        T Get<T>();
        void Register<T>(T value);
        void Unregister<T>();
    }
}