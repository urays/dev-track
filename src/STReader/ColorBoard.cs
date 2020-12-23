using System;
using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.STReader
{
    public partial class ColorBoard : UserControl
    {
        public ColorBoard()
        {
            InitializeComponent();
            Size = MinimumSize = new Size(B8.Left + B8.Width, B8.Top + B8.Height);

            BackColor = System.Drawing.Color.Green;

            B1.BackColor = System.Drawing.Color.Red;
            B2.BackColor = Color.FromArgb(128, 0, 255);
            B3.BackColor = System.Drawing.Color.Blue;
            B4.BackColor = System.Drawing.Color.Green;
            B5.BackColor = System.Drawing.Color.DimGray;
            B6.BackColor = System.Drawing.Color.Orange;
            B7.BackColor = System.Drawing.Color.Purple;
            B8.BackColor = System.Drawing.Color.Brown;
        }

        private void Color_Click(object sender, EventArgs e)
        {
            BackColor = (sender as Button).BackColor;
        }

        /// this._colorboard = new ColorBoard();
        /// this._colorboard.BackColorChanged +=
    }
}