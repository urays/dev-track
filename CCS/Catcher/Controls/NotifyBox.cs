using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Text;

//using System.Threading;
using System.Windows.Forms;

namespace CCS.Catcher.Controls
{
    public class NotifyBox : Control
    {
        //Success = 0,
        //Warning = 1,
        //info = 2,
        //Error = 3

        private const int W0 = 216;
        private const int H0 = 34;
        private const int HeadWidth = 20;
        private const int TailWidth = 40;

        private Timer TIMER = null;

        #region Properties

        [Description("消息框的类型")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public int Type { get; set; } = 0;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = H0;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Visible = false;
        }

        //private Point _offset;
        //private bool _movflg = false;

        //protected override void OnMouseDown(MouseEventArgs e)
        //{
        //    base.OnMouseDown(e);

        //    if (e.Button == MouseButtons.Left)
        //    {
        //        _movflg = true;
        //        _offset = new Point(this.Location.X - e.Location.X, this.Location.Y - e.Location.Y);
        //    }
        //    //Console.WriteLine("ss");
        //}

        //protected override void OnMouseUp(MouseEventArgs e)
        //{
        //    base.OnMouseUp(e);
        //    this.Location = new Point(e.Location.X + _offset.X, e.Location.Y + _offset.Y);
        //    _movflg = false;
        //}

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    base.OnMouseMove(e);
        //    if (_movflg)
        //    {
        //        //Console.WriteLine(_offset.X.ToString() + " + " + _offset.Y.ToString());
        //        //this.Location = new Point(e.Location.X + _offset.X, e.Location.Y + _offset.Y);
        //    }
        //    //Console.WriteLine(e.Location.X.ToString() + " - " + e.Location.Y.ToString());
        //    //Console.WriteLine(e.Location.X.ToString() + " - " + e.Location.Y.ToString());
        //}

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Parent.BackColor);

            Width = (int)(g.MeasureString(Text, Font).Width) + HeadWidth + TailWidth;

            Brush backBrush = new SolidBrush(Color.DarkGray);
            Brush textBrush = new SolidBrush(Color.DarkGray);
            switch (Type)
            {
                case 0:  // MessageType.Success:
                    backBrush = new SolidBrush(Color.FromArgb(25, Color.Green));
                    textBrush = new SolidBrush(Color.Green);
                    break;

                case 1:  // MessageType.Warning:
                    backBrush = new SolidBrush(Color.FromArgb(25, Color.Orange));
                    textBrush = new SolidBrush(Color.Orange);
                    break;

                case 2:  //MessageType.info:
                    backBrush = new SolidBrush(Color.FromArgb(25, Color.DimGray));
                    textBrush = new SolidBrush(Color.DimGray);
                    break;

                case 3:  //MessageType.Error:
                    backBrush = new SolidBrush(Color.FromArgb(25, Color.Red));
                    textBrush = new SolidBrush(Color.Red);
                    break;

                default:
                    break;
            }

            GraphicsPath back = Catcher.Utils.Draw.CreateRoundRect(0.5f, 0.5f, Width - 1, Height - 1, 3);
            g.FillPath(new SolidBrush(Color.White), back);
            g.FillPath(backBrush, back);
            g.DrawPath(new Pen(textBrush, 1f), back);
            g.DrawString(Text, Font, textBrush, new RectangleF(HeadWidth, 0, Width - TailWidth, Height), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            g.DrawString("r", new Font("Marlett", 10), new SolidBrush(Color.Gray), new Rectangle(Width - 34, 0, 34, 34), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        #endregion Properties

        #region Constructors

        public NotifyBox()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            base.Size = new Size(W0, H0); //初始尺寸
            Font = new Font("Microsoft YaHei UI", 9F);
        }

        #endregion Constructors

        #region Method

        private void Timer_Tick(object sender, EventArgs e)
        {
            Visible = false;
            TIMER.Enabled = false;
            TIMER.Tick -= Timer_Tick;
            TIMER.Dispose();
            TIMER = null;
        }

        /// <summary>
        /// 发布通知消息
        /// </summary>
        /// <param name="type">消息类别</param>
        /// <param name="text">要显示的文本</param>
        /// <param name="Interval">显示时间（毫秒）</param>
        public void Notify(int type, string text, int Interval)
        {
            Type = type;
            Text = text;
            Visible = true;
            BringToFront();
            TIMER = new Timer();
            TIMER.Tick += Timer_Tick;
            TIMER.Interval = Interval;
            TIMER.Enabled = true;
        }

        #endregion Method
    }
}