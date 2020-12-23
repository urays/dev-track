using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.IControls
{
    internal class ITabControl : TabControl
    {
        public ITabControl() : base()
        {
        }

        /// <summary>
        /// 清除TabControl的边框
        /// </summary>
        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle rect = base.DisplayRectangle;
                return new Rectangle(rect.Left - 1, rect.Top - 4, rect.Width + 5, rect.Height + 8);
            }
        }
    }
}