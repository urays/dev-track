namespace CCS
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.istScope1 = new CCS.Catcher.Controls.ISTScope();
            this.pageBox1 = new CCS.Catcher.Controls.PageBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(377, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "读入文件";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // istScope1
            // 
            this.istScope1.BackColor = System.Drawing.SystemColors.Control;
            this.istScope1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.istScope1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.istScope1.Location = new System.Drawing.Point(116, 224);
            this.istScope1.Name = "istScope1";
            this.istScope1.Size = new System.Drawing.Size(353, 202);
            this.istScope1.TabIndex = 3;
            this.istScope1.Visible = false;
            // 
            // pageBox1
            // 
            this.pageBox1.AllowDrop = true;
            this.pageBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pageBox1.BackColor = System.Drawing.SystemColors.Control;
            this.pageBox1.BorderColor = System.Drawing.Color.Red;
            this.pageBox1.BorderWidth = 2;
            this.pageBox1.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pageBox1.Location = new System.Drawing.Point(0, 31);
            this.pageBox1.MinimumSize = new System.Drawing.Size(120, 120);
            this.pageBox1.Name = "pageBox1";
            this.pageBox1.Size = new System.Drawing.Size(678, 446);
            this.pageBox1.TabIndex = 1;
            this.pageBox1.Text = "pageBox1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(678, 477);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.istScope1);
            this.Controls.Add(this.pageBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private Catcher.Controls.PageBox pageBox1;
        private Catcher.Controls.ISTScope istScope1;
        private System.Windows.Forms.Button button1;
    }
}