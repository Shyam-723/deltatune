using System.Windows.Forms;

namespace DeltaTune.Settings
{
    public class SettingsMenu : ISettingsMenu
    {
        private readonly ISettingsService settingsService;
        private ContextMenuStrip settingsMenuStrip;

        public SettingsMenu(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public void Show()
        {
            settingsMenuStrip?.Hide();
            
            settingsMenuStrip = new ContextMenuStrip();
            settingsMenuStrip.AutoClose = true;
            settingsMenuStrip.TopLevel = true;
            
            ToolStripMenuItem headingItem = new ToolStripMenuItem();
            headingItem.Text = "DeltaTune";
            headingItem.Enabled = false;
            settingsMenuStrip.Items.Add(headingItem);
            
            settingsMenuStrip.Items.Add(new ToolStripSeparator());
            
            settingsMenuStrip.Items.Add(GetPositionMenuItem());
            settingsMenuStrip.Items.Add(GetScaleMenuItem());
            
            settingsMenuStrip.Show(Cursor.Position);
        }

        private ToolStripMenuItem GetPositionMenuItem()
        {
            ToolStripMenuItem positionItem = new ToolStripMenuItem();
            positionItem.Text = "Position";
            
            ToolStripMenuItem topLeftItem = new ToolStripMenuItem();
            topLeftItem.Text = "Top Left";
            topLeftItem.Checked = PositionPresetHelper.GetFractionalPosition(PositionPreset.TopLeft) == settingsService.Position.Value;
            topLeftItem.Click += (sender, args) => settingsService.Position.Value = PositionPresetHelper.GetFractionalPosition(PositionPreset.TopLeft);
            positionItem.DropDownItems.Add(topLeftItem);
            
            ToolStripMenuItem topRightItem = new ToolStripMenuItem();
            topRightItem.Text = "Top Right";
            topRightItem.Checked = PositionPresetHelper.GetFractionalPosition(PositionPreset.TopRight) == settingsService.Position.Value;
            topRightItem.Click += (sender, args) => settingsService.Position.Value = PositionPresetHelper.GetFractionalPosition(PositionPreset.TopRight);
            positionItem.DropDownItems.Add(topRightItem);
            
            ToolStripMenuItem bottomLeftItem = new ToolStripMenuItem();
            bottomLeftItem.Text = "Bottom Left";
            bottomLeftItem.Checked = PositionPresetHelper.GetFractionalPosition(PositionPreset.BottomLeft) == settingsService.Position.Value;
            bottomLeftItem.Click += (sender, args) => settingsService.Position.Value = PositionPresetHelper.GetFractionalPosition(PositionPreset.BottomLeft);
            positionItem.DropDownItems.Add(bottomLeftItem);
            
            ToolStripMenuItem bottomRightItem = new ToolStripMenuItem();
            bottomRightItem.Text = "Bottom Right";
            bottomRightItem.Checked = PositionPresetHelper.GetFractionalPosition(PositionPreset.BottomRight) == settingsService.Position.Value;
            bottomRightItem.Click += (sender, args) => settingsService.Position.Value = PositionPresetHelper.GetFractionalPosition(PositionPreset.BottomRight);
            positionItem.DropDownItems.Add(bottomRightItem);

            return positionItem;
        }
        
        private ToolStripMenuItem GetScaleMenuItem()
        {
            ToolStripMenuItem scaleItem = new ToolStripMenuItem();
            scaleItem.Text = "Scale";

            for (int i = 1; i < 9; i++)
            {
                var factor = i;
                
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = $"{factor}";
                item.Checked = settingsService.ScaleFactor.Value == factor;
                item.Click += (sender, args) => settingsService.ScaleFactor.Value = factor;
                scaleItem.DropDownItems.Add(item);
            }
            
            return scaleItem;
        }
    }
}