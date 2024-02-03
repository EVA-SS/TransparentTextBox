namespace AntDesign
{
    partial class TextBox
    {
        void AddHistoryRecord()
        {
            if (history_I > -1)
            {
                history_Log.RemoveRange(history_I + 1, history_Log.Count - (history_I + 1));
                history_I = -1;
            }
            if (IsPassWord) return;
            history_Log.Add(new TextHistoryRecord(this));
        }

        int history_I = -1;
        List<TextHistoryRecord> history_Log = new List<TextHistoryRecord>();
        internal class TextHistoryRecord
        {
            public TextHistoryRecord(TextBox input)
            {
                SelectionStart = input.SelectionStart;
                SelectionLength = input.SelectionLength;
                Text = input.Text;
            }
            public int SelectionStart { get; set; }
            public int SelectionLength { get; set; }
            public string? Text { get; set; }
        }
    }
}