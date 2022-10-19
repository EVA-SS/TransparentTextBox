namespace TSkin.ST
{
    public interface ITextHistory
    {
        void SetHistory(TextHistoryRecord[] histories);
        TextHistoryRecord[] GetUndo();
        TextHistoryRecord[] GetRedo();
        void Clear();
    }
}
