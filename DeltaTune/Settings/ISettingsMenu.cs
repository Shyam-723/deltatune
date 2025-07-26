using System.Windows.Forms;

namespace DeltaTune.Settings
{
    public interface ISettingsMenu
    {
        ContextMenuStrip GetSettingsMenu();
    }
}