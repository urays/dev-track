using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

// @brief  approach to analyzing swcode.
// @author urays @date 2019-02-08
// @email  zhl.rays@outlook.com
// https://github.com/urays/

namespace IDevTrack.Utils
{
    /// <summary>
    /// 数据文件抽象类
    /// </summary>
    internal abstract class DataFile
    {
        //最小帧号 = 0 最大帧号 =_tolpage -1

        #region Field

        protected int _curpage; //当前访问帧

        #endregion Field

        #region delegate

        public delegate void delegateCurposChanged();

        public event delegateCurposChanged eventCurposChanged;

        #endregion delegate

        #region Constructors

        public DataFile()
        {
            _curpage = 0;
        }

        /// <summary>
        /// swc格式文件标识
        /// </summary>
        protected bool IsSwcMark { get; set; } = true;

        /// <summary>
        /// 数据源全路径名
        /// </summary>
        public string FileName { get; protected set; } = string.Empty;

        /// <summary>
        ///  文件包含的数据量(以帧为单位)
        /// </summary>
        public int TolPage { get; protected set; } = 0;

        /// <summary>
        /// 每帧数据的字节行高
        /// </summary>
        public int PageH { get; protected set; } = -1;

        /// <summary>
        /// 每帧数据一行包含的字节数
        /// </summary>
        public int PageW { get; protected set; } = -1;

        /// <summary>
        /// 当前访问帧索引值
        /// </summary>
        public int CurPage
        {
            get { return _curpage; }
            set
            {
                _curpage = (Math.Min(TolPage - 1, Math.Max(value, 0)));
                eventCurposChanged();
            }
        }

        public bool isLoaded
        {
            get { return FileName.Length != 0; }
        }

        /// <summary>
        /// 数据显示宽高比(即字节解压成二进制位后的宽高比)
        /// </summary>
        public virtual double WhRatio { get; } = 2.0;

        #endregion Constructors
    }

    /// <summary>
    /// 本地基础类支持SWCODE文件访问
    /// </summary>
    internal static class OPSwcode
    {
        private const string PATHSWCODE = "rcm.dll";

        #region Constants

        public const int DEFAULT_WIDTH = 120;    //0 ~ 119
        public const int DEFAULT_HEIGHT = 60;    //0 ~ 59
        public const int DEFAULT_SIZE = 900;     //120*60/8 即一帧解压后的数据大小

        #endregion Constants

        #region Methods

        /// <summary>
        /// 压缩一帧二值化图像
        /// </summary>
        /// <param name="usrc">待压缩数据首地址</param>
        /// <param name="usize">待压缩数据长度</param>
        /// <param name="pdst">处理后数组首地址</param>
        /// <param name="IGs">压缩时一字节前部忽略位数</param>
        /// <returns>处理后数据长度</returns>
        [DllImport(PATHSWCODE, EntryPoint = "swcc_encode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int swcc_encode(ref byte usrc, int usize, ref byte pdst, int IGs);

        /// <summary>
        /// 解压一帧数据
        /// </summary>
        /// <param name="psrc">待解压数据首地址</param>
        /// <param name="udst">解压后的数据首地址</param>
        /// <returns>解压后的数据长度</returns>
        [DllImport(PATHSWCODE, EntryPoint = "swcc_decode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int swcc_decode(ref byte psrc, ref byte udst);

        /// <summary>
        /// 创建包含多帧的静态文本
        /// </summary>
        /// <param name="fname">目标文本名</param>
        /// <param name="psrc">每帧数据首地址</param>
        /// <param name="setw">每帧图像数据的宽度</param>
        /// <param name="seth">每帧图像数据的高度</param>
        /// <returns>压入当前帧后目标文本包含的总帧数</returns>
        [DllImport(PATHSWCODE, EntryPoint = "swcf_push_code", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int swcf_push_code(string fname, ref byte psrc, int setw, int seth);

        /// <summary>
        /// 使读取bin格式象读取swc格式一样(一次全部调入内存)
        /// </summary>
        /// <param name="fname">目标文本名</param>
        /// <param name="IGs">压缩时一字节前部忽略位数</param>
        /// <param name="setw">每帧图像的真实宽度</param>
        /// <param name="seth">每帧图像的真实高度</param>
        [DllImport(PATHSWCODE, EntryPoint = "en_bin_as_swcf", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void en_bin_as_swcf(string fname, int IGs, int setw, int seth);

        /// <summary>
        /// 读取swc数据格式(按需求部分调入内存)
        /// </summary>
        /// <param name="fname">目标文本名</param>
        /// <param name="getw">每帧数据的宽度</param>
        /// <param name="geth">每帧数据的高度</param>
        /// <returns>文本包含的总帧数(int) 大于0:读取成功 小于等于0:文件读取失败</returns>
        [DllImport(PATHSWCODE, EntryPoint = "swcf_read_form", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int swcf_read_form(string fname, ref int getw, ref int geth);

        /// <summary>
        /// 读取静态文本中指定帧
        /// </summary>
        /// <param name="fname">静态文本地址</param>
        /// <param name="iframe">第几帧 范围(0 ~ 总帧数 - 1)</param>
        /// <param name="pdst">目标帧数据访问地址</param>
        /// <returns>指定压缩帧数据长度</returns>
        [DllImport(PATHSWCODE, EntryPoint = "swcf_read_code", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern unsafe int swcf_read_code(string fname, int iframe, byte** pdst);

        /// <summary>
        /// 释放指定文本缓存
        /// </summary>
        /// <param name="fname">待释放内存对应的目标文本名</param>
        [DllImport(PATHSWCODE, EntryPoint = "swcf_read_free", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void swcf_read_free(string fname);

        private static readonly byte[] BITC = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        /// <summary>
        /// SWCODE创建二值化位图
        /// </summary>
        /// <param name="fn">访问的swcode文件名</param>
        /// <param name="cp">当前访问的帧号</param>
        /// <param name="w">swcode的数据宽度</param>
        /// <param name="h">swcode的数据高度</param>
        /// <param name="r">绘制缩放比</param>
        /// <returns>Bitmap</returns>
        public static Bitmap Get1SwcBitmap(string fn, int cp, int w, int h, double r)
        {
            byte[] usrc = new byte[DEFAULT_SIZE];
            int sz = 0, usz = 0; //sz:未解压的数据大小  usz：解压后数据大小 15*60
            unsafe
            {
                byte* org = null; //方法中已经包含了对于cp超出范围的处理  usz = 0;
                sz = swcf_read_code(fn, cp, &org);   //读取一帧的图像数据
                usz = swcc_decode(ref *org, ref usrc[0]);//解压一帧的图像数据
            }
            Utils.AnlsPort.RunAnls(usrc, cp);//执行赛道分析 以及 搬运数据
            int x_w = (int)(w * r), x_h = (int)(h * r);//计算新的尺寸大小

            Bitmap bmp = new Bitmap(x_w, x_h, PixelFormat.Format1bppIndexed);
            //将Bitmap锁定到系统内存中
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, x_w, x_h), ImageLockMode.WriteOnly, bmp.PixelFormat);
            IntPtr iptr = bmpData.Scan0;//位图中第一个像素数据的地址
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] data = new byte[bytes];

            for (int i = 0; i < bmpData.Height; i++)
            {
                for (int j = 0; j < bmpData.Width; j += 8)
                {
                    data[i * bmpData.Stride + j / 8] = 0;
                    for (int k = 0; k < 8; ++k) //最近邻插值法 实现图像无失真缩放
                    {
                        int index = (int)(i / r) * w + (int)((j + k) / r);
                        if (index < usz * 8)
                        {
                            data[i * bmpData.Stride + j / 8] += //7200 投射到 900
                                (byte)((usrc[index / 8] & BITC[index & 7]) >> (7 - index & 7) << (7 - k));
                        }
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(data, 0, iptr, bytes);
            bmp.UnlockBits(bmpData); ///读取完元信息 解锁内存区域
            return bmp;
        }

        public static int __SWCH = -1, __SWCW = -1; //SWC文件高度 宽度

        public static Point GetLogicCellxy(Point t_xy, int objw, int objh)
        {
            return new Point(t_xy.X * __SWCW * 8 / objw, t_xy.Y * __SWCH / objh);
        }

        public static Point GetBoardCellxy(Point l_xy, int objw, int objh)
        {
            return new Point(l_xy.X * objw / __SWCW / 8, l_xy.Y * objh / __SWCH);
        }

        public static void DrawCells(Graphics g, int objw, int objh, Color clr, Point[] points)
        {
            if (__SWCH == -1 && __SWCW == -1) { __SWCH = DEFAULT_HEIGHT; __SWCW = DEFAULT_WIDTH >> 3; }
            int i = objh / __SWCH;

            if (i >= 2)  //绘制的方形点宽度为2个像素 为有效绘制
            {
                foreach (Point ele in points)
                {
                    Point stp = GetBoardCellxy(ele, objw, objh);
                    using (Brush bsh = new SolidBrush(clr))
                    {
                        g.FillRectangle(bsh, new Rectangle(stp, new Size(i, i)));
                    }
                }
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// SWCODE文件访问绘图类(picture)
    /// </summary>
    internal class SwcodeFile : DataFile
    {
        #region enum

        public enum _MSGBACK
        {
            Ok = 0,
            Fail = 1,
            Error = 2,
        };

        #endregion enum

        #region Constant

        //public enum _MSGBACK { _ERROR, _OK, _FAILED };
        private const int _MAX_WIDTH = 1560;  //maxheight 可以根据maxwidth计算得到

        //目前这个数 为 60 48 120 的公倍数 即可  2 2 3 5   2 2 2 2 3   2 2 2 2 3 5
        //240 360 480 600 720 840 960 1080 1200 1320 1440 1560 1680 1800 1920
        private readonly Tuple<int, int>[] ENGRIDW = { //达到网格启用宽度 3 5 2 2 2
                Tuple.Create(240, 30),     //(网格有效时的Paper宽度,有效宽度检测范围)
                Tuple.Create(360, 45),
                Tuple.Create(480, 60),
                Tuple.Create(600, 75),
                Tuple.Create(720, 90),
                Tuple.Create(840, 105),
                Tuple.Create(960, 120), //无延迟绘制尺寸 Intel(R) Core(TM) i5-8250U CPU@1.60GHz 1.80GHz
                Tuple.Create(1080, 135),
                Tuple.Create(1200, 150),
                Tuple.Create(1320, 165),
                Tuple.Create(1440, 180),
                Tuple.Create(1560, 195),
                Tuple.Create(1680, 210),
                Tuple.Create(1800, 225),
                Tuple.Create(1920, 240),
        };

        #endregion Constant

        #region Fields

        private PictureBox OBJ = null;    //绘制对象
        private double ADJRATIO = 0.0;    //缩放比例
        private bool HASADJCALC = false;  //计算过缩放参数标志
        private int WADDC = 0;            //网格宽度增量
        private int HADDC = 0;            //网格高度增量

        #endregion Fields

        #region Properties

        /// <summary>
        /// 数据显示宽高比(即字节解压成二进制位后的宽高比)
        /// </summary>
        public override double WhRatio
        {
            get
            {
                return PageH > 0 ? (PageW / (PageH * 0.125)) :
                    (OPSwcode.DEFAULT_WIDTH / OPSwcode.DEFAULT_HEIGHT * 1.0);
            }
        }

        /// <summary>
        /// 绘制对象最大宽度
        /// </summary>
        public int PaintMaxW { get; private set; } = _MAX_WIDTH;

        /// <summary>
        /// swcode绘制对象最大高度
        /// </summary>
        public int PaintMaxH { get { return (int)(_MAX_WIDTH / WhRatio); } }

        /// <summary>
        /// 是否开始分析图
        /// </summary>
        public bool isEnAnls { get; set; } = true;

        /// <summary>
        /// 是否开启网格功能
        /// </summary>
        public bool isEnGrid { get; set; } = false;

        /// <summary>
        ///是否可以生成有效网格
        /// </summary>
        public bool isValidGrid { get; private set; } = false;

        #endregion Properties

        #region Constructors

        public SwcodeFile() : base()
        {
            ADJRATIO = 0.0;
            HASADJCALC = false;
        }

        public void SetOBJ(PictureBox _obj)
        {
            OBJ = _obj;
            OBJ.SizeChanged += new System.EventHandler(CalcForDraw);
        }

        ~SwcodeFile()
        {
            try
            {
                OPSwcode.swcf_read_free(FileName);
                OBJ.SizeChanged -= new System.EventHandler(CalcForDraw);
            }
            catch { }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// 尝试载入数据文件
        /// </summary>
        /// <param name="fpath">全路径名</param>
        /// <param name="spare_w">备用参数 表示非swc文件的图像宽度</param>
        /// <param name="spare_h">备用参数 表示非swc文件的图像高度</param>
        /// <returns>反馈消息</returns>
        public _MSGBACK OnLoad(string fpath, int IGs, int spare_w, int spare_h)
        {
            //string prjn = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            try
            {
                int w = 0, h = 0, f = 0;
                OPSwcode.swcf_read_free(FileName); //尝试释放之前的文件占用
                if (Utils.Basic.GetFileSuffix(fpath) == "bin")
                {
                    //一次全部写入内存
                    OPSwcode.en_bin_as_swcf(fpath, IGs, spare_w, spare_h);
                }
                f = OPSwcode.swcf_read_form(fpath, ref w, ref h);
                if (f > 0 && w > 0 && h > 0)
                {
                    FileName = fpath;
                    TolPage = f;
                    _curpage = 0;
                    PageW = OPSwcode.__SWCW = w; //更新OPSwcode所需数据
                    PageH = OPSwcode.__SWCH = h;
                    HASADJCALC = false;
                    return _MSGBACK.Ok;
                }
                else { return _MSGBACK.Fail; }
            }
            catch (Exception e) ///抛出异常提示
            {
                _ = MessageBox.Show(string.Format("{0}", e.GetBaseException().Message), /*prjn + " - */"Exception Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return _MSGBACK.Error;
            }
        }

        /// <summary>
        /// 计算绘制参数
        /// </summary>
        private void CalcForDraw(object sender, EventArgs e)
        {
            if (TolPage >= 1 && PageW > 0)
            {
                double HR = OBJ.Height / (PageH * 1.0);
                double WR = OBJ.Width / (PageW * 8.0);
                WADDC = (int)WR;
                HADDC = (int)HR;
                ADJRATIO = Math.Min(HR, WR); //计算Object的放缩比
                HASADJCALC = true;
            }
            else
            {
                WADDC = OBJ.Width / OPSwcode.DEFAULT_WIDTH;
                HADDC = OBJ.Height / OPSwcode.DEFAULT_HEIGHT;
            }
        }

        /// <summary>
        /// 绘制数据帧(创建picture)
        /// </summary>
        public void PicPaint()
        {
            try
            {
                Bitmap bp;
                if (TolPage >= 1 && _curpage < TolPage)
                {
                    if (HASADJCALC == false) { CalcForDraw(null, null); }
                    Task<Bitmap> task = new Task<Bitmap>(() =>
                        OPSwcode.Get1SwcBitmap(FileName, _curpage, PageW * 8, PageH, ADJRATIO));
                    task.Start();
                    bp = task.Result;
                }
                else { bp = Utils.Basic.ColorBitmap(OBJ.Width, OBJ.Height, OBJ.BackColor); }

                if (OBJ.Image != null) { OBJ.Image.Dispose(); } //先释放原始图像资源
                OBJ.Image = Utils.Basic.BitmapToImage(bp); //比g.DrawImage()效率高很多,但需要注意内存的释放
                bp.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 计算网格有效宽度
        /// </summary>
        /// <param name="tmpw">窗口推荐尺寸</param>
        /// <returns>网格有效宽度</returns>
        public int MakeGridEffect(int tmpw)
        {
            isValidGrid = false; //初始设置,需进一步判断
            if (tmpw >= PaintMaxW && PaintMaxW == ENGRIDW[ENGRIDW.Length - 1].Item1)
            {
                isValidGrid = true;
                return PaintMaxW;
            }
            for (int i = 0; i < ENGRIDW.Length; i++)
            {
                if (tmpw <= ENGRIDW[i].Item1 + ENGRIDW[i].Item2 && tmpw >= ENGRIDW[i].Item1)
                {
                    isValidGrid = true;
                    return ENGRIDW[i].Item1;
                }
            }
            return -1;
        }

        /// <summary>
        /// 绘制网格、分析图
        /// </summary>
        /// <param name="g">GDI+绘图图面</param>
        public void AddPaint(Graphics g)
        {
            if (OBJ == null || g == null) return;
            if (isValidGrid == true)
            {
                if (isEnAnls == true) //绘制分析图
                {
                    Utils.AnlsPort.DrawLineData(g, OBJ.Width, OBJ.Height);
                }
                if (isEnGrid == true)
                {
                    for (int i = -1; i < OBJ.Width - WADDC - 1;) //绘制网格
                    {
                        i += WADDC;
                        g.DrawLine(Pens.Thistle, new Point(i, 0), new Point(i, OBJ.Height));
                    }
                    for (int j = -1; j < OBJ.Height - HADDC - 1;)
                    {
                        j += HADDC;
                        g.DrawLine(Pens.Thistle, new Point(0, j), new Point(OBJ.Width, j));
                    }
                }
            }
        }

        #endregion Methods
    }
}