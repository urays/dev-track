using System;
using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.IControls
{
    public class PickBoxs : IDisposable
    {
        #region Fields

        private const int BOX_SIZE = 9;  //调整大小触模柄方框大小
        private readonly Color BOX_COLOR = Color.White;

        private readonly int min_w = 100, min_h = 100;
        private readonly IDevTrack.IControls.IUserControl m_control = null;
        private bool ismousedown = false;
        private bool dragging = false;
        private Point startpoint;
        private readonly Cursor oldCursor;
        private readonly Label[] lbl = new Label[8];
        private int sl, st, sw, sh;

        private readonly Cursor[] arrArrow = new Cursor[]
        {
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE,
            Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE,
        };

        #endregion Fields

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (m_control != null))
            {
                m_control.Dispose();
            }
        }

        #endregion Dispose

        #region Constructor

        public PickBoxs(IDevTrack.IControls.IUserControl ctl, int minw, int minh)
        {
            min_w = minw;
            min_h = minh;
            for (int i = 0; i < 8; i++)
            {
                lbl[i] = new Label();
                lbl[i].TabIndex = i;
                lbl[i].FlatStyle = 0;
                lbl[i].BorderStyle = BorderStyle.FixedSingle;
                lbl[i].BackColor = BOX_COLOR;
                lbl[i].Cursor = arrArrow[i];
                lbl[i].Text = "";
                lbl[i].BringToFront();
                lbl[i].MouseDown += new MouseEventHandler(hdl_MouseDown);
                lbl[i].MouseMove += new MouseEventHandler(hdl_MouseMove);
                lbl[i].MouseUp += new MouseEventHandler(hdl_MouseUp);
            }

            m_control = ctl;
            if (m_control != null)
            {
                m_control.MouseDown += new MouseEventHandler(ctl_MouseDown);
                m_control.MouseUp += new MouseEventHandler(ctl_MouseUp);
                m_control.MouseMove += new MouseEventHandler(ctl_MouseMove);
                m_control.MouseClick += new MouseEventHandler(ctl_MouseClick);
                m_control.ToFrontLater += new IUserControl.delegateBringToFrontLater(ControlToFrontLater);

                for (int i = 0; i < 8; i++)
                {
                    m_control.Parent.Controls.Add(lbl[i]);
                    lbl[i].BringToFront();
                }
                oldCursor = m_control.Cursor;
            }
            HideHandles();
        }

        private void ControlToFrontLater()
        {
            for (int i = 0; i < 8; i++)
            {
                lbl[i].BringToFront();
            }
        }

        public void Remove()
        {
            HideHandles();
            m_control.Cursor = oldCursor;
        }

        ~PickBoxs()
        {
            Dispose(false);
        }

        #endregion Constructor

        #region Control Event

        private void ctl_MouseClick(object sender, MouseEventArgs e)
        {
            //清除显示-其它选中的组件
            MoveHandles();

            for (int i = 0; i < (sender as Control).Parent.Controls.Count; i++)
            {
                if ((sender as Control).Parent.Controls[i].Text.Trim().Length == 0
                    && m_control.Parent.Controls[i] is Label)
                {
                    (sender as Control).Parent.Controls[i].Visible = false;
                }
            }
        }

        private void ctl_MouseDown(object sender, MouseEventArgs e)
        {
            ismousedown = true;
            startpoint = new Point(e.X, e.Y);

            m_control.BringToFront(); //将选中的控件置于顶层
            for (int i = 0; i < 8; i++)
            {
                m_control.Parent.Controls.Add(lbl[i]);
                lbl[i].BringToFront();
            }
            HideHandles();
        }

        private void ctl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ismousedown = false;
            MoveHandles();
            ShowHandles();
            m_control.Visible = true;
        }

        private void ctl_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (ismousedown)
            {
                int l = m_control.Left + (e.X - startpoint.X);
                int t = m_control.Top + (e.Y - startpoint.Y);
                int w = m_control.Width;
                int h = m_control.Height;
                int pa_w = m_control.Parent.ClientRectangle.Width;
                int pa_h = m_control.Parent.ClientRectangle.Height;
                l = (l < BOX_SIZE) ? BOX_SIZE : ((l + w > pa_w - BOX_SIZE) ? pa_w - w - BOX_SIZE : l);
                t = (t < BOX_SIZE) ? BOX_SIZE : ((t + h > pa_h - BOX_SIZE) ? pa_h - h - BOX_SIZE : t);
                m_control.Left = l;
                m_control.Top = t;
            }
            m_control.Cursor = Cursors.SizeAll;
        }

        #endregion Control Event

        #region Box Event

        private void hdl_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            sl = m_control.Left;
            st = m_control.Top;
            sw = m_control.Width;
            sh = m_control.Height;
            HideHandles();
            //m_control.Visible = false;
        }

        private void hdl_MouseMove(object sender, MouseEventArgs e)
        {
            //   0  1  2
            //   7     3
            //   6  5  4
            int l = m_control.Left;
            int w = m_control.Width;
            int t = m_control.Top;
            int h = m_control.Height;
            int pa_w = m_control.Parent.ClientRectangle.Width;
            int pa_h = m_control.Parent.ClientRectangle.Height;
            if (dragging)
            {
                switch (((Label)sender).TabIndex)
                {
                    case 0:
                        l = sl + e.X < sl + sw - min_w ? sl + e.X : sl + sw - min_w;
                        t = st + e.Y < st + sh - min_h ? st + e.Y : st + sh - min_h;
                        w = sl + sw - m_control.Left;
                        h = st + sh - m_control.Top;
                        break;

                    case 1:
                        t = st + e.Y < st + sh - min_h ? st + e.Y : st + sh - min_h;
                        h = st + sh - m_control.Top;
                        break;

                    case 2:
                        w = sw + e.X > min_w ? sw + e.X : min_w;
                        t = st + e.Y < st + sh - min_h ? st + e.Y : st + sh - min_h;
                        h = st + sh - m_control.Top;
                        break;

                    case 3:
                        w = sw + e.X > min_w ? sw + e.X : min_w;
                        break;

                    case 4:
                        w = sw + e.X > min_w ? sw + e.X : min_w;
                        h = sh + e.Y > min_h ? sh + e.Y : min_h;
                        break;

                    case 5:
                        h = sh + e.Y > min_h ? sh + e.Y : min_h;
                        break;

                    case 6:
                        l = sl + e.X < sl + sw - min_w ? sl + e.X : sl + sw - min_w;
                        w = sl + sw - m_control.Left;
                        h = sh + e.Y > min_h ? sh + e.Y : min_h;
                        break;

                    case 7:
                        l = sl + e.X < sl + sw - min_w ? sl + e.X : sl + sw - min_w;
                        w = sl + sw - m_control.Left;
                        break;
                }
                l = (l < BOX_SIZE) ? BOX_SIZE : l;
                t = (t < BOX_SIZE) ? BOX_SIZE : t;
                w = (l + w < pa_w - BOX_SIZE) ? w : pa_w - l - BOX_SIZE;
                h = (t + h < pa_h - BOX_SIZE) ? h : pa_h - t - BOX_SIZE;
                m_control.SetBounds(l, t, w, h);
            }
        }

        private void hdl_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            MoveHandles();
            ShowHandles();
            m_control.Visible = true;
        }

        private void MoveHandles()
        {
            int sX = m_control.Left - BOX_SIZE;
            int sY = m_control.Top - BOX_SIZE;
            int sW = m_control.Width + BOX_SIZE;
            int sH = m_control.Height + BOX_SIZE;
            int hB = BOX_SIZE / 2;
            int[] arrPosX = new int[] { sX + hB, sX + sW / 2, sX + sW - hB, sX + sW - hB, sX + sW - hB, sX + sW / 2, sX + hB, sX + hB };
            int[] arrPosY = new int[] { sY + hB, sY + hB, sY + hB, sY + sH / 2, sY + sH - hB, sY + sH - hB, sY + sH - hB, sY + sH / 2 };
            for (int i = 0; i < 8; i++)
            {
                lbl[i].SetBounds(arrPosX[i], arrPosY[i], BOX_SIZE, BOX_SIZE);
            }
        }

        private void HideHandles()
        {
            for (int i = 0; i < 8; i++)
            {
                lbl[i].Visible = false;
            }
        }

        private void ShowHandles()
        {
            if (m_control != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    lbl[i].Visible = true;
                }
            }
        }

        #endregion Box Event

        #region Extra

        /// <summary>
        /// 用于解决当窗口缩放时,控件太大的问题
        /// </summary>
        public void DealOutBounds()
        {
            int l = m_control.Left;
            int w = m_control.Width;
            int t = m_control.Top;
            int h = m_control.Height;

            int pa_w = m_control.Parent.ClientRectangle.Width;
            int pa_h = m_control.Parent.ClientRectangle.Height;

            w = (pa_w - 2 * BOX_SIZE > w) ? w : pa_w - 2 * BOX_SIZE;
            h = (pa_h - 2 * BOX_SIZE > h) ? h : pa_h - 2 * BOX_SIZE;
            l = (l + w < pa_w - BOX_SIZE) ? l : pa_w - w - BOX_SIZE;
            t = (t + h < pa_h - BOX_SIZE) ? t : pa_h - h - BOX_SIZE;
            w = (w > min_w) ? w : min_w;
            h = (h > min_h) ? h : min_h;
            m_control.SetBounds(l, t, w, h);
        }

        #endregion Extra
    }
}