using System.Collections.Generic;
using System.Drawing;

namespace CCS.Catcher.Internal
{
    public enum PixelShape
    {
        Cell = 0, //方形格子
        Normal,
    };

    public class DataCell
    {
        public PixelShape DataShape { get; set; } = PixelShape.Normal;

        public Color Color { get; set; } = Color.Green;

        //当PixelShape.Normals时有用
        public float PenWidth { get; set; } = 2.5F;

        public Point Location { get; set; } = new Point(0, 0);
    }

    public class DrawPlate
    {
        private readonly Stack<List<DataCell>> _undo = new System.Collections.Generic.Stack<List<DataCell>>();
        private readonly Stack<List<DataCell>> _redo = new System.Collections.Generic.Stack<List<DataCell>>();

        public DrawPlate()
        {
        }
    }
}