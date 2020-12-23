using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace IDevTrack.Utils
{
    // @brief  draw tools
    // @author urays @date 2019-02-19
    // @email  zhl.rays@outlook.com
    // https://github.com/urays/

    public class DrawPlate
    {
        #region Constants

        public enum MODE { NONE, DOTS, LINES, RECTANGLE, RECTDOT };

        #endregion Constants

        #region Fields

        private MODE _mode;
        private Point _tpoint;
        private Bitmap _curpic;
        private Bitmap _lstpic;
        private bool _mvflag;
        private readonly Stack _undo = new Stack();
        private readonly Stack _redo = new Stack();
        private PictureBox OBJ;

        #endregion Fields

        #region Properties

        public float PenWidth { set; get; } = 2.5f;

        public Color PenColor { get; set; } = Color.Green;

        public MODE Mode
        {
            get { return _mode; }
            set
            {
                try
                {
                    if (_mode != value)
                    {
                        _mode = value;
                        if (_curpic != null) _curpic.Dispose(); //更改模式则 清除用于撤销/重做的历史记录
                        if (_lstpic != null) _lstpic.Dispose();
                        switch (_mode)  //模式相关设置
                        {
                            case (MODE.NONE):
                                _undo.Clear(); _redo.Clear();
                                OBJ.Cursor = Cursors.Arrow;
                                break;

                            case (MODE.DOTS):
                                Utils.Basic.SetCursorIcon(OBJ, Properties.Resources.pen, 5);
                                break;

                            case (MODE.LINES):
                                Utils.Basic.SetCursorIcon(OBJ, Properties.Resources.line, 5);
                                break;

                            case (MODE.RECTANGLE):
                                Utils.Basic.SetCursorIcon(OBJ, Properties.Resources.rectange, 5);
                                break;

                            case (MODE.RECTDOT):
                                Utils.Basic.SetCursorIcon(OBJ, Properties.Resources.dotpen, 5);
                                break;
                        }
                        if (_mode != MODE.NONE)
                        {
                            if (OBJ.Image == null)
                            {
                                OBJ.Image = Utils.Basic.ColorBitmap(OBJ.Width, OBJ.Height, OBJ.BackColor);
                            }
                            _curpic = (Bitmap)(OBJ).Image.Clone();
                            _lstpic = (Bitmap)_curpic.Clone();
                        }
                        else
                        {
                        }
                    }
                }
                catch { }
            }
        }

        #endregion Properties

        #region Constructors

        public DrawPlate()
        {
        }

        public void SetOBJ(PictureBox objc)
        {
            OBJ = objc;
            _mode = MODE.NONE;
            _mvflag = false;
        }

        #endregion Constructors

        #region Methods

        private void SendImage(Bitmap data)
        {
            try
            {
                if (OBJ.Image != null)
                {
                    OBJ.Image.Dispose();
                }
                OBJ.Image = Utils.Basic.BitmapToImage(data);
            }
            catch { }
        }

        private void Line(Point start, Point end, Graphics g, Pen pen)
        {
            using (Brush bsh = new SolidBrush(pen.Color))
            {
                g.FillEllipse(bsh, start.X - PenWidth / 2, start.Y - PenWidth / 2, PenWidth, PenWidth);
                g.DrawLine(pen, start, end);
                g.FillEllipse(bsh, end.X - PenWidth / 2, end.Y - PenWidth / 2, PenWidth, PenWidth);
            }
        }

        private static void Rectangle(Point start, Point end, Graphics g, Pen pen)
        {
            try
            {
                g.DrawRectangle(pen, Math.Min(start.X, end.X),
                    Math.Min(start.Y, end.Y), Math.Abs(start.X - end.X), Math.Abs(start.Y - end.Y));
            }
            catch { }
        }

        private void RectDot(Point start, Graphics g, Pen pen)
        {
            try
            {
                Point[] stp = new Point[] { Utils.OPSwcode.GetLogicCellxy(start, OBJ.Width, OBJ.Height) };
                Utils.OPSwcode.DrawCells(g, OBJ.Width, OBJ.Height, pen.Color, stp);

                //Debug.WriteLine("[" + start.X.ToString() + "," + start.Y.ToString() + "]");
            }
            catch { }
        }

        public void DrawStart(Point mousepos)
        {
            _mvflag = false; //初始化为未拖动画笔
            _tpoint = mousepos;
            if (_mode == MODE.DOTS || _mode == MODE.RECTDOT)
            {
                DrawMove(mousepos);
            }
        }

        public void DrawMove(Point mousepos)
        {
            if (_mode == MODE.NONE) { return; }

            _mvflag = true;
            try
            {
                if (_mode != MODE.DOTS && _mode != MODE.RECTDOT)
                {
                    _lstpic.Dispose();
                    _lstpic = (Bitmap)_curpic.Clone();
                }

                Graphics g = Graphics.FromImage(_lstpic);
                Pen pen = new Pen(PenColor, PenWidth);
                switch (_mode)
                {
                    case MODE.LINES:
                        Line(_tpoint, mousepos, g, pen);
                        break;

                    case MODE.RECTANGLE:
                        Rectangle(_tpoint, mousepos, g, pen);
                        break;

                    case MODE.DOTS:
                        Line(_tpoint, mousepos, g, pen);
                        _tpoint = mousepos;
                        break;

                    case MODE.RECTDOT:
                        RectDot(_tpoint, g, pen);
                        _tpoint = mousepos;
                        break;
                }
                SendImage(_lstpic);//绘制
                g.Dispose();
                pen.Dispose();
            }
            catch { }
        }

        public void DrawFinish()
        {
            try
            {
                if (_mvflag && _mode != MODE.NONE)
                {
                    _mvflag = false;
                    _redo.Clear(); //清空重做栈
                    _undo.Push(_curpic.Clone());//保存上一幅图像
                    _curpic.Dispose();
                    _curpic = (Bitmap)_lstpic.Clone();
                }
            }
            catch { }
        }

        //public bool CanUndo() { return _undo.Count > 0; }

        public void Undo()
        {
            if (_undo.Count > 0)
            {
                _redo.Push(_curpic.Clone());
                _curpic.Dispose();
                _curpic = (Bitmap)_undo.Pop();
                _lstpic.Dispose();
                _lstpic = (Bitmap)_curpic.Clone();
                SendImage(_curpic);
            }
        }

        //public bool CanRedo() { return _redo.Count > 0; }

        public void Redo()
        {
            if (_redo.Count > 0)
            {
                _undo.Push(_curpic.Clone());
                _curpic.Dispose();
                _curpic = (Bitmap)_redo.Pop();
                _lstpic.Dispose();
                _lstpic = (Bitmap)_curpic.Clone();
                SendImage(_curpic);
            }
        }

        #endregion Methods
    }
}