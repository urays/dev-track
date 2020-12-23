namespace IDevTrack.STReader
{
    partial class ISTReader
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
            this.components = new System.ComponentModel.Container();
            this.PlayTimer = new System.Windows.Forms.Timer(this.components);
            this.Paper = new System.Windows.Forms.PictureBox();
            this.Mvblock = new System.Windows.Forms.Panel();
            this.NoticeBoard = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Paper)).BeginInit();
            this.SuspendLayout();
            // 
            // PlayTimer
            // 
            this.PlayTimer.Interval = 10;
            this.PlayTimer.Tick += new System.EventHandler(this.PlayTimer_Tick);
            // 
            // Paper
            // 
            this.Paper.BackColor = System.Drawing.Color.SkyBlue;
            this.Paper.Cursor = System.Windows.Forms.Cursors.Default;
            this.Paper.Location = new System.Drawing.Point(35, 32);
            this.Paper.Margin = new System.Windows.Forms.Padding(0);
            this.Paper.Name = "Paper";
            this.Paper.Size = new System.Drawing.Size(400, 214);
            this.Paper.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Paper.TabIndex = 3;
            this.Paper.TabStop = false;
            this.Paper.Paint += new System.Windows.Forms.PaintEventHandler(this.Paper_Paint);
            this.Paper.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Paper_MouseDown);
            this.Paper.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Paper_MouseMove);
            this.Paper.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Paper_MouseUp);
            this.Paper.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Paper_MouseWheel);
            // 
            // Mvblock
            // 
            this.Mvblock.BackColor = System.Drawing.SystemColors.HotTrack;
            this.Mvblock.Location = new System.Drawing.Point(35, 260);
            this.Mvblock.Margin = new System.Windows.Forms.Padding(0);
            this.Mvblock.Name = "Mvblock";
            this.Mvblock.Size = new System.Drawing.Size(77, 6);
            this.Mvblock.TabIndex = 5;
            // 
            // NoticeBoard
            // 
            this.NoticeBoard.AutoSize = true;
            this.NoticeBoard.BackColor = System.Drawing.Color.White;
            this.NoticeBoard.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NoticeBoard.Location = new System.Drawing.Point(0, 260);
            this.NoticeBoard.Margin = new System.Windows.Forms.Padding(0);
            this.NoticeBoard.Name = "NoticeBoard";
            this.NoticeBoard.Size = new System.Drawing.Size(79, 20);
            this.NoticeBoard.TabIndex = 6;
            this.NoticeBoard.Text = "[724/724]";
            // 
            // ISTReader
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.NoticeBoard);
            this.Controls.Add(this.Mvblock);
            this.Controls.Add(this.Paper);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(480, 225);
            this.Name = "ISTReader";
            this.Size = new System.Drawing.Size(480, 281);
            this.Load += new System.EventHandler(this.STReader_Load);
            this.SizeChanged += new System.EventHandler(this.STReader_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ISTReader_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ISTReader_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.Paper)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer PlayTimer;
        private System.Windows.Forms.Panel Mvblock;
        private System.Windows.Forms.PictureBox Paper;
        private System.Windows.Forms.Label NoticeBoard;
    }
}