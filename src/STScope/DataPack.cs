using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

// @brief  Scope Data Deal
// @author urays @date 2020-01-09
// @email  zhl.rays@outlook.com
// https://github.com/urays/

namespace IDevTrack.STScope
{
    public class DataElement
    {
        public double data { get; set; } = 0;

        public int atpage { get; set; } = -1;

        public bool valid { get; set; } = false;
    };

    public class DataList
    {
        public List<DataElement> Datas { get; private set; } = new List<DataElement>()
        {
            //new DataElement(){data = 0,atpage = -2,valid = false},
            //new DataElement(){data = 0,atpage = -1,valid = false},
        };

        public DataElement LatestData { get; set; } = new DataElement();

        public string Name { get; set; } = "";

        public int ColorNum { get; set; } = -1;

        public DataList()
        {
        }
    }

    public class DataPack
    {
        private const int DefaultColorNum = 0;

        private readonly Color[] COLORPACK = {
            Color.Black,  //默认数据颜色
            Color.Red, Color.Blue, Color.Orange, Color.Purple, Color.Sienna,
        };

        private readonly Dictionary<string, DataList> Packs = new Dictionary<string, DataList>();
        private readonly Dictionary<string, int> sPacks = new Dictionary<string, int>();

        private double _max = -Utils.Basic.DOUBLE_DELTA; //DataList 初始化时默认包含 {0,0}
        private double _min = Utils.Basic.DOUBLE_DELTA;
        private int _remain_digit = 2;  //数据小数点后保留位数

        public double ldratio { get; set; } = 1.0f; //layout / data_value

        public double sDataMax { get { return Math.Round(_max, _remain_digit); } }

        public double sDataMin { get { return Math.Round(_min, _remain_digit); } }

        public double sDataMid { get { return Math.Round((sDataMax + sDataMin) / 2, _remain_digit); } }

        public double sDataRange { get { return Math.Round(sDataMax - sDataMin, _remain_digit); } }

        public int sCount { get { return sPacks.Count; } }

        public int sMaxCount { get; private set; } = 3;

        public bool canSelect { get { return sCount < sMaxCount; } }

        public int remDigit
        {
            get { return _remain_digit; }
            set { _remain_digit = Math.Min(value, Utils.Basic.DOUBLE_MAX_DIGIT); }
        }

        public DataPack(int content)
        {
            sMaxCount = content;
        }

        public void AddPack(string name)
        {
            if (!Packs.ContainsKey(name))
            {
                Packs.Add(name, new DataList());
                Packs[name].Name = name;
                Packs[name].ColorNum = DefaultColorNum; //默认黑色
            }
        }

        public void AddData(string name, double data, int curpage)
        {
            var DT = new DataElement()
            {
                data = Math.Round(data, _remain_digit),
                atpage = curpage,
                valid = true,
            };
            AddPack(name); //尝试添加 参数对象
            Packs[name].LatestData = DT;  //更新最新数据
            if (sPacks.ContainsKey(name))
            {
                Packs[name].Datas.Add(DT);
                if (_max < _min) _max = _min = data;
                else
                {
                    if (_max < data) _max = data;
                    if (_min > data) _min = data;
                }
            }
        }

        public void KicksPack(string name)
        {
            if (sPacks.ContainsKey(name))
            {
                sPacks.Remove(name);
                Packs[name].ColorNum = DefaultColorNum;
                Packs[name].Datas.Clear();
                GlobalMaxMinReset();
            }
        }

        public void KicksPack_All()
        {
            foreach (var name in sPacks.Keys)
            {
                Packs[name].ColorNum = DefaultColorNum;
                Packs[name].Datas.Clear();
            }
            sPacks.Clear();
            GlobalMaxMinReset();
        }

        public void AddsPack(string name)
        {
            if (sPacks.Count >= sMaxCount) return;
            if (!Packs.ContainsKey(name)) return;
            if (sPacks.ContainsKey(name)) return;

            for (int i = 1; i <= sMaxCount; i++)
            {
                if (!sPacks.ContainsValue(i))
                {
                    sPacks.Add(name, i);
                    Packs[name].ColorNum = i;
                    if (Packs[name].LatestData.valid)
                    {
                        Packs[name].Datas.Add(Packs[name].LatestData);
                    }
                    GlobalMaxMinReset();
                    break;
                }
            }
        }

        public void Clean()
        {
            foreach (string item in Packs.Keys)
            {
                Packs[item].LatestData.valid = false;
                Packs[item].Datas.Clear();
            }
            GlobalMaxMinReset();
        }

        public bool isSelected(string name)
        {
            return sPacks.ContainsKey(name);
        }

        private void GlobalMaxMinReset()
        {
            _max = double.NegativeInfinity;
            _min = double.PositiveInfinity;
            foreach (KeyValuePair<string, int> item in sPacks)
            {
                List<DataElement> dt = Packs[item.Key].Datas;
                if (dt.Count > 0)
                {
                    double mx = dt.Max(t => t.data);
                    double mi = dt.Min(t => t.data);
                    if (_max < mx) _max = mx;
                    if (_min > mi) _min = mi;
                }
            }
            if (double.IsInfinity(_max) || double.IsInfinity(_min))
            {
                _max = -Utils.Basic.DOUBLE_DELTA;
                _min = Utils.Basic.DOUBLE_DELTA;  //以_max < _min 作为强制更新条件
            }
        }

        public string GetName(int index)
        {
            if (index < 0 || index >= sPacks.Count) return "";
            return sPacks.ElementAt(index).Key;
        }

        private Color DataColor(int index)
        {
            if (index < 0 || index >= sPacks.Count) return COLORPACK[DefaultColorNum];
            else return COLORPACK[sPacks.ElementAt(index).Value];
        }

        public Color DataColor(string name)
        {
            if (!sPacks.ContainsKey(name)) return COLORPACK[DefaultColorNum];
            else return COLORPACK[sPacks[name]];
        }

        public void RemoveData(int index, int maxc)
        {
            if (index < 0 || index >= sPacks.Count) return;
            string packname = sPacks.ElementAt(index).Key;
            int listcount = Packs[packname].Datas.Count;
            if (listcount - 1 > maxc)
            {
                Packs[packname].Datas.RemoveRange(0, listcount - maxc - 1);
                GlobalMaxMinReset();
            }
        }

        public delegate void delegateDrawScopeLine(Graphics g, Color clr, float ox1, float oy1, float oy2);

        public event delegateDrawScopeLine eventDrawScopeLine;

        public delegate void delegateDrawScopePoint(Graphics g, Color clr, float ox, float oy);

        public event delegateDrawScopePoint eventDrawScopePoint;

        //public delegate void delegateDrawScopeYaxis(Graphics g, Color clr, float ox);
        //public event delegateDrawScopeYaxis eventDrawScopeYaxis;

        public void DrawScope(int index, Graphics g)
        {
            if (index < 0 || index >= sPacks.Count) return;
            string pkname = sPacks.ElementAt(index).Key;
            int pkcount = Packs[pkname].Datas.Count;
            Color pkcolor = COLORPACK[sPacks[pkname]];

            for (int i = 0; i < pkcount - 1; i++)//绘制波形线
            {
                eventDrawScopeLine(g, pkcolor, i, (float)Packs[pkname].Datas[i].data,
                    (float)Packs[pkname].Datas[i + 1].data);
            }
            for (int i = 0; i < pkcount; i++)//标记数据点 以及其所在的帧
            {
                eventDrawScopePoint(g, pkcolor, i, (float)Packs[pkname].Datas[i].data);
            }
        }

        //public void DrawYAxis()
        //{
        //    if (true)
        //    {
        //        //eventDrawScopeYaxis();
        //    }

        //}
    }
}