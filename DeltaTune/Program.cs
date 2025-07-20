namespace DeltaTune
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new DeltaTune())
            {
                game.Run();
            }
        }
    }
}