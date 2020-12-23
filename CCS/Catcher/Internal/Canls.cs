using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CCS.Catcher.Internal
{
    /// <summary>
    /// 实现与图像分析算法的数据交互
    /// </summary>
    internal static class Canls
    {
#if DEBUG
        private const string DLL_PATH = "../src/Libs/canls.dll"; //图像分析包
#else
        private const string DLL_PATH = "./canls/canls.dll";
#endif

        private const int DLL_EACH_MAX = 10;  //与 DLL_PATH 中 MAXSIZE 一致

        #region DLL

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct _POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct _POINT_SET
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string name;

            public _POINT* pPoints;
            public int* endpos;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct _FLOAT
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string name;

            public float* pdata;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct _INTEGER
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string name;

            public int* pdata;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public unsafe struct DataPack
        {
            public int OFC;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DLL_EACH_MAX)]
            public _FLOAT[] OF_PRAM;

            public int OIC;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DLL_EACH_MAX)]
            public _INTEGER[] OI_PRAM;

            public int SFC;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DLL_EACH_MAX)]
            public _FLOAT[] SF_PRAM;

            public int SIC;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DLL_EACH_MAX)]
            public _INTEGER[] SI_PRAM;

            public int OPC;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = DLL_EACH_MAX)]
            public _POINT_SET[] OP_PRAM;
        }

        [DllImport(DLL_PATH, EntryPoint = "canls_dll_ver", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr canls_dll_ver(ref int w, ref int h);

        [DllImport(DLL_PATH, EntryPoint = "canls_dll_init", CallingConvention = CallingConvention.Cdecl)]
        private static extern void canls_dll_init(ref DataPack dpk);

        [DllImport(DLL_PATH, EntryPoint = "canls_dll_cls", CallingConvention = CallingConvention.Cdecl)]
        private static extern void canls_dll_cls();

        [DllImport(DLL_PATH, EntryPoint = "canls_dll_run", CallingConvention = CallingConvention.Cdecl)]
        private static extern void canls_dll_run(byte[] src);

        #endregion DLL

        #region Field

        public static string PkgName = "---";
        public static int SupW = Catcher.Internal.OPSwcode.DEFAULT_WIDTH;  //分析包支持的图像尺寸
        public static int SupH = Catcher.Internal.OPSwcode.DEFAULT_HEIGHT;

        private static Action<string, float, int> AddData;//添加某对象数据 浮点型
        private static DataPack DPK;         //数据包
        private static int curpage = -1;      //当前访问帧号
        private static bool init_ok = false;  //初始化是否完成

        private static readonly Color[] COLORPACK = { Color.Red, Color.Blue, Color.Orange, Color.Purple, Color.Sienna, };
        private static readonly List<int> point_set_visible = new List<int>(); //可视的点集数据

        #endregion Field

        #region Method

        public static void LoadInit(Action<string, float, int> adddata)
        {
            DPK.OFC = DPK.OIC = DPK.OPC = DPK.SFC = DPK.SIC = 0;
            AddData = adddata;
            try
            {
                IntPtr optr = canls_dll_ver(ref SupW, ref SupH);
                PkgName = Marshal.PtrToStringAnsi(optr);

                canls_dll_init(ref DPK);  //初始化分析包 并分配参数
                init_ok = true;
                Catcher.Utils.Notify.Send(0, string.Format("{0} SUP({1},{2})", "分析包读取成功", SupW, SupH));
            }
            catch (Exception)
            {
                Catcher.Utils.Notify.Send(3, "分析包读取失败");
            }
        }

        public static void MatchOBJ(Action<string> AF, Action<string, Color, int> AP)
        {
            if (init_ok == true)
            {
                for (int i = 0; i < DPK.OFC; i++) //浮点数据项
                {
                    AF.Invoke(DPK.OF_PRAM[i].name);
                }
                for (int i = 0; i < DPK.OIC; i++) //整型数据项
                {
                    AF.Invoke(DPK.OI_PRAM[i].name);
                }
                for (int i = 0; i < DPK.OPC; i++) //点集
                {
                    point_set_visible.Add(i);
                    AP.Invoke(DPK.OP_PRAM[i].name, COLORPACK[i], i);
                }
            }
        }

        public static unsafe void RunAnls(byte[] src, int cup)
        {
            if (init_ok && curpage != cup)
            {
                curpage = cup;
                try
                {
                    canls_dll_run(src); //赛道分析函数
                }
                catch { }

                for (int i = 0; i < DPK.OFC; i++) //浮点数据
                {
                    AddData.Invoke(DPK.OF_PRAM[i].name, *(DPK.OF_PRAM[i].pdata), curpage);
                }

                for (int i = 0; i < DPK.OIC; i++) //整型数据
                {
                    AddData.Invoke(DPK.OI_PRAM[i].name, *(DPK.OI_PRAM[i].pdata), curpage);
                }
            }
        }

        public static void DisPointSet(int l_idx)
        {
            point_set_visible.Remove(l_idx);
        }

        public static void EnPointSet(int l_idx)
        {
            point_set_visible.Add(l_idx);
        }

        public static unsafe void DrawPointSets(Bitmap g, int pgw, int pgh)
        {
            List<Point> lst = new List<Point>();
            for (int i = 0; i < point_set_visible.Count; i++)  //点集
            {
                lst.Clear();
                for (int j = 0; j < *(DPK.OP_PRAM[point_set_visible[i]].endpos); j++)
                {
                    _POINT tmp = *(DPK.OP_PRAM[point_set_visible[i]].pPoints + j);

                    //以下部分所解决的错误。完全由分析包要求尺寸与图像数据尺寸部分不匹配造成,不属于程序错误
                    if (tmp.x < 0 || tmp.x >= pgw * 8 || tmp.y < 0 || tmp.y >= pgh) { continue; }

                    lst.Add(new Point(tmp.x, tmp.y));
                }
                OPSwcode.DrawCellsFill(g, COLORPACK[point_set_visible[i]], lst);
            }
        }

        public static unsafe void mRefresh()
        {
            curpage = -1; //-1为无效页号
            canls_dll_cls(); //刷新图像分析程序
        }

        #endregion Method
    }
}