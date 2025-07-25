namespace DeltaTune.Settings
{
    public class SettingsFile : ISettingsFile
    {
        private readonly ISettingsService settingsService;
        private readonly string filePath;

        public SettingsFile(ISettingsService settingsService, string filePath)
        {
            this.settingsService = settingsService;
            this.filePath = filePath;
        }

        public void Load()
        {
            
        }
    }
}