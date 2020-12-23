using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.STScope
{
    public partial class ISTScope : IDevTrack.IControls.IUserControl
    {
        //添加设置选项
        public readonly Tuple<string, bool>[] SetItems = {
            new Tuple<string, bool>("max", true),
            new Tuple<string, bool>("min", true),
        };

        private const int Layout_Head_H = 20;
        private const int Layout_Tail_H = 20;
        private const int Layout_Right_W = 10;

        private readonly Color TestColor1 = SystemColors.Window; //LightGray
        private readonly Color TestColor2 = SystemColors.Control;
        //private readonly Color TestColor3 = SystemColors.Control;

        private float wave_layout_top;     //绘制区域最顶边纵坐标
        private float wave_layout_bottom;  //绘制区域最低边纵坐标
        private float wave_layout_height;  //绘制波形区域的高度
        private float wave_layout_midval;  //数据中值对应的纵坐标
        private float wave_layout_start_x = 16F; //波的起始横坐标
        private readonly float wave_layout_step = 16F;    //波的振动步长

        private readonly DataPack DPK = new DataPack(5); //数据包
        private readonly Font MarkFont = DefaultFont;  //标记字体

        private readonly Tuple<Color, float>[] MarkPen = {
            new Tuple<Color, float>(Color.Black, 1),
            new Tuple<Color, float>(Color.Gray, 1),
            new Tuple<Color, float>(Color.Red, 3),
        };

        private float mv_wave_layout_midval; //移动画板所需
        private Point mv_start = new Point();

        private readonly Dictionary<string, bool> Settings = new Dictionary<string, bool>(); //参数设置字典

        public new bool Visible
        {
            get { return base.Visible; }
            set
            {
                base.Visible = value;
                if (value == false)
                {
                    DPK.KicksPack_All();//清空所有选中的数据对象以及数据
                    //pramBox.Invalidate(); //自动执行
                    //Paper.Invalidate();
                }
            }
        }

        public ISTScope()
        {
            DPK.eventDrawScopeLine += eventDrawDataLine;
            DPK.eventDrawScopePoint += eventDrawDataPoint;
            // DPK.eventDrawScopeYaxis += eventDrawDataYaxis;

            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            InitializeComponent();

            pramBox.DrawMode = DrawMode.OwnerDrawFixed;
            pramBox.Items.Clear();
            pramBox.BackColor = TestColor1;
            pramBox.Cursor = Cursors.Arrow;
            sp1.BackColor = TestColor1;
            Paper.Cursor = Cursors.Arrow;
            //Font
            pramBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MarkFont = new Font("宋体", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            //
            ResetLayout();  //初始化时
            base.BackColor = TestColor2;
        }

        private void ISTScope_Load(object sender, EventArgs e)
        {
            base.BorderStyle = BorderStyle.FixedSingle;
            //初始化参数设置表
            foreach (var item in SetItems)
            {
                if (!Settings.ContainsKey(item.Item1))
                {
                    Settings.Add(item.Item1, item.Item2);
                }
            }
        }

        public void Setting(string name)
        {
            if (Settings.ContainsKey(name))
            {
                Settings[name] = !Settings[name];
                Paper.Invalidate(); //重新绘制
            }
        }

        /// <summary>
        /// 清除scope的所有数据
        /// </summary>
        public void mRefresh()
        {
            DPK.Clean();  //清除数据包数据,但保留对象
            ResetLayout();
        }

        private void ResetLayout()
        {
            wave_layout_top = Layout_Head_H;
            wave_layout_bottom = Paper.Height - Layout_Tail_H;
            wave_layout_height = wave_layout_bottom - wave_layout_top;
            wave_layout_midval = wave_layout_height / 2 + Layout_Head_H;
            Paper.Invalidate();
        }

        private void SelfAdaption() //计算显示参数:WaveMidData,DataRatio
        {
            if (DPK.sDataRange < Utils.Basic.DOUBLE_DELTA)
            {
                DPK.ldratio = Math.Round(wave_layout_height / MarkFont.Height / 2, 3);
            }
            else
            {
                DPK.ldratio = Math.Round(wave_layout_height / DPK.sDataRange, 3);
            }
        }

        public void AddItems(string pname)
        {
            pramBox.Items.Add(pname);

            //调整listBox尺寸
            using (Graphics g = CreateGraphics())
            {
                var tmpw = (int)(g.MeasureString(pname, pramBox.Font).Width) + 1;
                var tmph = (int)(g.MeasureString(pname, pramBox.Font).Height);
                if (sp1.Width - sp1.SplitterDistance < tmpw)
                {
                    sp1.SplitterDistance = sp1.Width - tmpw - sp1.SplitterWidth;
                }
                pramBox.ItemHeight = tmph;
            }
            DPK.AddPack(pname); //添加到数据包 自动剔除重复的数据包
        }

        public void AddData(string name, float data, int curpage)
        {
            DPK.AddData(name, data, curpage); //添加数据

            if (Visible)
            {
                Paper.Invalidate(); //刷新显示
            }
        }

        private string ConvertToStr(double data)
        {
            var fmt = "f" + DPK.remDigit.ToString();
            return Convert.ToDouble(data).ToString(fmt);
        }

        private float GetLayoutY(double dataval)
        {
            return wave_layout_midval - (float)((dataval - DPK.sDataMid) * DPK.ldratio);
        }

        private void DrawMarkLine(Graphics g, string cog, float resety, int markpennum)
        {
            using (Brush bsh = new SolidBrush(MarkPen[markpennum].Item1))
            {
                g.DrawString(cog, MarkFont, bsh, new PointF(0, resety - MarkFont.Height / 2 + 1));
            }
            using (Pen pen = new Pen(MarkPen[markpennum].Item1, MarkPen[markpennum].Item2))
            {
                g.DrawLine(pen, wave_layout_start_x, resety, Paper.Width - Layout_Right_W, resety);
            }
        }

        private void Paper_Paint(object sender, PaintEventArgs e)
        {
            SelfAdaption();   //调整数据显示参数

            string[] STR = { "", ConvertToStr(DPK.sDataMid),
                ConvertToStr(DPK.sDataMax), ConvertToStr(DPK.sDataMin)
            };
            for (int i = 1; i < STR.Length; i++) //找到最长的字符串
            {
                if (STR[0].Length < STR[i].Length) STR[0] = STR[i];
            }
            wave_layout_start_x = e.Graphics.MeasureString(STR[0], MarkFont).Width; //计算起始列

            using (Brush bsh1 = new SolidBrush(TestColor1), bsh2 = new SolidBrush(TestColor1))
            {
                //全局填充白色
                e.Graphics.FillRectangle(bsh1, e.Graphics.ClipBounds);
                //填充刻度标尺栏
                e.Graphics.FillRectangle(bsh2, new RectangleF(new PointF(0, Layout_Head_H), new SizeF(wave_layout_start_x, wave_layout_height)));
                //右侧过渡栏
                e.Graphics.FillRectangle(bsh2, new RectangleF(new PointF(Paper.Width - Layout_Right_W, 0), new SizeF(Layout_Right_W, Paper.Height)));
                //填充顶部信息栏
                e.Graphics.FillRectangle(bsh2, new RectangleF(new PointF(0, 0), new SizeF(Paper.Width - Layout_Right_W, Layout_Head_H)));
                //填充底部信息栏
                e.Graphics.FillRectangle(bsh2, new RectangleF(new PointF(0, Layout_Head_H + wave_layout_height), new SizeF(Paper.Width - Layout_Right_W, Layout_Tail_H)));
            }

            //绘制标记信息
            if (Settings[SetItems[0].Item1])
            {
                DrawMarkLine(e.Graphics, STR[2], GetLayoutY(DPK.sDataMax), 1); //最大值标记线
            }
            if (Settings[SetItems[1].Item1])
            {
                DrawMarkLine(e.Graphics, STR[3], GetLayoutY(DPK.sDataMin), 1); //最小值标记线
            }
            //DrawMarkLine(e.Graphics, STR[1], wave_layout_midval, 0);//中线
            //DrawMarkLine(e.Graphics, "top", wave_layout_top, 0);    //顶边
            //DrawMarkLine(e.Graphics, "bot", wave_layout_bottom, 0); //底边

            //绘制所有被选数据
            for (int idx = 0; idx < DPK.sCount; idx++)
            {
                DPK.RemoveData(idx, (int)((Paper.Width - wave_layout_start_x - Layout_Right_W) / wave_layout_step)); //调整要绘制的数据
                DPK.DrawScope(idx, e.Graphics);  //绘制数据波形+点
            }
            for (int i = 0; i < 3; i++)
            {
                //e.Graphics.DrawLine(PenAssist, wave_layout_start_x + i * wave_layout_step, Layout_Head_H, wave_layout_start_x + i * wave_layout_step, Layout_Head_H + Layout_Wave_H);
            }
        }

        private void eventDrawDataLine(Graphics g, Color clr, float ox1, float oy1, float oy2)
        {
            using (Pen pen = new Pen(clr, 1))
            {
                g.DrawLine(pen, wave_layout_start_x + ox1 * wave_layout_step, GetLayoutY(oy1), wave_layout_start_x + (ox1 + 1) * wave_layout_step, GetLayoutY(oy2));
            }
        }

        private void eventDrawDataPoint(Graphics g, Color clr, float ox, float oy)
        {
            using (Brush bsh = new SolidBrush(clr))
            {
                g.FillRectangle(bsh, new RectangleF(wave_layout_start_x + ox * wave_layout_step - 2, GetLayoutY(oy) - 2, 4, 4));
            }
        }

        //private void eventDrawDataYaxis(Graphics g, float ox, float oy, string page)
        //{
        //    using (Brush bsh = new SolidBrush(PenAssist.Color))
        //    {
        //        float dpx = wave_layout_start_x + ox * wave_layout_step;
        //        float dpy = GetLayoutY(oy);
        //        g.DrawLine(PenAssist, dpx, Layout_Head_H, dpx, dpy);
        //        float wdw = g.MeasureString(page, MarkFont).Width;
        //        g.DrawString(page, MarkFont, bsh, new PointF(dpx - wdw / 2, Layout_Head_H - MarkFont.Height + 2));
        //    }
        //}

        private void Scope_SizeChanged(object sender, EventArgs e)
        {
            ResetLayout(); //窗口尺寸改变时
        }

        private void pramBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
                Brush bsh1, bsh2;
                string itemname = pramBox.Items[e.Index].ToString();
                if (DPK.isSelected(itemname))
                {
                    bsh1 = new SolidBrush(DPK.DataColor(itemname));
                    bsh2 = new SolidBrush(Color.White);
                }
                else
                {
                    bsh1 = new SolidBrush(TestColor1);
                    bsh2 = new SolidBrush(Color.Black);
                }
                e.Graphics.FillRectangle(bsh1, e.Bounds);
                e.Graphics.DrawString(itemname, e.Font, bsh2, e.Bounds, StringFormat.GenericDefault);
                bsh1.Dispose();
                bsh2.Dispose();
            }
        }

        private void pramBox_MouseClick(object sender, MouseEventArgs e)
        {
            var index = pramBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                var item = pramBox.SelectedItem.ToString();
                if (!DPK.isSelected(item))
                {
                    if (DPK.canSelect)
                    {
                        DPK.AddsPack(item);
                        pramBox.Invalidate();
                    }
                    else { }
                }
                else
                {
                    DPK.KicksPack(item);
                    pramBox.Invalidate();  //重绘listBox
                }
                Paper.Invalidate(); //重置显示
            }
            pramBox.SelectedIndex = -1;
        }

        private void Paper_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var tmp = mv_wave_layout_midval + (e.Y - mv_start.Y);
                if (tmp >= wave_layout_bottom) { wave_layout_midval = wave_layout_bottom; }
                else if (tmp <= wave_layout_top) { wave_layout_midval = wave_layout_top; }
                else { wave_layout_midval = tmp; }
                Paper.Invalidate();
            }
        }

        private void Paper_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mv_start.X = e.X;
                mv_start.Y = e.Y;
                mv_wave_layout_midval = wave_layout_midval;
            }
        }
    }
}