using System.Collections.Generic;
using System.Drawing;

namespace CCS.Catcher.Internal
{
    public class DataCell
    {
        //操作数标记
        public int sign { get; set; } = 0;

        public Color clr { get; set; } = Color.Green;

        public Point loc { get; set; } = new Point(0, 0);
    }

    public class DrawPlate
    {
        /// <summary>
        /// 当前操作指针
        /// </summary>
        public int CurOper { get; set; }

        public Color PenColor { get; set; } = Color.Green;

        private readonly Stack<List<DataCell>> _undo = new System.Collections.Generic.Stack<List<DataCell>>();
        private readonly Stack<List<DataCell>> _redo = new System.Collections.Generic.Stack<List<DataCell>>();

        public DrawPlate()
        {
        }
    }
}