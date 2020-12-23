using System;

namespace IDevTrack.Coder.Document
{
	/// <summary>
	/// This interface is used to describe a span inside a text sequence
	/// </summary>
	public class AbstractSegment : ISegment
	{
		[CLSCompliant(false)]
		protected int offset = -1;
		[CLSCompliant(false)]
		protected int length = -1;
		
		#region IDevTrack.Coder.Document.ISegment interface implementation
		public virtual int Offset {
			get {
				return offset;
			}
			set {
				offset = value;
			}
		}
		
		public virtual int Length {
			get {
				return length;
			}
			set {
				length = value;
			}
		}
		
		#endregion
		
		public override string ToString()
		{
			return String.Format("[AbstractSegment: Offset = {0}, Length = {1}]",
			                     Offset,
			                     Length);
		}
		
		
	}
}
