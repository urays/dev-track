using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CCS.Catcher.Controls
{
    public class EditListBox : Control
    {
        #region Fields

        private readonly IContainer components = null;
        private readonly ScrollableControl itemPanel = null;

        #endregion Fields

        #region Properties

        public Color BorderColor { get; set; } = Color.Green;

        public int BorderWidth { get; set; } = 2;

        #endregion Properties

        public EditListBox() : base()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            components = new System.ComponentModel.Container();

            MinimumSize = new Size(50, 50);

            itemPanel = new ScrollableControl
            {
                //itemPanel.BackColor = this.BackColor;
                BackColor = Color.LightGray,
                AutoScroll = true
            };
            itemPanel.SetBounds(BorderWidth, BorderWidth, Width - 2 * BorderWidth, Height - 2 * BorderWidth);
            itemPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            Controls.Add(itemPanel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            GraphicsPath back = Catcher.Utils.Draw.CreateRect(0, 0, Width, Height);
            //g.FillPath(new SolidBrush(BackColor), back);
            g.DrawPath(new Pen(BorderColor, BorderWidth), back);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                itemPanel.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}