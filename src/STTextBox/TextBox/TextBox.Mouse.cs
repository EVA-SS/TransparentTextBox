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

        string[] sptext = new string[]{
            "，",
            ",",
            "。",
            ".",
            "；",
            ";",

            "\r","\t","\n"
        };
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (cache_font != null)
            {
                var index = GetCaretPostion(e.Location.X + scrollx, e.Location.Y + scrolly);

                int start = 0, end = cache_font.Length;

                #region 查找前面

                if (index > 0)
                {
                    var is_ = new List<int>();
                    for (int i = 0; i < index; i++)
                    {
                        if (sptext.Contains(cache_font[i].text)) is_.Add(i);
                    }
                    if (is_.Count > 0)
                        start = is_[is_.Count - 1] + 1;
                }

                #endregion

                #region 查找后面

                if (index < end)
                {
                    for (int i = index; i < end; i++)
                    {
                        if (sptext.Contains(cache_font[i].text))
                        {
                            end = i;
                            i = end;
                        }
                    }
                }

                #endregion

                SelectionStart = start;
                SelectionLength = end - start;
            }
        }
    }
}