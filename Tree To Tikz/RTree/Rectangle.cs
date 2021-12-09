using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tree_To_Tikz
{
    public class Rectangle : IComparable<Rectangle>
    {
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Area { get { return (Right - Left) * (Top - Bottom); } }

        public bool Overlaps(Rectangle r)
        {
            return (IsInBetween(Left, r.Left, r.Right) || IsInBetween(Right, r.Left, r.Right))
                && (IsInBetween(Top, r.Bottom, r.Top) || IsInBetween(Bottom, r.Bottom, r.Top));
        }

        public Rectangle(double left, double top, double right, double bottom)
        {
            if (right < left || top < bottom)
                throw new ArgumentException();
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public Rectangle(List<Record> group)
        {
            Left = group.Min(r => r.MBR.Left);
            Right = group.Max(r => r.MBR.Right);
            Top = group.Max(r => r.MBR.Top);
            Bottom = group.Min(r => r.MBR.Bottom);
        }

        public Rectangle(Rectangle r1, Rectangle r2)
        {
            Left = Math.Min(r1.Left, r2.Left);
            Right = Math.Max(r1.Right, r2.Right);
            Top = Math.Max(r1.Top, r2.Top);
            Bottom = Math.Min(r1.Bottom, r2.Bottom);
        }

        bool IsInBetween(double x, double min, double max)
        {
            return min <= x && x <= max;
        }

        public int CompareTo(Rectangle other)
        {
            if (Area < other.Area)
                return -1;
            else if (Area == other.Area)
                return 0;
            else
                return 1;
        }
    }
}
