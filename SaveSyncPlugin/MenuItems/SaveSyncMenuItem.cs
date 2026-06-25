using System.Drawing;
using System.Drawing.Drawing2D;

namespace SaveSyncPlugin.MenuItems
{
    public abstract class SaveSyncMenuItem
    {
        public virtual string Caption => "SaveSync";
        public virtual bool ShowInLaunchBox => true;
        public virtual bool ShowInBigBox => true;
        public virtual bool AllowInBigBoxWhenLocked => false;

        private static Image _icon;
        public Image IconImage
        {
            get
            {
                if (_icon == null)
                {
                    _icon = new Bitmap(16, 16);
                    using (var g = Graphics.FromImage(_icon))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                        {
                            g.FillEllipse(brush, 1, 1, 14, 14);
                        }
                        using (var pen = new Pen(Color.White, 2))
                        {
                            g.DrawLine(pen, 5, 8, 11, 8);
                            g.DrawLine(pen, 8, 5, 8, 11);
                        }
                    }
                }
                return _icon;
            }
        }

        public abstract void OnSelected();
    }
}
