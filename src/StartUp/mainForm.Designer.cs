using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace IDevTrack.StartUp
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainForm));
            this.toolStripButton5 = new System.Windows.Forms.ToolStripSeparator();
            this.sp1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolBar = new System.Windows.Forms.ToolStrip();
            this.ToolOpen = new System.Windows.Forms.ToolStripButton();
            this.ToolShotPic = new System.Windows.Forms.ToolStripButton();
            this.ToolPen = new System.Windows.Forms.ToolStripButton();
            this.ToolPen2 = new System.Windows.Forms.ToolStripButton();
            this.ToolLine = new System.Windows.Forms.ToolStripButton();
            this.ToolUndo = new System.Windows.Forms.ToolStripButton();
            this.ToolRedo = new System.Windows.Forms.ToolStripButton();
            this.ToolColor = new IDevTrack.STReader.ColorButton();
            this.sp2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolSetFps = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolFast1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolFast2 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolFast3 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolFast4 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolFast5 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolPlay = new System.Windows.Forms.ToolStripSplitButton();
            this.ClsBeforeRun = new System.Windows.Forms.ToolStripMenuItem();
            this.DirectRun = new System.Windows.Forms.ToolStripMenuItem();
            this.sp3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolGrid = new System.Windows.Forms.ToolStripButton();
            this.ToolAnls = new System.Windows.Forms.ToolStripSplitButton();
            this.sp4 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolScope = new System.Windows.Forms.ToolStripSplitButton();
            this.sp5 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolAbout = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton9 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton10 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.StatusBar = new System.Windows.Forms.StatusStrip();
            this.StatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new IDevTrack.IControls.ITabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.stReader1 = new IDevTrack.STReader.ISTReader();
            this.stScope1 = new IDevTrack.STScope.ISTScope();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.ToolBar.SuspendLayout();
            this.StatusBar.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(6, 25);
            // 
            // sp1
            // 
            this.sp1.Name = "sp1";
            this.sp1.Size = new System.Drawing.Size(6, 27);
            // 
            // ToolBar
            // 
            this.ToolBar.BackColor = System.Drawing.SystemColors.Control;
            this.ToolBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolOpen,
            this.ToolShotPic,
            this.sp1,
            this.ToolPen,
            this.ToolPen2,
            this.ToolLine,
            this.ToolUndo,
            this.ToolRedo,
            this.ToolColor,
            this.sp2,
            this.ToolSetFps,
            this.ToolPlay,
            this.sp3,
            this.ToolGrid,
            this.ToolAnls,
            this.sp4,
            this.ToolScope,
            this.sp5,
            this.ToolAbout,
            this.toolStripButton1});
            this.ToolBar.Location = new System.Drawing.Point(0, 0);
            this.ToolBar.Name = "ToolBar";
            this.ToolBar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.ToolBar.Size = new System.Drawing.Size(889, 27);
            this.ToolBar.TabIndex = 0;
            this.ToolBar.Text = "工具栏";
            this.ToolBar.Paint += new System.Windows.Forms.PaintEventHandler(this.ToolBar_Paint);
            // 
            // ToolOpen
            // 
            this.ToolOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolOpen.Image = ((System.Drawing.Image)(resources.GetObject("ToolOpen.Image")));
            this.ToolOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolOpen.Name = "ToolOpen";
            this.ToolOpen.Size = new System.Drawing.Size(29, 24);
            this.ToolOpen.Text = "openswcode";
            this.ToolOpen.ToolTipText = "打开文件";
            this.ToolOpen.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolShotPic
            // 
            this.ToolShotPic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolShotPic.Image = ((System.Drawing.Image)(resources.GetObject("ToolShotPic.Image")));
            this.ToolShotPic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolShotPic.Name = "ToolShotPic";
            this.ToolShotPic.Size = new System.Drawing.Size(29, 24);
            this.ToolShotPic.Text = "_shotpic";
            this.ToolShotPic.ToolTipText = "截图";
            this.ToolShotPic.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolPen
            // 
            this.ToolPen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolPen.Image = ((System.Drawing.Image)(resources.GetObject("ToolPen.Image")));
            this.ToolPen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolPen.Name = "ToolPen";
            this.ToolPen.Size = new System.Drawing.Size(29, 24);
            this.ToolPen.Text = "pen";
            this.ToolPen.ToolTipText = "画笔1";
            this.ToolPen.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolPen2
            // 
            this.ToolPen2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolPen2.Image = ((System.Drawing.Image)(resources.GetObject("ToolPen2.Image")));
            this.ToolPen2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolPen2.Name = "ToolPen2";
            this.ToolPen2.Size = new System.Drawing.Size(29, 24);
            this.ToolPen2.Text = "Pen2";
            this.ToolPen2.ToolTipText = "画笔2";
            this.ToolPen2.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolLine
            // 
            this.ToolLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolLine.Image = ((System.Drawing.Image)(resources.GetObject("ToolLine.Image")));
            this.ToolLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolLine.Name = "ToolLine";
            this.ToolLine.Size = new System.Drawing.Size(29, 24);
            this.ToolLine.Text = "line";
            this.ToolLine.ToolTipText = "画笔3";
            this.ToolLine.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolUndo
            // 
            this.ToolUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolUndo.Image = ((System.Drawing.Image)(resources.GetObject("ToolUndo.Image")));
            this.ToolUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolUndo.Name = "ToolUndo";
            this.ToolUndo.Size = new System.Drawing.Size(29, 24);
            this.ToolUndo.Text = "_undo";
            this.ToolUndo.ToolTipText = "撤销";
            this.ToolUndo.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolRedo
            // 
            this.ToolRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolRedo.Image = ((System.Drawing.Image)(resources.GetObject("ToolRedo.Image")));
            this.ToolRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolRedo.Name = "ToolRedo";
            this.ToolRedo.Size = new System.Drawing.Size(29, 24);
            this.ToolRedo.Text = "_redo";
            this.ToolRedo.ToolTipText = "重做";
            this.ToolRedo.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolColor
            // 
            this.ToolColor.AutoSize = false;
            this.ToolColor.BackColor = System.Drawing.Color.Green;
            this.ToolColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolColor.Name = "ToolColor";
            this.ToolColor.ShowDropDownArrow = false;
            this.ToolColor.Size = new System.Drawing.Size(22, 22);
            this.ToolColor.Text = "color";
            this.ToolColor.ToolTipText = "画笔颜色";
            this.ToolColor.ColorChanged += new System.EventHandler(this.ToolBar_stReader_ColorChanged);
            // 
            // sp2
            // 
            this.sp2.Name = "sp2";
            this.sp2.Size = new System.Drawing.Size(6, 27);
            // 
            // ToolSetFps
            // 
            this.ToolSetFps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ToolSetFps.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolFast1,
            this.ToolFast2,
            this.ToolFast3,
            this.ToolFast4,
            this.ToolFast5});
            this.ToolSetFps.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolSetFps.Name = "ToolSetFps";
            this.ToolSetFps.ShowDropDownArrow = false;
            this.ToolSetFps.Size = new System.Drawing.Size(64, 24);
            this.ToolSetFps.Text = "Fps: 20";
            this.ToolSetFps.ToolTipText = "帧速设置";
            // 
            // ToolFast1
            // 
            this.ToolFast1.Name = "ToolFast1";
            this.ToolFast1.Size = new System.Drawing.Size(162, 26);
            this.ToolFast1.Text = "较快";
            this.ToolFast1.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolFast2
            // 
            this.ToolFast2.Name = "ToolFast2";
            this.ToolFast2.Size = new System.Drawing.Size(162, 26);
            this.ToolFast2.Text = "较快(微调)";
            this.ToolFast2.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolFast3
            // 
            this.ToolFast3.Name = "ToolFast3";
            this.ToolFast3.Size = new System.Drawing.Size(162, 26);
            this.ToolFast3.Text = "普通";
            this.ToolFast3.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolFast4
            // 
            this.ToolFast4.Name = "ToolFast4";
            this.ToolFast4.Size = new System.Drawing.Size(162, 26);
            this.ToolFast4.Text = "较慢(微调)";
            this.ToolFast4.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolFast5
            // 
            this.ToolFast5.Name = "ToolFast5";
            this.ToolFast5.Size = new System.Drawing.Size(162, 26);
            this.ToolFast5.Text = "较慢";
            this.ToolFast5.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolPlay
            // 
            this.ToolPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolPlay.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClsBeforeRun,
            this.DirectRun});
            this.ToolPlay.Image = ((System.Drawing.Image)(resources.GetObject("ToolPlay.Image")));
            this.ToolPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolPlay.Name = "ToolPlay";
            this.ToolPlay.Size = new System.Drawing.Size(39, 24);
            this.ToolPlay.Tag = "0";
            this.ToolPlay.Text = "_toolplay";
            this.ToolPlay.ToolTipText = "运行/停止[space]";
            this.ToolPlay.ButtonClick += new System.EventHandler(this.ToolBar_stReader_Click);
            this.ToolPlay.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ToolPlay_DropDownItemClicked);
            // 
            // ClsBeforeRun
            // 
            this.ClsBeforeRun.Checked = true;
            this.ClsBeforeRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ClsBeforeRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ClsBeforeRun.Name = "ClsBeforeRun";
            this.ClsBeforeRun.Size = new System.Drawing.Size(182, 26);
            this.ClsBeforeRun.Text = "运行前刷新";
            // 
            // DirectRun
            // 
            this.DirectRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.DirectRun.Name = "DirectRun";
            this.DirectRun.Size = new System.Drawing.Size(182, 26);
            this.DirectRun.Text = "运行前不刷新";
            // 
            // sp3
            // 
            this.sp3.Name = "sp3";
            this.sp3.Size = new System.Drawing.Size(6, 27);
            // 
            // ToolGrid
            // 
            this.ToolGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolGrid.Image = ((System.Drawing.Image)(resources.GetObject("ToolGrid.Image")));
            this.ToolGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolGrid.Name = "ToolGrid";
            this.ToolGrid.Size = new System.Drawing.Size(29, 24);
            this.ToolGrid.Text = "_toolgrid";
            this.ToolGrid.ToolTipText = "网格线";
            this.ToolGrid.Click += new System.EventHandler(this.ToolBar_stReader_Click);
            // 
            // ToolAnls
            // 
            this.ToolAnls.BackColor = System.Drawing.SystemColors.Control;
            this.ToolAnls.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolAnls.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.ToolAnls.Image = global::IDevTrack.Properties.Resources.analysis;
            this.ToolAnls.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolAnls.Name = "ToolAnls";
            this.ToolAnls.Size = new System.Drawing.Size(39, 24);
            this.ToolAnls.Text = "toolanls";
            this.ToolAnls.ToolTipText = "重置分析包";
            this.ToolAnls.ButtonClick += new System.EventHandler(this.ToolAnls_ButtonClick);
            // 
            // sp4
            // 
            this.sp4.Name = "sp4";
            this.sp4.Size = new System.Drawing.Size(6, 27);
            // 
            // ToolScope
            // 
            this.ToolScope.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolScope.Image = global::IDevTrack.Properties.Resources.scope;
            this.ToolScope.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolScope.Name = "ToolScope";
            this.ToolScope.Size = new System.Drawing.Size(39, 24);
            this.ToolScope.Text = "toolStripButton1";
            this.ToolScope.ToolTipText = "示波器";
            this.ToolScope.ButtonClick += new System.EventHandler(this.ToolScope_ButtonClick);
            // 
            // sp5
            // 
            this.sp5.Name = "sp5";
            this.sp5.Size = new System.Drawing.Size(6, 27);
            // 
            // ToolAbout
            // 
            this.ToolAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolAbout.Image = ((System.Drawing.Image)(resources.GetObject("ToolAbout.Image")));
            this.ToolAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ToolAbout.Name = "ToolAbout";
            this.ToolAbout.Size = new System.Drawing.Size(29, 24);
            this.ToolAbout.Text = "about";
            this.ToolAbout.ToolTipText = "关于/反馈";
            this.ToolAbout.Click += new System.EventHandler(this.ToolAbout_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(29, 24);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton8.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton8.Image")));
            this.toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton8.Text = "toolStripButton8";
            // 
            // toolStripButton9
            // 
            this.toolStripButton9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton9.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton9.Image")));
            this.toolStripButton9.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton9.Name = "toolStripButton9";
            this.toolStripButton9.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton9.Text = "toolStripButton9";
            // 
            // toolStripButton10
            // 
            this.toolStripButton10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton10.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton10.Image")));
            this.toolStripButton10.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton10.Name = "toolStripButton10";
            this.toolStripButton10.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton10.Text = "toolStripButton10";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "toolStripButton4";
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton7.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton7.Image")));
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton7.Text = "toolStripButton7";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton6.Text = "toolStripButton6";
            // 
            // StatusBar
            // 
            this.StatusBar.BackColor = System.Drawing.SystemColors.Window;
            this.StatusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.StatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel1});
            this.StatusBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.StatusBar.Location = new System.Drawing.Point(0, 559);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.StatusBar.Size = new System.Drawing.Size(889, 26);
            this.StatusBar.TabIndex = 1;
            this.StatusBar.Text = "statusStrip1";
            // 
            // StatusLabel1
            // 
            this.StatusLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StatusLabel1.Name = "StatusLabel1";
            this.StatusLabel1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.StatusLabel1.Size = new System.Drawing.Size(152, 20);
            this.StatusLabel1.Text = "待导入swcode文件...";
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft YaHei UI", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(0, 27);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(889, 532);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.tabPage1.Controls.Add(this.stReader1);
            this.tabPage1.Controls.Add(this.stScope1);
            this.tabPage1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage1.Location = new System.Drawing.Point(26, 0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(863, 532);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "MONITOR";
            this.tabPage1.SizeChanged += new System.EventHandler(this.tabPage1_SizeChanged);
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // stReader1
            // 
            this.stReader1.AllowDrop = true;
            this.stReader1.BackColor = System.Drawing.SystemColors.Control;
            this.stReader1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stReader1.Location = new System.Drawing.Point(46, 152);
            this.stReader1.Margin = new System.Windows.Forms.Padding(0);
            this.stReader1.MinimumSize = new System.Drawing.Size(120, 73);
            this.stReader1.Name = "stReader1";
            this.stReader1.PenColor = System.Drawing.Color.Green;
            this.stReader1.Size = new System.Drawing.Size(485, 302);
            this.stReader1.TabIndex = 0;
            this.stReader1.eventSRCFileChanged += new IDevTrack.STReader.ISTReader.delegateSRCFileChanged(this.stReader1_eventSRCFileChanged);
            this.stReader1.eventPlayChanged += new IDevTrack.STReader.ISTReader.delegatePlayChanged(this.stReader1_eventPlayChanged);
            this.stReader1.SizeChanged += new System.EventHandler(this.stReader1_SizeChanged);
            // 
            // stScope1
            // 
            this.stScope1.BackColor = System.Drawing.SystemColors.Control;
            this.stScope1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.stScope1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stScope1.Location = new System.Drawing.Point(303, 15);
            this.stScope1.Name = "stScope1";
            this.stScope1.Size = new System.Drawing.Size(548, 265);
            this.stScope1.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Location = new System.Drawing.Point(26, 0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(863, 532);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "CODING";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Location = new System.Drawing.Point(26, 0);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(863, 532);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "待开发...";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Consolas", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(248, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(218, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "待开发...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(889, 585);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.ToolBar);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "mainForm";
            this.Text = "mainForm";
            this.Load += new System.EventHandler(this.mainForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mainForm_KeyDown);
            this.ToolBar.ResumeLayout(false);
            this.ToolBar.PerformLayout();
            this.StatusBar.ResumeLayout(false);
            this.StatusBar.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripSeparator toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripButton toolStripButton8;
        private System.Windows.Forms.ToolStripButton toolStripButton9;
        private System.Windows.Forms.ToolStripButton toolStripButton10;
        private System.Windows.Forms.ToolStripButton ToolOpen;
        private System.Windows.Forms.ToolStripSeparator sp1;
        private System.Windows.Forms.ToolStrip ToolBar;
        private System.Windows.Forms.ToolStripButton ToolAbout;
        private System.Windows.Forms.StatusStrip StatusBar;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel1;
        private IControls.ITabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private STScope.ISTScope stScope1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton ToolShotPic;
        private System.Windows.Forms.ToolStripButton ToolPen;
        private System.Windows.Forms.ToolStripButton ToolPen2;
        private System.Windows.Forms.ToolStripButton ToolLine;
        private System.Windows.Forms.ToolStripButton ToolUndo;
        private System.Windows.Forms.ToolStripButton ToolRedo;
        private STReader.ColorButton ToolColor;
        private System.Windows.Forms.ToolStripSeparator sp2;
        private System.Windows.Forms.ToolStripDropDownButton ToolSetFps;
        private System.Windows.Forms.ToolStripMenuItem ToolFast1;
        private System.Windows.Forms.ToolStripMenuItem ToolFast2;
        private System.Windows.Forms.ToolStripMenuItem ToolFast3;
        private System.Windows.Forms.ToolStripMenuItem ToolFast4;
        private System.Windows.Forms.ToolStripMenuItem ToolFast5;
        private System.Windows.Forms.ToolStripSplitButton ToolPlay;
        private System.Windows.Forms.ToolStripMenuItem ClsBeforeRun;
        private System.Windows.Forms.ToolStripMenuItem DirectRun;
        private System.Windows.Forms.ToolStripSeparator sp3;
        private System.Windows.Forms.ToolStripButton ToolGrid;
        private System.Windows.Forms.ToolStripSeparator sp4;
        private STReader.ISTReader stReader1;
        private ToolStripSeparator sp5;
        private ToolStripButton toolStripButton1;
        private ToolStripSplitButton ToolAnls;
        private ToolStripSplitButton ToolScope;
    }
}