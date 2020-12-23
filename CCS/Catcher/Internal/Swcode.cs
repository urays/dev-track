using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

// @brief  approach to analyzing swcode.
// @author urays @date 2019-02-08
// @email  zhl.rays@outlook.com
// https://github.com/urays/swcode

namespace CCS.Catcher.Internal
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

        public delegate void delegateCurposChanged(bool clsall);

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
            get => _curpage;
            set
            {
                _curpage = (Math.Min(TolPage - 1, Math.Max(value, 0)));
                eventCurposChanged(false);
            }
        }

        public bool isLoaded => FileName.Length != 0;

        /// <summary>
        /// 数据显示宽高比(即字节解压成二进制位后的宽高比)
        /// </summary>
        public virtual double WHR { get; } = 2.0;

        #endregion Constructors
    }

    /// <summary>
    /// 本地基础类支持SWCODE文件访问
    /// </summary>
    internal static class OPSwcode
    {
#if DEBUG
        private const string PATHSWCODE = "../src/Libs/rcm.dll";
#else
        private const string PATHSWCODE = "./rcm.dll";
#endif

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
            Canls.RunAnls(usrc, cp);//执行赛道分析 以及 搬运数据

            int x_w = (int)(w * r), x_h = (int)(h * r);//计算新的尺寸大小

            Bitmap bmp = new Bitmap(x_w, x_h, PixelFormat.Format32bppArgb);
            //将Bitmap锁定到系统内存中
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, x_w, x_h), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
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

        public static int __SWCH = DEFAULT_HEIGHT;
        public static int __SWCW = DEFAULT_WIDTH / 8;

        /// <summary>
        /// 通过窗口中的坐标计算获得逻辑上的格子坐标
        /// </summary>
        /// <param name="t_xy">窗口坐标</param>
        /// <param name="area">绘制区域</param>
        /// <returns>逻辑上的格子坐标</returns>
        public static Point GetLogicCellxy(Point t_xy, Rectangle area)
        {
            Point _s = new Point((t_xy.X - area.X) * __SWCW * 8 / area.Width, (t_xy.Y - area.Y) * __SWCH / area.Height);
            _s.X = _s.X < 0 ? 0 : _s.X > __SWCW * 8 - 1 ? __SWCW * 8 - 1 : _s.X;
            _s.Y = _s.Y < 0 ? 0 : _s.Y > __SWCH - 1 ? __SWCH - 1 : _s.Y;
            return _s;
        }

        /// <summary>
        /// 根据格子的逻辑坐标 左上角在窗口中的坐标
        /// </summary>
        /// <param name="l_xy">逻辑上的格子坐标</param>
        /// <param name="area">绘制区域</param>
        /// <returns>逻辑格子左上角在窗口中的坐标</returns>
        public static Point GetBoardCellxy(Point l_xy, Rectangle area)
        {
            //Debug.WriteLine("{0}  {1}  {2}", area.Width, area.Height, __SWCH);
            return new Point(area.X + l_xy.X * area.Width / __SWCW / 8, area.Y + l_xy.Y * area.Height / __SWCH);
        }

        /// <summary>
        /// 根据格子的逻辑坐标 获取矩形区域窗口坐标
        /// </summary>
        /// <param name="l_xy">逻辑上的格子坐标</param>
        /// <param name="area">绘制区域</param>
        /// <returns>>逻辑格子矩形区域的窗口坐标</returns>
        public static Rectangle GetBoardCellRect(Point l_xy, Rectangle area)
        {
            int _l = area.Height / __SWCH;
            return new Rectangle(area.X + l_xy.X * area.Width / __SWCW / 8, area.Y + l_xy.Y * area.Height / __SWCH, _l, _l);
        }

        /// <summary>
        /// 将窗口中的鼠标坐标格式化为所在格子左上角在窗口中的坐标矩形区域
        /// </summary>
        /// <param name="t_xy">窗口坐标</param>
        /// <param name="area">绘制区域</param>
        /// <returns>格式化之后的窗口坐标 为格子的左上角</returns>
        public static Rectangle GetStdBoardRect(Point t_xy, Rectangle area)
        {
            int _l = area.Height / __SWCH;
            return new Rectangle(area.X + (t_xy.X - area.X) * __SWCW * 8 / area.Width * area.Width / __SWCW / 8,
                                 area.Y + (t_xy.Y - area.Y) * __SWCH / area.Height * area.Height / __SWCH, _l, _l);
        }

        /// <summary>
        /// 绘制以某个格子为中心的X,Y两条标记线
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="area">绘制区域</param>
        /// <param name="clr1">标记线颜色</param>
        /// <param name="clr2">中心格子颜色</param>
        /// <param name="mosxy">鼠标的窗口坐标</param>
        public static void CellsGridXY(Graphics g, Rectangle area, Color clr1, Color clr2, Point mosxy)
        {
            int l = area.Height / __SWCH; //网格边长
            if (l >= 2)
            {
                Brush bsh = new SolidBrush(Catcher.Utils.Draw.ScaleAlpha(clr1, 0.5f));
                Brush bsh2 = new SolidBrush(Catcher.Utils.Draw.ScaleAlpha(clr2, 1f));
                int _mosx = (mosxy.X < area.X) ? area.X : ((mosxy.X > area.X + area.Width - l) ? area.X + area.Width - l : mosxy.X);
                int _mosy = (mosxy.Y < area.Y) ? area.Y : ((mosxy.Y > area.Y + area.Height - l) ? area.Y + area.Height - l : mosxy.Y);
                g.FillRectangle(bsh, _mosx, area.Y, l, area.Height);
                g.FillRectangle(bsh, area.X, _mosy, area.Width, l);
                g.FillRectangle(bsh2, _mosx, _mosy, l, l);
            }
        }

        /// <summary>
        /// 以cellxy为中心绘制网格
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="area">画布矩阵范围</param>
        /// <param name="clr">网格线颜色</param>
        /// <param name="cellxy">逻辑格子坐标</param>
        /// <param name="c">网格数</param>
        public static void CellsGrid(Graphics g, Rectangle area, Color clr, Point cellxy, int c)
        {
            int l = area.Height / __SWCH; //网格边长

            if (l >= 6)
            {
                int x, y;
                int x1 = (cellxy.X - c) * l + area.X;
                int x2 = (cellxy.X + c + 1) * l + area.X;
                int y1 = (cellxy.Y - c) * l + area.Y;
                int y2 = (cellxy.Y + c + 1) * l + area.Y;

                using (Pen pen = new Pen(clr, 1))
                {
                    for (int i = -c; i < c; i++) //纵向线
                    {
                        x = (cellxy.X - i) * l + area.X;
                        g.DrawLine(pen, x, y1, x, y2);
                    }
                    for (int i = -c; i < c; i++)  //横向线
                    {
                        y = (cellxy.Y - i) * l + area.Y;
                        g.DrawLine(pen, x1, y, x2, y);
                    }
                }
                g.FillRectangle(new SolidBrush(Catcher.Utils.Draw.ScaleAlpha(Color.Red, 0.5f)), new Rectangle(GetBoardCellxy(cellxy, area), new Size(l, l)));
            }
        }

        public static void DrawCellsFill(Bitmap g, Color clr, List<Point> P)
        {
            int i = g.Height / __SWCH;
            for (int j = 0; j < P.Count; j++)
            {
                int x = P[j].X * i;
                int y = P[j].Y * i;
                for (int m = x; m < x + i; m++)
                {
                    for (int n = y; n < y + i; n++) { g.SetPixel(m, n, clr); }
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

        //目前这个数 为 60 48 120 的公倍数 即可  2 2 3 5   2 2 2 2 3   2 2 2 2 3 5
        //240 360 480 600 720 840 960 1080 1200 1320 1440 1560 1680 1800 1920
        public static int[] ENGRIDW = { //达到网格启用宽度 3 5 2 2 2
            120,
            240,
            360,
            480,
            600,
            720,
            840,
            960,  //无延迟绘制尺寸 Intel(R) Core(TM) i5-8250U CPU@1.60GHz 1.80GHz
            1080,
            1200,
            1320,
            1440,
            1560,
            1680,
            1800,
            1920,
        };

        #endregion Constant

        #region Fields

        private double ADJRATIO = 0.0;    //缩放比例
        private int OLDOBJW = -2;
        private int OLDOBJH = -2;

        #endregion Fields

        #region Properties

        /// <summary>
        /// SWCODE对应的已缩放后的Bitmap
        /// </summary>
        public Bitmap CodeMap { get; protected set; } = null;

        public bool PutAnlysis { get; set; } = false;

        /// <summary>
        /// 数据显示宽高比(即字节解压成二进制位后的宽高比)
        /// </summary>
        public override double WHR => PageH > 0 ? (PageW / (PageH * 0.125)) :
                    (Catcher.Internal.OPSwcode.DEFAULT_WIDTH / Catcher.Internal.OPSwcode.DEFAULT_HEIGHT * 1.0);

        /// <summary>
        /// 绘制对象最大宽度
        /// </summary>
        public static int PaintMaxW => ENGRIDW[ENGRIDW.Length - 1];

        /// <summary>
        /// swcode绘制对象最大高度
        /// </summary>
        public int PaintMaxH => (int)(ENGRIDW[ENGRIDW.Length - 1] / WHR);

        /// <summary>
        /// 绘制对象最小宽度
        /// </summary>
        public static int PaintMinW => ENGRIDW[0];

        /// <summary>
        /// swcode绘制对象最小高度
        /// </summary>
        public int PaintMinH => (int)(ENGRIDW[0] / WHR);

        #endregion Properties

        #region Constructors

        public SwcodeFile() : base()
        {
            ADJRATIO = 0.0;
        }

        ~SwcodeFile()
        {
            try
            {
                Catcher.Internal.OPSwcode.swcf_read_free(FileName);
            }
            catch { }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// 尝试载入数据文件
        /// </summary>
        /// <param name="fpath">全路径名</param>
        /// <param name="IGs">备用参数 压缩时一字节前部忽略位数</param>
        /// <param name="spare_w">备用参数 表示非swc文件的图像宽度</param>
        /// <param name="spare_h">备用参数 表示非swc文件的图像高度</param>
        /// <returns>反馈消息</returns>
        public _MSGBACK OnLoad(string fpath, int IGs, int spare_w, int spare_h)
        {
            //string prjn = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            try
            {
                int w = 0, h = 0, f = 0;
                Catcher.Internal.OPSwcode.swcf_read_free(FileName); //尝试释放之前的文件占用
                if (Catcher.Utils.Basic.GetFileSuffix(fpath) == "bin")
                {
                    //一次全部写入内存
                    Catcher.Internal.OPSwcode.en_bin_as_swcf(fpath, IGs, spare_w, spare_h);
                }
                f = Catcher.Internal.OPSwcode.swcf_read_form(fpath, ref w, ref h);
                if (f > 0 && w > 0 && h > 0)
                {
                    FileName = fpath;
                    TolPage = f;
                    _curpage = 0;
                    PageW = Catcher.Internal.OPSwcode.__SWCW = w; //更新Catcher.Internal.OPSwcode所需数据
                    PageH = Catcher.Internal.OPSwcode.__SWCH = h;
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
        /// 绘制数据帧(创建picture)
        /// </summary>
        public void CreateCodeMap(int objW, int objH, Color objClr)
        {
            try
            {
                if (TolPage >= 1 && _curpage < TolPage)
                {
                    if (objW != OLDOBJW || objH != OLDOBJH)
                    {
                        OLDOBJW = objW;
                        OLDOBJH = objH;
                        if (PageH >= 1 && PageW >= 1)
                        {
                            double HR = objH / (PageH * 1.0);
                            double WR = objW / (PageW * 8.0);
                            ADJRATIO = Math.Min(HR, WR); //计算Object的放缩比
                        }
                    }
                    Task<Bitmap> task = new Task<Bitmap>(() =>
                        Catcher.Internal.OPSwcode.Get1SwcBitmap(FileName, _curpage, PageW * 8, PageH, ADJRATIO));
                    task.Start();
                    CodeMap = task.Result;

                    //绘制分析图
                    if (PutAnlysis) { Canls.DrawPointSets(CodeMap, PageW, PageH); }
                }
                else { CodeMap = Catcher.Utils.Basic.ColorBitmap(objW, objH, objClr); }
            }
            catch { }
        }

        /// <summary>
        /// 获取网格有效宽度
        /// </summary>
        /// <param name="tmpw">窗口推荐尺寸</param>
        /// <returns>网格有效宽度</returns>
        public static int GetValidWidth(int tmpw)
        {
            int _i = 0;
            if (tmpw <= PaintMinW) { return PaintMinW; }

            for (_i = ENGRIDW.Length - 1; _i >= 0; _i--)
            {
                if (tmpw >= ENGRIDW[_i]) { break; }
            }
            return ENGRIDW[_i];
        }

        #endregion Methods
    }
}