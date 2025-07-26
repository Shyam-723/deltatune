namespace DeltaTune
{
    public static class Program
    {
        private static SingleInstance singleInstance = new SingleInstance(ProgramInfo.Name);
        
        public static void Main(string[] args)
        {
            using (singleInstance)
            {
                if(singleInstance.IsRunning) return;
                
                using (var game = new DeltaTune())
                {
                    game.Run();
                }
            }
        }
    }
}