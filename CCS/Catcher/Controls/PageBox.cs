using CCS.Catcher.Internal;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CCS.Catcher.Controls
{
    public enum PageBoxCMD
    {
        //FUNC
        CtlAnls = 0, CtlGrid, CtlPlay, Slide, UsePen, UsePen2, UseLine, Undo, Redo, Capture, Load,

        //FPS
        Faster2, Faster1, Nomal, Slower1, Slower2,
    };

    public class PageBox : Control
    {
        #region Const

        //整个窗体
        private const int STD_FPS = 20;

        private const int FRONT_BLANK_HEIGHT = 35;
        private const int AFTER_BLANK_HEIGHT = 25;
        private const int DEFAULT_BORDERWIDTH = 2;
        private readonly Color DEFAULT_BACKCOLOR = SystemColors.Control;
        private readonly Color DEFAULT_BORDERCOLOR = Color.Red;
        private readonly Font DEFAULT_FONT = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
        private readonly int DEFAULT_MIN_WIDTH = Catcher.Internal.OPSwcode.DEFAULT_WIDTH + DEFAULT_BORDERWIDTH * 2;
        private readonly int DEFAULT_MIN_HEIGHT = Catcher.Internal.OPSwcode.DEFAULT_HEIGHT + FRONT_BLANK_HEIGHT + AFTER_BLANK_HEIGHT + DEFAULT_BORDERWIDTH * 4;

        //MOVEBAR
        private readonly Color DEFAULT_MOVEBAR_BACKCOLOR1 = Color.FromArgb(128, 0, 255);

        private readonly Color DEFAULT_MOVEBAR_BACKCOLOR2 = Color.LightGray;
        private readonly Color DEFAULT_MOVEBAR_TEXTCOLOR = Color.White;

        //MAPBOX
        private const int DEFAULT_MAPBOX_WIDTH = Catcher.Internal.OPSwcode.DEFAULT_WIDTH * 3;

        private const int DEFAULT_MAPBOX_HEIGHT = Catcher.Internal.OPSwcode.DEFAULT_HEIGHT * 3;
        private const int DEFAULT_MAPBOX_BORDERWIDTH = 2;
        private readonly Color DEFAULT_MAPBOX_BACKCOLOR = Color.White;
        private readonly Color DEFAULT_MAPBOX_BORDERCOLOR = Color.FromArgb(128, 0, 255);

        //MOUSEMARK
        private readonly Color DEFAULT_MOUSECELL_COLOR1 = Color.FromArgb(128, 0, 255);

        private readonly Color DEFAULT_MOUSECELL_COLOR2 = Color.Black;
        private readonly Color DEFAULT_MOUSECELL_COLOR3 = Color.FromArgb(128, 0, 255);

        #endregion Const

        private delegate float CalcMoveBarHandler(float w, int curpg, int tolpg);

        #region Fields

        private MapBox mapbox;
        private MovBar movbar;
        private MouseMark mosmark;
        private readonly Timer playtimer = null;
        private readonly double runfps = 1.0f;     //播放帧速 x0.05 ~ x4.0 x1.0

        //private bool _
        private readonly Internal.SwcodeFile swcode = new Internal.SwcodeFile();

        #endregion Fields

        #region Properies

        //public new Font Font { get; protected set; }

        public Color BorderColor { get; set; }

        public int BorderWidth { get; set; }

        public string FileName => swcode.FileName;

        public bool HasFile => swcode.isLoaded;

        public int CurPage => swcode.CurPage;

        public int TolPages => swcode.TolPage;

        #endregion Properies

        public PageBox() : base()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            //Total
            Font = DEFAULT_FONT;
            BackColor = DEFAULT_BACKCOLOR;
            BorderColor = DEFAULT_BORDERCOLOR;
            BorderWidth = DEFAULT_BORDERWIDTH;
            MinimumSize = new Size(DEFAULT_MIN_WIDTH, DEFAULT_MIN_HEIGHT);
            AllowDrop = true;
            //MapBox
            mapbox.X = 0;
            mapbox.Y = 0;
            mapbox.Width = DEFAULT_MAPBOX_WIDTH;
            mapbox.Height = DEFAULT_MAPBOX_HEIGHT;
            mapbox.BackColor = DEFAULT_MAPBOX_BACKCOLOR;
            mapbox.BorderWidth = DEFAULT_MAPBOX_BORDERWIDTH;
            mapbox.BorderColor = DEFAULT_MAPBOX_BORDERCOLOR;
            mapbox.Selected = false;
            //MovBar
            movbar.X = 0;
            movbar.Y = 0;
            movbar.Text = "";
            movbar.Height = CreateGraphics().MeasureString("427", Font).Height;
            movbar.Width = 0;
            movbar.TextColor = DEFAULT_MOVEBAR_TEXTCOLOR;
            movbar.BackColor1 = DEFAULT_MOVEBAR_BACKCOLOR1;
            movbar.BackColor2 = DEFAULT_MOVEBAR_BACKCOLOR2;
            movbar.CurWidth = 0;
            movbar.Selected = false;
            //MouseMark
            mosmark.clr1 = DEFAULT_MOUSECELL_COLOR1;
            mosmark.clr2 = mosmark.clr1;
            mosmark.clr3 = DEFAULT_MOUSECELL_COLOR3;
            mosmark.visual = true;
            mosmark.rect = new Rectangle(0, 0, 0, 0);
            mosmark.mlock = false;
            mosmark.logic = new Point(0, 0);
            mosmark.font = Font;

            playtimer = new Timer
            {
                Interval = (int)(1000 / STD_FPS / runfps),
                Enabled = false
            };

            swcode.PutAnlysis = true;
            swcode.eventCurposChanged += DisplayUpdate; //显示刷新
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Stopwatch paintWatch = new Stopwatch();

            paintWatch.Restart();

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            //绘制边框
            if (BorderWidth > 0)
            {
                GraphicsPath back1 = Catcher.Utils.Draw.CreateRect(0, 0, Width, Height);
                g.DrawPath(new Pen(BorderColor, BorderWidth), back1);
            }

            if (mapbox.BorderWidth > 0)
            {
                Rectangle _tmpb = mapbox.Border();
                GraphicsPath back2 = Catcher.Utils.Draw.CreateRect(_tmpb.X, _tmpb.Y, _tmpb.Width, _tmpb.Height);
                g.DrawPath(new Pen(mapbox.BorderColor, mapbox.BorderWidth), back2);
            }

            Point _mousep = PointToClient(Cursor.Position);

            //绘制访问进度条
            if (swcode.isLoaded)
            {
                float txtw = g.MeasureString(movbar.Text, Font).Width;
                g.FillRectangle(new SolidBrush(movbar.BackColor2), movbar.NotArea());
                g.FillRectangle(new SolidBrush(movbar.BackColor1), movbar.CurArea());
                g.DrawString(movbar.Text, Font, new SolidBrush(movbar.TextColor),
                             movbar.TextX(txtw), movbar.Y,
                             new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });
            }

            //绘制页帧图像/分析图
            g.DrawImage(swcode.CodeMap, mapbox.X, mapbox.Y);

            //标记线
            if (mosmark.visual == true)
            {
                if (mosmark.mlock == false)
                {
                    mosmark.rect = Catcher.Internal.OPSwcode.GetStdBoardRect(_mousep, mapbox.Area());
                    mosmark.logic = Catcher.Internal.OPSwcode.GetLogicCellxy(_mousep, mapbox.Area());
                }
                Catcher.Internal.OPSwcode.CellsGridXY(g, mapbox.Area(), mosmark.clr1, mosmark.clr2, mosmark.rect.Location);

                Size _txtsz = g.MeasureString(mosmark.Text(), mosmark.font).ToSize();
                int _txtx = mosmark.rect.X + mosmark.rect.Width;
                int _txty = mosmark.rect.Y - _txtsz.Height;
                if (mosmark.rect.X < mapbox.X + mosmark.rect.Width) { _txtx = mapbox.X + mosmark.rect.Width; }
                else if (mosmark.rect.X > mapbox.X + mapbox.Width - _txtsz.Width - mosmark.rect.Width) { _txtx = mapbox.X + mapbox.Width - _txtsz.Width - mosmark.rect.Width; }
                if (mosmark.rect.Y < mapbox.Y + Math.Max(mosmark.rect.Height, _txtsz.Height)) { _txty = mapbox.Y + Math.Max(mosmark.rect.Height, _txtsz.Height); }
                else if (mosmark.rect.Y > mapbox.Y + mapbox.Height - _txtsz.Height - Math.Max(mosmark.rect.Height, _txtsz.Height)) { _txty = mapbox.Y + mapbox.Height - _txtsz.Height - Math.Max(mosmark.rect.Height, _txtsz.Height); }

                g.DrawString(mosmark.Text(), mosmark.font, new SolidBrush(mosmark.clr3), _txtx, _txty);
            }

            paintWatch.Stop();
            Console.WriteLine("PaintTime: {0} ms", paintWatch.Elapsed.TotalMilliseconds);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            LayoutResize();
            DisplayUpdate(true);
        }

        private void LayoutResize()
        {
            int use_area_w = Width - BorderWidth * 2 - mapbox.BorderWidth * 2;
            int use_area_h = Height - FRONT_BLANK_HEIGHT - AFTER_BLANK_HEIGHT - mapbox.BorderWidth * 2 - BorderWidth * 2;
            int cen_x = Width / 2;
            int cen_y = FRONT_BLANK_HEIGHT + use_area_h / 2 + mapbox.BorderWidth + BorderWidth;

            int tmp_w; //临时变量 保存临时宽度
            if (swcode.WHR < use_area_w / (use_area_h * 1.0))
            {
                tmp_w = (int)(swcode.WHR * use_area_h);
            }
            else { tmp_w = use_area_w; }

            mapbox.Width = SwcodeFile.GetValidWidth(tmp_w);  //设置有效网格Paper尺寸
            mapbox.Height = (int)(mapbox.Width / swcode.WHR);
            mapbox.X = cen_x - mapbox.Width / 2;
            mapbox.Y = cen_y - mapbox.Height / 2;

            //MOVEBAR
            movbar.X = BorderWidth;
            movbar.Y = Height - movbar.Height - BorderWidth;
            movbar.Width = Width - BorderWidth * 2;

            //MOUSEMARK
            mosmark.rect = Catcher.Internal.OPSwcode.GetBoardCellRect(mosmark.logic, mapbox.Area());
        }

        private static float CalcMoveBar(float w, int curpg, int tolpg)
        {
            return (tolpg > 1) ? w * curpg / (tolpg - 1) : 0;
        }

        private void CalcMoveBarCallBack(IAsyncResult iar)
        {
            CalcMoveBarHandler hdl = (CalcMoveBarHandler)iar.AsyncState;
            movbar.CurWidth = hdl.EndInvoke(iar);
            movbar.Text = (swcode.CurPage + 1).ToString();
        }

        private void DisplayUpdate(bool clsall)
        {
            CalcMoveBarHandler hdl = new CalcMoveBarHandler(CalcMoveBar);
            IAsyncResult arr = hdl.BeginInvoke(movbar.Width, swcode.CurPage, swcode.TolPage, CalcMoveBarCallBack, hdl);

            //_picTools.Mode = Catcher.Utils.DrawPlate.MODE.NONE;
            swcode.CreateCodeMap(mapbox.Width, mapbox.Height, mapbox.BackColor);
            if (clsall) { Invalidate(); }
            else
            {
                Invalidate(mapbox.Border(), false);
                Invalidate(movbar.Border(), false);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            //MOUSEMARK
            if (!mosmark.visual)
            {
                mosmark.visual = true;
            }
            else
            {
                if (mosmark.mlock)
                {
                    Rectangle area = mosmark.rect;
                    area.X -= 5;
                    area.Y -= 5;
                    area.Width += 10;
                    area.Height += 10;
                    if (area.Contains(e.Location))
                    {
                        mosmark.mlock = false;
                        mosmark.clr2 = DEFAULT_MOUSECELL_COLOR1;
                        Invalidate();
                    }
                }
                else
                {
                    mosmark.mlock = true;
                    mosmark.clr2 = DEFAULT_MOUSECELL_COLOR2;
                    mosmark.rect = Catcher.Internal.OPSwcode.GetStdBoardRect(e.Location, mapbox.Area());
                    mosmark.logic = Catcher.Internal.OPSwcode.GetLogicCellxy(e.Location, mapbox.Area());
                    Invalidate();
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (playtimer.Enabled == false)
            {
                if (movbar.Border().Contains(e.Location))
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (mosmark.mlock == false)
                        {
                            mosmark.visual = false;
                        }
                        swcode.CurPage = e.X * (swcode.TolPage - 1) / Width;
                        movbar.Selected = true;
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            movbar.Selected = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //
            if (movbar.Selected)
            {
                swcode.CurPage = e.X * (swcode.TolPage - 1) / Width;
            }
            //
            if (!mosmark.mlock && mosmark.visual)
            {
                Invalidate(mapbox.Border(), false);
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);

            string[] s = ((string[])e.Data.GetData(DataFormats.FileDrop, false));
            string f_name = s.GetValue(0).ToString();
            FileLoading(f_name);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public void Run(PageBoxCMD cmd)
        {
            switch (cmd)
            {
                case (PageBoxCMD.CtlAnls):
                    //_swcTools.isEnAnls = !_swcTools.isEnAnls;
                    //	Paper.Invalidate();  //重绘控件
                    break;

                case (PageBoxCMD.CtlGrid):
                    //_swcTools.isEnGrid = !_swcTools.isEnGrid;
                    //Paper.Invalidate();
                    BringToFront();
                    break;

                case (PageBoxCMD.CtlPlay):
                    //	_picTools.Mode = Catcher.Utils.aaaaa.MODE.NONE;
                    playtimer.Enabled = !playtimer.Enabled;
                    break;

                case (PageBoxCMD.Slide):
                    playtimer.Enabled = false;
                    //	_picTools.Mode = Catcher.Utils.aaaaa.MODE.NONE;
                    // '' _swcTools.PicPaint(); //清除画笔痕迹
                    break;

                case (PageBoxCMD.UsePen):
                    playtimer.Enabled = false;
                    //	_picTools.Mode = Catcher.Utils.aaaaa.MODE.DOTS;
                    BringToFront();
                    break;

                case (PageBoxCMD.UsePen2):
                    playtimer.Enabled = false;
                    //	_picTools.Mode = Catcher.Utils.aaaaa.MODE.RECTDOT;
                    BringToFront();
                    break;

                case (PageBoxCMD.UseLine):
                    playtimer.Enabled = false;
                    //_picTools.Mode = Catcher.Utils.aaaaa.MODE.LINES;
                    BringToFront();
                    break;
                //case (FUNC_COMMOND.Undo): { _picTools.Undo(); break; }
                //case (FUNC_COMMOND.Redo): { _picTools.Redo(); break; }
                case (PageBoxCMD.Capture):
                    playtimer.Enabled = false;
                    BringToFront();
                    //SaveImage("crshot/");
                    break;

                case (PageBoxCMD.Load):
                    playtimer.Enabled = false;
                    BringToFront();
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "导入源文件";
                        dialog.Filter = "SWC文件|*.swc;*.bin";
                        if (dialog.ShowDialog() == DialogResult.OK) { FileLoading(dialog.FileName); }
                    }
                    break;
            }
            //选择TOOL都会强制暂停播放
            //eventPlayChanged(playtimer.Enabled);
        }

        private void FileLoading(string path)
        {
            Catcher.Internal.SwcodeFile._MSGBACK HasLoaded = Catcher.Internal.SwcodeFile._MSGBACK.Fail;
            if (Catcher.Utils.Basic.GetFileSuffix(path) == "bin") //非标准swc格式
            {
                using (Catcher.Forms.FormatForm format = new Catcher.Forms.FormatForm())
                {
                    format.StartPosition = FormStartPosition.Manual;
                    format.Location = PointToScreen(new Point(Width / 2 - format.Width / 2, Height / 2 - format.Height / 2));
                    format.FileName = Catcher.Utils.Basic.GetFileName(path) + ".bin";
                    DialogResult res = format.ShowDialog(this);
                    if (res == DialogResult.OK)
                    {
                        HasLoaded = swcode.OnLoad(path, format.FormatIGs, format.FormatWidth, format.FormatHeight);
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        HasLoaded = Catcher.Internal.SwcodeFile._MSGBACK.Fail;
                    }
                }
            }
            else { HasLoaded = swcode.OnLoad(path, -1, -1, -1); } //标准swc格式

            if (HasLoaded == Catcher.Internal.SwcodeFile._MSGBACK.Ok)
            {
                //eventSRCFileChanged(swcode.FileName);

                LayoutResize();      //重新布局
                DisplayUpdate(true); //显示刷新

                Catcher.Utils.Notify.Send(0, "文件读入成功 包含" + swcode.TolPage.ToString() + "帧");
            }
            else if (HasLoaded == Catcher.Internal.SwcodeFile._MSGBACK.Fail)
            {
                Catcher.Utils.Notify.Send(3, "文件格式错误或为空文件");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////
        private struct MapBox
        {
            public int Height;
            public int Width;
            public int X;
            public int Y;
            public int BorderWidth;
            public Color BorderColor;
            public Color BackColor;
            public bool Selected;

            public Rectangle Area()
            {
                return new Rectangle(X, Y, Width, Height);
            }

            public Rectangle Border()
            {
                return new Rectangle(X - BorderWidth, Y - BorderWidth, Width + 2 * BorderWidth, Height + 2 * BorderWidth);
            }
        };

        private struct MovBar
        {
            public float Height;
            public float Width;
            public float X;
            public float Y;
            public string Text;
            public float CurWidth;
            public Color TextColor;
            public Color BackColor1;
            public Color BackColor2;
            public bool Selected;

            public Rectangle Border()
            {
                return new Rectangle((int)X - 1, (int)Y - 1, (int)Width + 2, (int)Height + 2);
            }

            public RectangleF CurArea()
            {
                return new RectangleF(X, Y, CurWidth, Height);
            }

            public RectangleF NotArea()
            {
                return new RectangleF(X + CurWidth, Y, Width - CurWidth, Height);
            }

            public float TextX(float txtw)
            {
                float _x = 0;
                if (txtw < CurWidth)
                {
                    _x = X + CurWidth - txtw;
                    TextColor = BackColor2;
                }
                else
                {
                    _x = X + CurWidth + 2;
                    TextColor = BackColor1;
                }
                return _x;
            }
        };

        private struct MouseMark
        {
            public Rectangle rect;
            public Point logic;
            public Color clr1;
            public Color clr2;
            public Color clr3;
            public bool visual;
            public bool mlock;
            public Font font;

            public string Text()
            {
                return string.Format("[{0}, {1}]", logic.X, logic.Y);
            }
        };

        ////////////////////////////////////////////////////////////////////////////////////////
    }
}