namespace TSkin.ST
{
    public struct TextHistoryRecord
    {
        public int Index;
        public string OldText;
        public string NewText;
        public string RealText;

        public static TextHistoryRecord Empty;

        public override string ToString()
        {
            return "[" + Index + "," + OldText + "," + NewText + "]";
        }

        public static bool operator ==(TextHistoryRecord a, TextHistoryRecord b)
        {
            if (a.Index != b.Index) return false;
            if (a.OldText != b.OldText) return false;
            if (a.NewText != b.NewText) return false;
            return true;
        }

        public static bool operator !=(TextHistoryRecord a, TextHistoryRecord b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
