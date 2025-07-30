using System;
using System.Windows.Forms;

namespace DeltaTune
{
    public static class Program
    {
        private static SingleInstance singleInstance = new SingleInstance(ProgramInfo.Name);
        
        public static void Main(string[] args)
        {
            using (singleInstance)
            {
                if (singleInstance.IsRunning)
                {
                    MessageBox.Show("Another instance of DeltaTune is already running.\nTo close it, right-click the DeltaTune icon in your system tray and choose \"Quit\".", ProgramInfo.Name, MessageBoxButtons.OK);
                    return;
                }

                #if DEBUG
                RunGame();
                #else
                try
                {
                    RunGame();
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        $"An unknown error has occured.\nPlease report this as a bug and include a screenshot of this message.\n\nDetails:\n{e}",
                        ProgramInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                #endif
            }
        }

        private static void RunGame()
        {
            using (var game = new DeltaTune())
            {
                game.Run();
            }
        }
    }
}