using System.Drawing;

namespace TSkin.ST
{
    public struct FindInfo
    {
        public bool Find;
        public int IndexOfLine;
        public int IndexOfCharInLine;
        public int IndexOfChar { get { return Line.IndexOfFirstChar + IndexOfCharInLine; } }
        public TextLine Line;
        public Point Location;

        public static FindInfo Empty;
    }
}
