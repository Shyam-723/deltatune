using System.Windows.Forms;

namespace DeltaTune.Settings
{
    public class SettingsMenu : ISettingsMenu
    {
        private ContextMenuStrip settingsMenuStrip;
        
        public void Show()
        {
            settingsMenuStrip?.Hide();
            
            settingsMenuStrip = new ContextMenuStrip();
            settingsMenuStrip.AutoClose = true;
            settingsMenuStrip.TopLevel = true;
            
            ToolStripMenuItem headingItem = new ToolStripMenuItem();
            headingItem.Text = "DeltaTune";
            settingsMenuStrip.Items.Add(headingItem);
            
            settingsMenuStrip.Items.Add(new ToolStripSeparator());
            
            settingsMenuStrip.Show(Cursor.Position);
        }
    }
}