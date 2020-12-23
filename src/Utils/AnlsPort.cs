using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace IDevTrack.Utils
{
    /// <summary>
    /// 实现与图像分析算法的数据交互
    /// </summary>
    internal static class AnlsPort
    {
        private const string DLL_PATH = "canls/canls.dll"; //图像分析包
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
        public unsafe struct _DATAPACK
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
        private static extern void canls_dll_init(ref _DATAPACK dpk);

        [DllImport(DLL_PATH, EntryPoint = "canls_dll_cls", CallingConvention = CallingConvention.Cdecl)]
        private static extern void canls_dll_cls();

        [DllImport(DLL_PATH, EntryPoint = "canls_dll_run", CallingConvention = CallingConvention.Cdecl)]
        private static extern void canls_dll_run(byte[] src);

        #endregion DLL

        #region Field

        public static string Version = "---"; //分析包类型和版本号
        public static int SupW = OPSwcode.DEFAULT_WIDTH;  //分析包支持的图像尺寸
        public static int SupH = OPSwcode.DEFAULT_HEIGHT;

        private static Action<string> ScpAddItem;            //添加scope数据对象
        private static Action<string, float, int> ScpAddData;//添加某对象数据 浮点型
        private static Action<string, Color, int> AddLineItem;      //添加线性数据对象
        private static _DATAPACK DPK;         //数据包
        private static int curpage = -1;      //当前访问帧号
        private static bool init_ok = false;  //初始化是否完成

        private static readonly Color[] COLORPACK = { Color.Red, Color.Blue, Color.Orange, Color.Purple, Color.Sienna, };

        private class ILINE_INFO
        {
            public List<Point> POINTS = new List<Point>();
            public Color clr;

            public ILINE_INFO(int i)
            {
                POINTS.Clear();
                clr = COLORPACK[i];
            }
        }

        private static readonly Dictionary<int, ILINE_INFO> LINE_info = new Dictionary<int, ILINE_INFO>();
        private static readonly List<int> point_set_visible = new List<int>(); //可视的点集

        #endregion Field

        #region Method

        public static void ReadInfo()
        {
            IntPtr optr = canls_dll_ver(ref SupW, ref SupH);
            Version = Marshal.PtrToStringAnsi(optr);
        }

        public static void Init(Action<string> additem,
                                Action<string, float, int> adddata,
                                Action<string, Color, int> addlitem)
        {
            DPK.OFC = DPK.OIC = DPK.OPC = DPK.SFC = DPK.SIC = 0;
            ScpAddItem = additem;
            ScpAddData = adddata;
            AddLineItem = addlitem;
            try
            {
                canls_dll_init(ref DPK);  //初始化分析包 并分配参数
                init_ok = true;
                IDevTrack.Utils.Notify.Send(0, "分析包读取成功");
            }
            catch (Exception)
            {
                IDevTrack.Utils.Notify.Send(3, "分析包读取失败");
            }
        }

        public static void MatchOBJ()
        {
            if (init_ok == true)
            {
                for (int i = 0; i < DPK.OFC; i++) //浮点数据项
                {
                    ScpAddItem.Invoke(DPK.OF_PRAM[i].name);
                }
                for (int i = 0; i < DPK.OIC; i++) //整型数据项
                {
                    ScpAddItem.Invoke(DPK.OI_PRAM[i].name);
                }
                for (int i = 0; i < DPK.OPC; i++)
                {
                    LINE_info.Add(i, new ILINE_INFO(i));
                    point_set_visible.Add(i);
                    AddLineItem.Invoke(DPK.OP_PRAM[i].name, LINE_info[i].clr, i);
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

                for (int i = 0; i < DPK.OFC; i++) //浮点数据项
                {
                    ScpAddData.Invoke(DPK.OF_PRAM[i].name, *(DPK.OF_PRAM[i].pdata), curpage);
                }
                for (int i = 0; i < DPK.OIC; i++) //整型数据项
                {
                    ScpAddData.Invoke(DPK.OI_PRAM[i].name, *(DPK.OI_PRAM[i].pdata), curpage);
                }
            }
        }

        private static unsafe void TransLineData(int l_idx) //l_idx:line index
        {
            LINE_info[l_idx].POINTS.Clear();
            for (int i = 0; i < *(DPK.OP_PRAM[l_idx].endpos); i++)
            {
                _POINT tmp = *(DPK.OP_PRAM[l_idx].pPoints + i);
                LINE_info[l_idx].POINTS.Add(new Point(tmp.x, tmp.y));
            }
        }

        public static void DisLineData(int l_idx)
        {
            point_set_visible.Remove(l_idx);
        }

        public static void EnLineData(int l_idx)
        {
            point_set_visible.Add(l_idx);
        }

        public static unsafe void DrawLineData(Graphics g, int objw, int objh)
        {
            for (int i = 0; i < point_set_visible.Count; i++)
            {
                TransLineData(point_set_visible[i]);
                OPSwcode.DrawCells(g, objw, objh, LINE_info[point_set_visible[i]].clr, LINE_info[point_set_visible[i]].POINTS.ToArray());
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