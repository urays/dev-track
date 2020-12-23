using System;
using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.IControls
{
    public partial class IPanel : UserControl, IContainerControl
    {
        private int _oldheight = 0;
        private bool isExpand = false;

        public bool IsExpand
        {
            get { return isExpand; }
            set
            {
                isExpand = value;
                if (value)
                {
                    _oldheight = Height;
                    Height = TitleLabel.Height;
                }
                else
                {
                    Height = _oldheight;
                }
            }
        }

        public string Title
        {
            get { return TitleLabel.Text; }
            set { TitleLabel.Text = value; }
        }

        public override Color ForeColor
        {
            get
            {
                return TitleLabel.ForeColor;
            }
            set
            {
                TitleLabel.ForeColor = value;
            }
        }

        public IPanel()
        {
            InitializeComponent();
            SizeChanged += UCPanelTitle_SizeChanged;
        }

        private void lblTitle_MouseDown(object sender, MouseEventArgs e)
        {
            IsExpand = !IsExpand;
        }

        private void UCPanelTitle_SizeChanged(object sender, EventArgs e)
        {
            if (Height != TitleLabel.Height)
            {
                _oldheight = Height;
            }
            TitleLabel.Width = Width;
        }
    }
}