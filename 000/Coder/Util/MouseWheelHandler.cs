using System;
using System.Windows.Forms;

namespace IDevTrack.Coder.Util
{
	/// <summary>
	/// Accumulates mouse wheel deltas and reports the actual number of lines to scroll.
	/// </summary>
	class MouseWheelHandler
	{	
		const int WHEEL_DELTA = 120;
		
		int mouseWheelDelta;
		
		public int GetScrollAmount(MouseEventArgs e)
		{
			// accumulate the delta to support high-resolution mice
			mouseWheelDelta += e.Delta;
			
			int linesPerClick = Math.Max(SystemInformation.MouseWheelScrollLines, 1);
			
			int scrollDistance = mouseWheelDelta * linesPerClick / WHEEL_DELTA;
			mouseWheelDelta %= Math.Max(1, WHEEL_DELTA / linesPerClick);
			return scrollDistance;
		}
	}
}
