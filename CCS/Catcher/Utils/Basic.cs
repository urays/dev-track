using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CCS.Catcher.Utils
{
    public static class Basic
    {
        public const double DOUBLE_DELTA = 1E-6;
        public const int DOUBLE_MAX_DIGIT = 5;  //浮点数最大小数点后保留位数

        ///释放逻辑笔、画笔、字体、位图、区域或者调色板有关的资源
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr hObj);

        public static Image BitmapToImage(Bitmap bp)
        {
            if (bp == null)
            {
                return null;
            }

            IntPtr bpp = bp.GetHbitmap();
            Image img = Image.FromHbitmap(bpp);
            //Image.FromHbitmap Method makes a copy of GDI Bitmap Need To be Released
            _ = DeleteObject(bpp);//this is vital,Otherwise Cause Memory Overflow!
            return img;
        }

        /// <summary>
        /// 设置鼠标形状
        /// </summary>
        /// <param name="paper">鼠标下方控件</param>
        /// <param name="image">Bitmap</param>
        /// <param name="ratio">Bitmap缩小比例[2^ratio]</param>
        public static void SetCursorIcon(Control paper, Bitmap image, int ratio)
        {
            if (paper == null || image == null)
            {
                return;
            }
            //光标位置位于画布中心,所以此处扩大2倍
            Bitmap mcursor = new Bitmap(image.Width >> (ratio - 1),
                image.Height >> (ratio - 1));
            Graphics g = Graphics.FromImage(mcursor);//创建画布

            g.Clear(Color.FromArgb(0, 0, 0, 0));//设置画布为透明
            //缩小image为2^ratio倍
            g.DrawImage(image, image.Width >> ratio, 0,
                image.Width >> ratio, image.Height >> ratio);

            IntPtr Hicon = mcursor.GetHicon();
            paper.Cursor = new Cursor(Hicon);//绘制光标
            _ = DeleteObject(Hicon);

            g.Dispose();   //释放资源
            mcursor.Dispose();
        }

        /// <summary>
        /// 创建一张纯色的Bitmap
        /// </summary>
        /// <param name="w">位图宽度</param>
        /// <param name="h">位图高度</param>
        /// <param name="c">位图背景色</param>
        /// <returns>Bitmap</returns>
        public static Bitmap ColorBitmap(int w, int h, Color c)
        {
            Bitmap bp = new Bitmap(w, h); //创建一张纯色Bitmap
            Graphics g = Graphics.FromImage(bp);
            g.Clear(c); //填充背景色
            g.Dispose(); //释放资源
            return bp;
        }

        /// <summary>
        /// 从全路径中提取文件名
        /// </summary>
        /// <param name="fullpath">全路径地址</param>
        /// <returns>文件名</returns>
        public static string GetFileName(string fullpath)
        {
            if (fullpath == null)
            {
                return "";
            }
            //System.IO.FileInfo(fullpath).Length
            string[] arr = fullpath.Split('/');
            string[] arr2 = arr[arr.Length - 1].Split('\\');
            return arr2[arr2.Length - 1].Split('.')[0];
        }

        /// <summary>
        /// 获取文件名后缀
        /// </summary>
        /// <param name="fullpath">全路径地址</param>
        /// <returns>后缀字符串</returns>
        public static string GetFileSuffix(string fullpath)
        {
            if (fullpath == null)
            {
                return "";
            }

            string[] arr = fullpath.Split('.');
            return arr[arr.Length - 1].ToLower(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        }
    }
}