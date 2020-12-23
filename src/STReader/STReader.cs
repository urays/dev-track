using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace IDevTrack.STReader
{
    public partial class ISTReader : IDevTrack.IControls.IUserControl
    {
        #region Constants

        private const int INIT_PAPER_W = Utils.OPSwcode.DEFAULT_WIDTH * 3;  //初始化宽度
        private const int INIT_PAPER_H = Utils.OPSwcode.DEFAULT_HEIGHT * 3; //画板区高度
        private const int PAPER_MinW = Utils.OPSwcode.DEFAULT_WIDTH;        //画板区最小宽度
        private const int INIT_PROGRESS_H = 5;//进度条高度
        private const int INIT_FRONT_H = 35;   //前部过度区高度
        private const int INIT_AFTER_H = 25;   //后部过度区高度
        private const int STD_FPS = 20;       //标准帧率
        private readonly Color BarColor = Color.FromArgb(128, 0, 255);

        #endregion Constants

        #region Fields

        private double _runFps = 1.0f;     //播放帧速 x0.05 ~ x4.0 x1.0
        private readonly Utils.SwcodeFile _swcTools = new Utils.SwcodeFile(); //定义swcode文件对象
        private readonly Utils.DrawPlate _picTools = new Utils.DrawPlate();   //定义绘图工具对象

        #endregion Fields

        #region delegate

        public delegate void delegateSRCFileChanged(string filename);

        public event delegateSRCFileChanged eventSRCFileChanged;

        public delegate void delegatePlayChanged(bool isplay);

        public event delegatePlayChanged eventPlayChanged;

        //public delegate void delegatePageChanged(int curpg);
        //public event delegatePageChanged eventPageChanged;

        private delegate int CalcMvblockHandler(int w, int curpg, int tolpg);

        #endregion delegate

        #region Properties

        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;
                Paper.BackColor = value;
            }
        }

        public Color PenColor
        {
            get { return _picTools.PenColor; }
            set { _picTools.PenColor = value; }
        }

        public string FileName { get { return _swcTools.FileName; } }

        public bool HasFile { get { return _swcTools.isLoaded; } }

        public int CurPage { get { return _swcTools.CurPage; } }

        public int TolPages { get { return _swcTools.TolPage; } }

        public bool isValidState { get { return _swcTools.isValidGrid; } }

        public bool isEnGrid { get { return _swcTools.isEnGrid; } }

        public bool isEnAnls { get { return _swcTools.isEnAnls; } }

        #endregion Properties

        #region Constructor

        public ISTReader()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            InitializeComponent();

            _swcTools.SetOBJ(Paper); //设置swcode文件对象
            _picTools.SetOBJ(Paper); //设置绘图工具对象
            _swcTools.eventCurposChanged += DisplayUpdate; //显示刷新
            ///画布
            Paper.Size = new Size(INIT_PAPER_W, INIT_PAPER_H);
            Paper.MinimumSize = new Size(PAPER_MinW, PAPER_MinW / 2);
            Paper.MaximumSize = new Size(_swcTools.PaintMaxW, _swcTools.PaintMaxH);//限制最大尺寸,网格绘制要求
            Paper.Left = 0;
            Paper.Top = INIT_FRONT_H;
            Paper.AllowDrop = false;
            Paper.Cursor = Cursors.Default;
            Paper.SizeMode = PictureBoxSizeMode.CenterImage;
            ///进度条
            Mvblock.BackColor = BarColor;
            Mvblock.Size = new Size(0, INIT_PROGRESS_H);
            Mvblock.Left = 0;
            Mvblock.Top = base.Height - INIT_PROGRESS_H;
            Mvblock.BringToFront();
            ///访问页显示牌
            NoticeBoard.Visible = false;
            NoticeBoard.Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            NoticeBoard.BackColor = Color.Transparent;
            NoticeBoard.ForeColor = BarColor; //与 Mvblock 相同
            NoticeBoard.BorderStyle = BorderStyle.FixedSingle;
            NoticeBoard.Left = 2;
            NoticeBoard.Top = Mvblock.Top + 2 - NoticeBoard.Height;
            ///时钟 播放时钟/动作时钟
            PlayTimer.Interval = (int)(1000 / STD_FPS / _runFps);
            PlayTimer.Enabled = false;

            base.Size = new Size(INIT_PAPER_W, INIT_PAPER_H + INIT_FRONT_H + INIT_AFTER_H);
            base.MinimumSize = new Size(PAPER_MinW, PAPER_MinW / 2 + INIT_FRONT_H + INIT_AFTER_H);
        }

        private void STReader_Load(object sender, EventArgs e)
        {
            base.BorderStyle = BorderStyle.FixedSingle;
            base.BackColor = SystemColors.Control;
            Paper.BackColor = SystemColors.Window;

            AutoLayout();    //自动布局
            DisplayUpdate(); //显示刷新
            PaperLoadCover(); //首次载入封面
        }

        private void STReader_SizeChanged(object sender, EventArgs e)
        {
            try  //中止播放
            {
                if (PlayTimer.Enabled == true)
                {
                    PlayTimer.Enabled = false;
                    eventPlayChanged(false);
                }
            }
            catch { }

            AutoLayout();    //自动布局
            DisplayUpdate(); //显示刷新
        }

        private void ISTReader_DragDrop(object sender, DragEventArgs e)
        {
            PlayTimer.Enabled = false;
            eventPlayChanged(false);

            string[] s = ((string[])e.Data.GetData(DataFormats.FileDrop, false));
            string f_name = s.GetValue(0).ToString();
            FileLoading(f_name);
        }

        private void ISTReader_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (_swcTools.CurPage < _swcTools.TolPage - 1)
            {
                _swcTools.CurPage++;
            }
            else
            {
                PlayTimer.Enabled = false;
                eventPlayChanged(false);
            }
        }

        private void AutoLayout()
        {
            int use_area_w = base.Width;
            int use_area_h = base.Height - INIT_FRONT_H - INIT_AFTER_H;
            int cen_x = base.Width / 2;
            int cen_y = INIT_FRONT_H + (base.Height - INIT_FRONT_H - INIT_AFTER_H) / 2;

            int tmp_w, t; //临时变量 保存临时宽度
            if (_swcTools.WhRatio < use_area_w / (use_area_h * 1.0))
            {
                tmp_w = (int)(_swcTools.WhRatio * use_area_h);
            }
            else { tmp_w = use_area_w; }
            if ((t = _swcTools.MakeGridEffect(tmp_w)) != -1) //设置有效网格Paper尺寸
            {
                Paper.Width = t;
                Paper.Height = (int)(t / _swcTools.WhRatio);
            }
            else
            {
                Paper.Width = tmp_w;
                Paper.Height = (int)(tmp_w / _swcTools.WhRatio);
            }
            Paper.Top = cen_y - Paper.Height / 2;
            Paper.Left = cen_x - Paper.Width / 2;
            Mvblock.Top = base.Height - INIT_PROGRESS_H - 1;
            NoticeBoard.Top = Mvblock.Top + 2 - NoticeBoard.Height;
        }

        private static int CalcMvLength(int w, int curpg, int tolpg)
        {
            return (tolpg > 1) ? w * curpg / (tolpg - 1) : 0;
        }

        private void CalcMvblockCallBack(IAsyncResult iar)
        {
            CalcMvblockHandler hdl = (CalcMvblockHandler)iar.AsyncState;
            Mvblock.Width = (int)hdl.EndInvoke(iar);

            NoticeBoard.Text = (_swcTools.CurPage + 1).ToString();
        }

        private void DisplayUpdate()
        {
            //var t1 = new Task<int>(() => CalcMvLength(base.Width, _swcTools.CurPage, _swcTools.TolPage));
            //t1.Start();
            //Mvblock.Width = t1.Result;
            CalcMvblockHandler hdl = new CalcMvblockHandler(CalcMvLength);
            IAsyncResult arr = hdl.BeginInvoke(base.Width, _swcTools.CurPage, _swcTools.TolPage, CalcMvblockCallBack, hdl);
            //
            mRefresh();
        }

        #endregion Constructor

        #region Public

        public void mRefresh()  //刷新显示
        {
            _picTools.Mode = Utils.DrawPlate.MODE.NONE;
            _swcTools.PicPaint();
            Paper.Invalidate();
        }

        public void ToolsFunc(string toolname)
        {
            //"ToolsAnls","ToolGrid","ToolPlay",
            //"ToolSlide","ToolPen","ToolPen2",
            //"ToolLine","ToolUndo","ToolRedo",
            //"ToolShotPic","ToolOpen"
            switch (toolname)
            {
                case ("ToolAnls"):
                    _swcTools.isEnAnls = !_swcTools.isEnAnls;
                    Paper.Invalidate();  //重绘控件
                    break;

                case ("ToolGrid"):
                    _swcTools.isEnGrid = !_swcTools.isEnGrid;
                    Paper.Invalidate();
                    BringToFront();
                    break;

                case ("ToolPlay"):
                    _picTools.Mode = Utils.DrawPlate.MODE.NONE;
                    PlayTimer.Enabled = !PlayTimer.Enabled;
                    break;

                case ("ToolSlide"):
                    PlayTimer.Enabled = false;
                    _picTools.Mode = Utils.DrawPlate.MODE.NONE;
                    _swcTools.PicPaint(); //清除画笔痕迹
                    break;

                case ("ToolPen"):
                    PlayTimer.Enabled = false;
                    _picTools.Mode = Utils.DrawPlate.MODE.DOTS;
                    BringToFront();
                    break;

                case ("ToolPen2"):
                    PlayTimer.Enabled = false;
                    _picTools.Mode = Utils.DrawPlate.MODE.RECTDOT;
                    BringToFront();
                    break;

                case ("ToolLine"):
                    PlayTimer.Enabled = false;
                    _picTools.Mode = Utils.DrawPlate.MODE.LINES;
                    BringToFront();
                    break;
                //case ("ToolRectange"):
                //    PlayTimer.Enabled = false;
                //    _picTools.Mode = Utils.DrawTools.MODE.RECTANGLE;
                //    this.BringToFront();
                //    break;
                case ("ToolUndo"): { _picTools.Undo(); break; }
                case ("ToolRedo"): { _picTools.Redo(); break; }
                case ("ToolShotPic"):
                    PlayTimer.Enabled = false;
                    BringToFront();
                    SaveImage("crshot/");
                    break;

                case ("ToolOpen"):
                    PlayTimer.Enabled = false;
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
            eventPlayChanged(PlayTimer.Enabled);
        }

        public string SetFps(string toolname)
        {
            switch (toolname)
            {
                case ("ToolFast1"):
                    _runFps += 0.5;
                    break;

                case ("ToolFast2"):
                    _runFps += 0.1;
                    break;

                case ("ToolFast3"):
                    _runFps = 1.0;
                    break;

                case ("ToolFast4"):
                    _runFps -= 0.1;
                    break;

                case ("ToolFast5"):
                    _runFps -= 0.5;
                    break;
            }
            if (_runFps < 0.05) _runFps = 0.05;
            else if (_runFps > 3.5) _runFps = 3.5;
            PlayTimer.Interval = (int)(1000 / STD_FPS / _runFps);

            return ((int)(STD_FPS * _runFps)).ToString();
        }

        #endregion Public

        #region DrawTools

        private Point Realxy(Point mousexy)
        {
            Point realpos = mousexy;
            realpos.Y -= (Paper.Height - Paper.Image.Height) >> 1;
            realpos.X -= (Paper.Width - Paper.Image.Width) >> 1;
            return realpos;
        }

        private void Paper_MouseDown(object sender, MouseEventArgs e)
        {
            _picTools.DrawStart(Realxy(new Point(e.X, e.Y)));
        }

        private void Paper_MouseMove(object sender, MouseEventArgs e)
        {
            if (_picTools.Mode != Utils.DrawPlate.MODE.NONE)
            {
                if (e.Button == MouseButtons.Left)
                {
                    _picTools.DrawMove(Realxy(new Point(e.X, e.Y)));
                }
            }
            else
            {
                if (PlayTimer.Enabled == false) //close auto play
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        _swcTools.CurPage = e.X * (_swcTools.TolPage - 1) / Paper.Width;
                    }
                }
            }
        }

        private void Paper_MouseUp(object sender, MouseEventArgs e)
        {
            _picTools.DrawFinish();
        }

        #endregion DrawTools

        #region Paper

        private void Paper_Paint(object sender, PaintEventArgs e)
        {
            _swcTools.AddPaint(e.Graphics);  //绘制分析图 和 网格
        }

        private void Paper_MouseWheel(object sender, MouseEventArgs e)
        {
            if (PlayTimer.Enabled == false)
            {
                int position = _swcTools.CurPage;
                position += e.Delta / 120;
                if (position < _swcTools.TolPage && position >= 0)
                {
                    _swcTools.CurPage = position;
                }
            }
        }

        private void PaperLoadCover()
        {
            Bitmap Cover = Properties.Resources.cover;
            if (Paper.Width >= Cover.Width && Paper.Height >= Cover.Height)
            {
                using (Bitmap bt = Utils.Basic.ColorBitmap(Paper.Width, Paper.Height, Paper.BackColor))
                {
                    using (Graphics g = Graphics.FromImage(bt))
                    {
                        g.DrawImage(Cover, new Rectangle((Paper.Width - Cover.Width) / 2,
                                                         (Paper.Height - Cover.Height) / 2,
                                                         Cover.Width,
                                                         Cover.Height),
                                0, 0, Cover.Width, Cover.Height, GraphicsUnit.Pixel);
                    }
                    Paper.Image = (Image)(bt).Clone();
                }
            }
        }

        #endregion Paper

        #region Utils

        private void FileLoading(string path)
        {
            IDevTrack.Utils.SwcodeFile._MSGBACK HasLoaded = IDevTrack.Utils.SwcodeFile._MSGBACK.Fail;
            if (Utils.Basic.GetFileSuffix(path) == "bin") //非标准swc格式
            {
                using (FormatForm format = new FormatForm())
                {
                    format.StartPosition = FormStartPosition.Manual;
                    format.Location = PointToScreen(new Point(Width / 2 - format.Width / 2, Height / 2 - format.Height / 2));
                    format.FileName = Utils.Basic.GetFileName(path) + ".bin";
                    DialogResult res = format.ShowDialog(this);
                    if (res == DialogResult.OK)
                    {
                        HasLoaded = _swcTools.OnLoad(path, format.FormatIGs, format.FormatWidth, format.FormatHeight);
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        HasLoaded = IDevTrack.Utils.SwcodeFile._MSGBACK.Fail;
                    }
                }
            }
            else { HasLoaded = _swcTools.OnLoad(path, -1, -1, -1); } //标准swc格式

            if (HasLoaded == IDevTrack.Utils.SwcodeFile._MSGBACK.Ok)
            {
                eventSRCFileChanged(_swcTools.FileName);
                Paper.MaximumSize = new Size(_swcTools.PaintMaxW, _swcTools.PaintMaxH); //限制最大尺寸,网格绘制要求
                AutoLayout();    //重新布局
                DisplayUpdate(); //显示刷新
                NoticeBoard.Visible = true;
                IDevTrack.Utils.Notify.Send(0, "文件读入成功 包含" + _swcTools.TolPage.ToString() + "帧");
            }
            else if (HasLoaded == IDevTrack.Utils.SwcodeFile._MSGBACK.Fail)
            {
                IDevTrack.Utils.Notify.Send(3, "文件格式错误或为空文件");
            }
        }

        private void SaveImage(string ipath)
        {
            string sn = Utils.Basic.GetFileName(_swcTools.FileName) + "_" + _swcTools.CurPage.ToString() + ".png";
            if (!Directory.Exists(ipath))
            {
                Directory.CreateDirectory(ipath);
            }

            Bitmap tmp = new Bitmap(Paper.Width - 1, Paper.Height - 1);//去除边界线
            Graphics g = Graphics.FromImage(tmp);
            g.DrawImage(Paper.Image, 0, 0);
            _swcTools.AddPaint(g);  //绘制网格
            tmp.Save(ipath + sn, ImageFormat.Png);
            g.Dispose();
            tmp.Dispose();

            if (File.Exists(ipath + sn))
            {
                IDevTrack.Utils.Notify.Send(0, "截取成功 " + Paper.Width.ToString() + " × " + Paper.Height.ToString() + " " + ipath + sn);
            }
            else
            {
                IDevTrack.Utils.Notify.Send(3, "截取失败");
            }
        }

        #endregion Utils
    }
}