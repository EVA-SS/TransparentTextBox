namespace AntDesign
{
    /// <summary>
    /// 透明文本框
    /// </summary>
    partial class TextBox
    {
        bool mouseDown = false, mouseDownMove = false;
        Point oldMouseDown;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && cache_font != null && e.Clicks > 1)
            {
                mouseDownMove = mouseDown = false;

                var index = GetCaretPostion(e.Location.X + scrollx, e.Location.Y + scrolly);

                int start = 0, end = 1;

                if (index > 0) start = FindStart(cache_font, index - 2);
                if (index < cache_font.Length) end = FindEnd(cache_font, index);

                SelectionStart = start;
                SelectionLength = end - start;

                return;
            }
            mouseDownMove = false;
            Select();
            oldMouseDown = e.Location;
            SelectionStart = GetCaretPostion(e.Location.X + scrollx, e.Location.Y + scrolly);
            SelectionLength = 0;
            if (e.Button == MouseButtons.Left && cache_font != null) mouseDown = true;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseDown && cache_font != null)
            {
                mouseDownMove = true;
                Cursor = Cursors.IBeam;
                var index = GetCaretPostion(oldMouseDown.X + scrollx + (e.Location.X - oldMouseDown.X), oldMouseDown.Y + scrolly + (e.Location.Y - oldMouseDown.Y));
                SelectionLength = Math.Abs(index - selectionStart);
                if (index > selectionStart) selectionStartTemp = selectionStart;
                else selectionStartTemp = index;
                SetCaretPostion(index);
            }
            else
            {
                if (rect_text.Contains(e.Location)) Cursor = Cursors.IBeam;
                else Cursor = DefaultCursor;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (mouseDown && mouseDownMove && cache_font != null)
            {
                var index = GetCaretPostion(e.Location.X + scrollx, e.Location.Y + scrolly);
                if (selectionStart == index) SelectionLength = 0;
                else if (index > selectionStart)
                {
                    SelectionLength = Math.Abs(index - selectionStart);
                    SelectionStart = selectionStart;
                }
                else
                {
                    int x = scrollx;
                    SelectionLength = Math.Abs(index - selectionStart);
                    SelectionStart = index;
                    ScrollX = x;
                }
            }
            mouseDown = false;
        }

        List<string> sptext = new List<string>{
            "，",
            ",",
            "。",
            ".",
            "；",
            ";",
            " ",

            "\r","\t","\n"
        };

        /// <summary>
        /// 查找前面
        /// </summary>
        int FindStart(CacheFont[] cache_font, int index)
        {
            for (int i = index; i >= 0; i--)
            {
                if (sptext.Contains(cache_font[i].text))
                {
                    return i + 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 查找后面
        /// </summary>
        int FindEnd(CacheFont[] cache_font, int index)
        {
            int end = cache_font.Length;
            for (int i = index; i < end; i++)
            {
                if (sptext.Contains(cache_font[i].text))
                {
                    return i;
                }
            }
            return end;
        }
    }
}