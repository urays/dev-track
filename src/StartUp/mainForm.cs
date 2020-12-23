using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace IDevTrack.StartUp
{
    public partial class mainForm : Form
    {
        #region Fields

        private readonly List<IControls.PickBoxs> CAL = new List<IControls.PickBoxs>(); //Control Action List

        #endregion Fields

        #region Layout

        private void InitSelfLayout()
        {
            StatusBar.BackColor = SystemColors.Window;

            Text = "DevTrack";
            MinimumSize = new Size(16 + stReader1.MinimumSize.Width, 39 + stReader1.MinimumSize.Height + ToolBar.Height);
            KeyPreview = true;

            ToolAnls.Enabled = stReader1.isValidState;
            ToolGrid.Enabled = stReader1.isValidState;
            ToolPen2.Enabled = stReader1.isValidState;

            ToolPlay.Enabled = false;
            ToolAnls.DropDownItems.Clear();
            ToolScope.DropDownItems.Clear();

            stScope1.Visible = false;
            ToolScope.ToolTipText = "开启Scoper";

            //Font
            Font font1 = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Font font2 = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ToolSetFps.Font = font2;
            ToolPlay.Font = font2;
            ToolPlay.DropDown.Font = ToolPlay.Font;
            ToolAnls.Font = font1;
            ToolAnls.DropDown.Font = ToolAnls.Font;
            ToolScope.Font = font1;
            ToolScope.DropDown.Font = ToolScope.Font;
            //
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x112)  //用于解决窗口最大化时,工具栏闪烁问题
            {
                if (m.WParam.ToInt32() == 0XF030) //最大化
                {
                    Visible = false;
                    SuspendLayout();
                    WindowState = FormWindowState.Maximized;
                    ResumeLayout(true);  //在最大化之前执行,尺寸以达最大,不需要再调整
                    Visible = true;
                }
                else if (m.WParam.ToInt32() == 0XF020) { } //最小化
                else if (m.WParam.ToInt32() == 0xF060) { } //关闭
            }
            base.WndProc(ref m);
        }

        #endregion Layout

        #region MainForm

        public mainForm()
        {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            InitializeComponent();
            InitSelfLayout();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            IDevTrack.Utils.Notify.register(this); //通知显示地址注册
            //IDevTrack.Utils.Notify.Send(2, "许可期:" + Utils.Licence.GetValidDayMark());

            //初始化载入scoper设置选项
            foreach (var item in stScope1.SetItems)
            {
                ToolScope.DropDownItems.Add(item.Item1, null, ScopeSet_Click);
                foreach (ToolStripMenuItem ele in ToolScope.DropDownItems)
                {
                    if (ele.Text == item.Item1)
                    {
                        ele.Checked = item.Item2;
                        break;
                    }
                }
            }
            //
            Utils.AnlsPort.Init(stScope1.AddItems, stScope1.AddData, ToolBar_AddLineData);//初始化数据中转站
            Utils.AnlsPort.MatchOBJ(); //匹配传送数据对象

            Utils.AnlsPort.ReadInfo(); //读取分析包类别和版本
            Text = Text + " - PKG:[" + Utils.AnlsPort.Version + "]";
            //
            CAL.Add(new IControls.PickBoxs(stReader1, 240, 168));
            CAL.Add(new IControls.PickBoxs(stScope1, 428, 216));
#if !DEBUG
            //检查更新
            IDevTrack.Update.UTools.ExistUpdate(new IDevTrack.Update.UTools.delegateCallBack(CallBack_ExistUpdate));
#endif
        }

        private void CallBack_ExistUpdate(IAsyncResult iar)
        {
            string[] lst = IDevTrack.Update.UTools.UpdateList();
            if (lst.Length > 1)
            {
                Enabled = false;
                IDevTrack.Update.UForm updateForm = new IDevTrack.Update.UForm(lst);
                DialogResult msg = updateForm.ShowDialog(this);
                updateForm.Dispose();
                if (msg == DialogResult.OK && File.Exists(IDevTrack.Update.Settings.UpdateExe))
                {
                    System.Diagnostics.Process.Start(IDevTrack.Update.Settings.UpdateExe, "u");
                    Application.Exit();
                }
                else { Enabled = true; }
            }
        }

        private void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0) //Monitor Interface
            {
                switch (e.KeyCode)
                {
                    case (Keys.Space):
                        stReader1.ToolsFunc("ToolPlay");
                        break;
                }
            }
        }

        #endregion MainForm

        #region stReader

        private void stReader1_eventPlayChanged(bool isplay)
        {
            if (isplay)  //running
            {
                ToolPlay.Tag = 1;
                ToolPlay.Image = global::IDevTrack.Properties.Resources.stop;
            }
            else
            {
                ToolPlay.Tag = 0;
                ToolPlay.Image = global::IDevTrack.Properties.Resources.play;
            }
        }

        private void stReader1_eventSRCFileChanged(string newfilename)
        {
            StatusLabel1.Text = newfilename;
            ToolPlay.Enabled = true;
        }

        private void stReader1_SizeChanged(object sender, EventArgs e)
        {
            //Debug.WriteLine(stReader1.Width.ToString() + " " + stReader1.Height.ToString());
            //Debug.WriteLine(stScope1.Width.ToString() + " " + stScope1.Height.ToString());

            ToolAnls.Enabled = stReader1.isValidState;
            ToolGrid.Enabled = stReader1.isValidState;
            ToolPen2.Enabled = stReader1.isValidState;

            if (stReader1.isValidState)
            {
                ToolPen2.ToolTipText = "画笔2";
                if (stReader1.isEnGrid)
                    ToolGrid.ToolTipText = "关闭网格";
                else
                    ToolGrid.ToolTipText = "打开网格";
                ToolAnls.ToolTipText = "刷新分析包";
            }
            else
            {
                ToolPen2.ToolTipText = "禁止启用";
                ToolGrid.ToolTipText = "禁止启用";
                ToolAnls.ToolTipText = "禁止启用";
            }
            //System.Diagnostics.Debug.WriteLine(spt2.SplitterDistance.ToString() + " - " + spt1.SplitterDistance.ToString());
        }

        #endregion stReader

        #region ToolBar

        private void ToolScope_ButtonClick(object sender, EventArgs e)
        {
            CAL[1].Remove();  //清除Pickboxs
            stScope1.Visible = !stScope1.Visible;
            if (stScope1.Visible)
            {
                ToolScope.ToolTipText = "关闭Scoper";
                stScope1.BringToFront();
            }
            else
            {
                ToolScope.ToolTipText = "开启Scoper";
            }
        }

        private void ToolAnls_ButtonClick(object sender, EventArgs e)
        {
            Utils.AnlsPort.mRefresh(); //分析函数内部刷新
            stScope1.mRefresh();       //清除所有数据,但保留对象
            stReader1.mRefresh();      //重置显示并运行分析函数
            IDevTrack.Utils.Notify.Send(2, "分析包已重置");
        }

        private void ToolBar_stReader_Click(object sender, EventArgs e)
        {
            ToolStripItem obj = sender as ToolStripItem;
            if (obj.Name == "ToolPlay") //运行停止 特殊处理
            {
                if (ClsBeforeRun.Checked && (int)obj.Tag == 0)
                { // 0 - current status:stop
                    ToolAnls_ButtonClick(null, null);
                }
            }

            if (obj.Name.Substring(0, obj.Name.Length - 1) == "ToolFast")
            {
                ToolSetFps.Text = "Fps: " + stReader1.SetFps(obj.Name);
            }
            else
            {
                stReader1.ToolsFunc(obj.Name);
                stReader1_SizeChanged(null, null);
            }
        }

        private void ToolPlay_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            (e.ClickedItem as ToolStripMenuItem).Checked = true;
            foreach (ToolStripMenuItem item in ToolPlay.DropDownItems)
            {
                if (item.Name != e.ClickedItem.Name)
                {
                    item.Checked = false;
                }
            }
        }

        private void ToolBar_stReader_ColorChanged(object sender, EventArgs e)
        {
            stReader1.PenColor = (sender as IDevTrack.STReader.ColorButton).Color;
        }

        private void ToolBar_Paint(object sender, PaintEventArgs e)
        {
            if ((sender as ToolStrip).RenderMode == ToolStripRenderMode.System)
            {
                Rectangle rect = new Rectangle(0, 0, ToolBar.Width, ToolBar.Height - 2);
                e.Graphics.SetClip(rect);
            }
        }

        private void ToolBar_AddLineData(string lname, Color clr, int idx)
        {
            ToolAnls.DropDownItems.Add(lname, null, ToolBar_AnlsClick);
            foreach (ToolStripMenuItem ele in ToolAnls.DropDownItems)
            {
                if (ele.Text == lname)
                {
                    ele.Checked = true;
                    ele.ForeColor = clr;
                    ele.Tag = idx;
                    break;
                }
            }
        }

        private void ToolBar_AnlsClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            string itemname = item.Text;
            if (item.Checked)
            {
                Utils.AnlsPort.DisLineData((int)(item.Tag));
            }
            else
            {
                Utils.AnlsPort.EnLineData((int)(item.Tag));
            }
            item.Checked = !item.Checked;
            stReader1.mRefresh(); //重绘stReader1.Paper
        }

        private void ScopeSet_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            stScope1.Setting(item.Text);
            item.Checked = !item.Checked;
        }

        #endregion ToolBar

        #region TabControl

        private void tabPage1_Click(object sender, EventArgs e)
        {
            foreach (IControls.PickBoxs x in CAL)
            {
                x.Remove();
            }
            stReader1.ToolsFunc("ToolSlide"); //放下画笔(如果有)
        }

        private void tabPage1_SizeChanged(object sender, EventArgs e)
        {
            foreach (IControls.PickBoxs x in CAL)
            {
                x.Remove();
                x.DealOutBounds();
            }
        }

        #endregion TabControl

        #region About

        private void ToolAbout_Click(object sender, EventArgs e)
        {
            aboutForm aboutform = new aboutForm();
            aboutform.MailSendTo = Config.Cfg.ReceiveEmail;
            aboutform.ShowDialog();
            aboutform.Dispose();
        }

        #endregion About
    }
}