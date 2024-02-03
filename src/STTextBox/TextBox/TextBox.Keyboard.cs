namespace AntDesign
{
    partial class TextBox
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Back:
                    ProcessBackSpaceKey();
                    return true;
                //========================================================
                case Keys.Left:
                    ProcessLeftKey();
                    return true;
                case Keys.Up:
                    if (multiline) ProcessUpKey();
                    else ProcessLeftKey();
                    return true;
                case Keys.Right:
                    ProcessRightKey();
                    return true;
                case Keys.Down:
                    if (multiline) ProcessDownKey();
                    else ProcessRightKey();
                    return true;
                case Keys.Home:
                    ProcessHomeKey();
                    return true;
                case Keys.End:
                    ProcessEndKey();
                    return true;
                //========================================================
                case Keys.Tab:
                    if (multiline && AcceptsTab)
                    {
                        EnterText("\t");
                        return true;
                    }
                    break;
                case Keys.Enter:
                    if (multiline && AcceptsTab)
                    {
                        EnterText(Environment.NewLine);
                        return true;
                    }
                    break;
                //========================================================
                case Keys.Control | Keys.A:
                    SelectAll();
                    return true;
                case Keys.Control | Keys.C:
                    Copy();
                    return true;
                case Keys.Control | Keys.X:
                    Cut();
                    return true;
                case Keys.Control | Keys.V:
                    Paste();
                    return true;
                case Keys.Control | Keys.Z:
                    Undo();
                    return true;
                case Keys.Control | Keys.Y:
                    Redo();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar < 32) return;
            EnterText(e.KeyChar.ToString());
        }

        /// <summary>
        /// 删除文本
        /// </summary>
        void ProcessBackSpaceKey()
        {
            if (cache_font == null || ReadOnly) return;
            if (selectionLength > 0)
            {
                int start = selectionStart, end = selectionLength;
                AddHistoryRecord();
                int end_temp = start + end;
                var texts = new List<string>();
                foreach (var it in cache_font)
                {
                    if (it.i < start || it.i >= end_temp) texts.Add(it.text);
                }
                Text = string.Join("", texts);
                SelectionLength = 0;
                SelectionStart = start;
            }
            else if (selectionStart > 0)
            {
                int start = selectionStart - 1;
                var texts = new List<string>();
                foreach (var it in cache_font)
                {
                    if (start != it.i) texts.Add(it.text);
                }
                Text = string.Join("", texts);
                SelectionStart = start;
            }
        }

        void ProcessLeftKey()
        {
            SelectionLength = 0;
            SelectionStart--;
        }

        void ProcessRightKey()
        {
            SelectionLength = 0;
            SelectionStart++;
        }

        void ProcessUpKey()
        {
            SelectionLength = 0;
            if (cache_font == null) SelectionStart--;
            else
            {
                int end = SelectionStart;
                if (end > cache_font.Length - 1) end = cache_font.Length - 1;
                var it = cache_font[end];
                var nearest = FindNearestFont(it.rect.X + it.rect.Width / 2, it.rect.Y - it.rect.Height / 2, cache_font);
                if (nearest == null || nearest.i == selectionStart) SelectionStart--;
                else SelectionStart = nearest.i;
            }
        }

        void ProcessDownKey()
        {
            SelectionLength = 0;
            if (cache_font == null) SelectionStart++;
            else
            {
                int end = SelectionStart;
                if (end > cache_font.Length - 1) return;
                var it = cache_font[end];
                var nearest = FindNearestFont(it.rect.X + it.rect.Width / 2, it.rect.Bottom + it.rect.Height / 2, cache_font);
                if (nearest == null || nearest.i == selectionStart) SelectionStart++;
                else SelectionStart = nearest.i;
            }
        }

        void ProcessHomeKey()
        {
            SelectionLength = 0;
            SelectionStart = 0;
        }

        void ProcessEndKey()
        {
            if (cache_font == null) return;
            SelectionLength = 0;
            SelectionStart = cache_font.Length;
        }
    }
}