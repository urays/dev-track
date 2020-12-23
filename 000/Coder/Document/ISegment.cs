using System.Collections.Generic;

namespace IDevTrack.Coder.Document
{
	/// <summary>
	/// This interface is used to describe a span inside a text sequence
	/// </summary>
	public interface ISegment
	{
		/// <value>
		/// The offset where the span begins
		/// </value>
		int Offset {
			get;
			set;
		}
		
		/// <value>
		/// The length of the span
		/// </value>
		int Length {
			get;
			set;
		}
	}

    public class SegmentComparer : IComparer<ISegment>
    {
        public int Compare(ISegment x, ISegment y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're 
                    // equal.  
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y 
                    // is greater.  
                    return -1;
                }
            }

            // If x is not null and y is null, x is greater.
            if (y == null)
            {
                return 1;
            }

            int retval = x.Offset.CompareTo(y.Offset);
            if (retval != 0)
                return retval;

            return x.Length.CompareTo(y.Length);
        }
    }
	
}
