using System.Windows.Forms;

namespace IDevTrack.IControls
{
    public class IUserControl : UserControl
    {
        public delegate void delegateBringToFrontLater();

        public event delegateBringToFrontLater ToFrontLater;

        public new void BringToFront()
        {
            base.BringToFront();
            ToFrontLater();
        }
    }
}