namespace CCS.Catcher.Controls
{
    partial class ISTScope
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                MarkFont.Dispose();
            }
            if (disposing && (MarkFont != null))
            {
                MarkFont.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.Paper = new System.Windows.Forms.PictureBox();
            this.pramBox = new System.Windows.Forms.ListBox();
            this.sp1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.Paper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).BeginInit();
            this.sp1.Panel1.SuspendLayout();
            this.sp1.Panel2.SuspendLayout();
            this.sp1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Paper
            // 
            this.Paper.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Paper.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.Paper.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Paper.Location = new System.Drawing.Point(0, 0);
            this.Paper.Name = "Paper";
            this.Paper.Size = new System.Drawing.Size(450, 307);
            this.Paper.TabIndex = 0;
            this.Paper.TabStop = false;
            this.Paper.Paint += new System.Windows.Forms.PaintEventHandler(this.Paper_Paint);
            this.Paper.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Paper_MouseDown);
            this.Paper.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Paper_MouseMove);
            // 
            // pramBox
            // 
            this.pramBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.pramBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.pramBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pramBox.FormattingEnabled = true;
            this.pramBox.IntegralHeight = false;
            this.pramBox.ItemHeight = 20;
            this.pramBox.Location = new System.Drawing.Point(0, 0);
            this.pramBox.Name = "pramBox";
            this.pramBox.Size = new System.Drawing.Size(92, 307);
            this.pramBox.TabIndex = 0;
            this.pramBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pramBox_MouseClick);
            this.pramBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.pramBox_DrawItem);
            // 
            // sp1
            // 
            this.sp1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sp1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.sp1.IsSplitterFixed = true;
            this.sp1.Location = new System.Drawing.Point(-1, 36);
            this.sp1.Name = "sp1";
            // 
            // sp1.Panel1
            // 
            this.sp1.Panel1.Controls.Add(this.Paper);
            // 
            // sp1.Panel2
            // 
            this.sp1.Panel2.Controls.Add(this.pramBox);
            this.sp1.Size = new System.Drawing.Size(543, 307);
            this.sp1.SplitterDistance = 450;
            this.sp1.SplitterWidth = 1;
            this.sp1.TabIndex = 1;
            // 
            // ISTScope
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.sp1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ISTScope";
            this.Size = new System.Drawing.Size(541, 342);
            this.Load += new System.EventHandler(this.ISTScope_Load);
            this.SizeChanged += new System.EventHandler(this.Scope_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.Paper)).EndInit();
            this.sp1.Panel1.ResumeLayout(false);
            this.sp1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sp1)).EndInit();
            this.sp1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Paper;
        private System.Windows.Forms.ListBox pramBox;
        private System.Windows.Forms.SplitContainer sp1;
    }
}
