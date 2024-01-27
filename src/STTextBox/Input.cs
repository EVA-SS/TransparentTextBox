using examples;
using System.ComponentModel;
using System.Globalization;

namespace AntDesign
{
    /// <summary>
    /// 透明文本框
    /// </summary>
    public class Input : Control
    {
        /// <summary>
        /// 透明文本框
        /// </summary>
        public Input()
        {
            m_sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            SetStyle(
               ControlStyles.AllPaintingInWmPaint |
               ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw |
               ControlStyles.DoubleBuffer |
               ControlStyles.SupportsTransparentBackColor |
               ControlStyles.ContainerControl |
               ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        #region 属性

        #region 文本

        string? _text = null;
        [Description("文本"), Category("外观"), DefaultValue(null)]
        public new string? Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                FixFontWidth();
                Invalidate();
            }
        }

        #endregion

        [Description("Emoji字体"), Category("外观"), DefaultValue("Segoe UI Emoji")]
        public string EmojiFont { get; set; } = "Segoe UI Emoji";

        #region 原生文本框

        int selectionStart = 0, selectionStartTemp = 0, selectionLength = 0;
        /// <summary>
        /// 所选文本的起点
        /// </summary>
        [Browsable(false), DefaultValue(0)]
        public int SelectionStart
        {
            get => selectionStart;
            set
            {
                if (value < 0) value = 0;
                else if (value > 0)
                {
                    if (cache_font == null) value = 0;
                    else if (value > cache_font.Length) value = cache_font.Length;
                }
                if (selectionStart == value) return;
                selectionStart = selectionStartTemp = value;
                SetCaretPostion(value);
                ScrollToX(value);
            }
        }

        /// <summary>
        /// 所选文本的长度
        /// </summary>
        [Browsable(false), DefaultValue(0)]
        public int SelectionLength
        {
            get => selectionLength;
            set
            {
                if (selectionLength == value) return;
                selectionLength = value;
                if (!mouseDown)
                {
                    SetCaretPostion(selectionStart+value);
                    ScrollToX(selectionStart + value);
                }
                Invalidate();
            }
        }

        #endregion

        #endregion

        #region 渲染

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics.High();
            if (cache_font != null)
            {
                g.TranslateTransform(-ScrollX, 0);
                if (selectionLength > 0)
                {
                    int end = selectionStartTemp + selectionLength - 1;
                    if (end > cache_font.Length - 1) end = cache_font.Length - 1;
                    CacheFont first = cache_font[selectionStartTemp], last = cache_font[end];
                    using (var brush = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
                    {
                        g.FillRectangle(brush, new Rectangle(first.rect.X, first.rect.Y, last.rect.Right - first.rect.X, first.rect.Height));
                    }
                }
                using (var fore = new SolidBrush(ForeColor))
                {
                    if (HasEmoji)
                    {
                        using (var font = new Font(EmojiFont, Font.Size))
                        {
                            foreach (var it in cache_font)
                            {
                                if (it.emoji) g.DrawString(it.text, font, fore, it.rect, m_sf);
                                else g.DrawString(it.text, Font, fore, it.rect, m_sf);
                            }
                        }
                    }
                    else
                    {
                        foreach (var it in cache_font)
                        {
                            g.DrawString(it.text, Font, fore, it.rect, m_sf);
                        }
                    }
                }
            }
            base.OnPaint(e);
        }

        #region 滚动条

        int scrollx = 0, ScrollMaxX = 0;
        int ScrollX
        {
            get => scrollx;
            set
            {
                if (value < 0) value = 0;
                else if (value > ScrollMaxX) value = ScrollMaxX;
                if (scrollx == value) return;
                scrollx = value;
                Invalidate();
            }
        }

        bool ScrollShow = false;
        void ScrollToX(int index)
        {
            if (ScrollShow && cache_font != null)
            {
                bool flag = false;
                Point pt = CurrentPos;
                //if (pt.Y < rect_text.Y)
                //{
                //    c.Scroll.YValue += (pt.Y - rect_text.Y) / c.LineHeight;
                //    flag = true;
                //}
                //else if (pt.Y + c.LineHeight > rect_text.Bottom)
                //{
                //    int nIncrement = (int)Math.Ceiling((pt.Y + c.LineHeight - rect_text.Bottom) / (float)c.LineHeight);
                //    c.Scroll.YValue += nIncrement;
                //    flag = true;
                //}
                if (pt.X >= rect_text.Right)
                {
                    int nIncrement = pt.X - rect_text.Right;
                    ScrollX = scrollx + nIncrement;
                    flag = true;
                }
                else if (pt.X < rect_text.X)
                {
                    int nIncrement = rect_text.X - pt.X;
                    ScrollX = scrollx - nIncrement;
                    flag = true;
                }
                if (flag) SetCaretPostion(index);
            }
            else scrollx = 0;
        }

        #endregion

        #endregion

        #region 坐标

        #region 确定字体宽度

        StringFormat m_sf = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        CacheFont[]? cache_font = null;
        bool HasEmoji = false;
        void FixFontWidth()
        {
            HasEmoji = false;
            cache_font = null;
            if (string.IsNullOrEmpty(_text)) return;
            else
            {
                int font_height = 0;
                var font_widths = new List<CacheFont>();
                using (var bmp = new Bitmap(1, 1))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        bool iseone = false;
                        foreach (char it in _text)
                        {
                            string txt = it.ToString();
                            var unicodeInfo = CharUnicodeInfo.GetUnicodeCategory(it);
                            if (IsEmoji(unicodeInfo))
                            {
                                HasEmoji = true;
                                if (unicodeInfo == UnicodeCategory.Surrogate)
                                {
                                    if (iseone)
                                    {
                                        iseone = false;
                                        font_widths[font_widths.Count - 1].text += txt;
                                        continue;
                                    }
                                    else iseone = true;
                                }
                                else iseone = false;
                                font_widths.Add(new CacheFont(txt, true, 0));
                            }
                            else
                            {
                                iseone = false;
                                var sizefont = g.MeasureString(txt, Font, 10000, m_sf);
                                if (font_height < sizefont.Height) font_height = (int)Math.Ceiling(sizefont.Height);
                                font_widths.Add(new CacheFont(txt, false, (int)Math.Ceiling(sizefont.Width)));
                            }
                        }

                        if (HasEmoji)
                        {
                            using (var font = new Font(EmojiFont, Font.Size))
                            {
                                foreach (var it in font_widths)
                                {
                                    if (it.emoji)
                                    {
                                        var sizefont = g.MeasureString(it.text, font, 10000, m_sf);
                                        if (font_height < sizefont.Height) font_height = (int)Math.Ceiling(sizefont.Height);
                                        it.width = (int)Math.Ceiling(sizefont.Width);
                                    }
                                }
                            }
                        }
                    }
                }
                int usex = 0;
                foreach (var it in font_widths)
                {
                    it.x = usex;
                    usex += it.width;
                }
                CaretHeight = font_height;
                cache_font = font_widths.ToArray();
                CalculateRect(cache_font);
            }
        }

        bool IsEmoji(UnicodeCategory unicodeInfo)
        {
            //return unicodeInfo == UnicodeCategory.Surrogate;
            return unicodeInfo == UnicodeCategory.Surrogate || unicodeInfo == UnicodeCategory.OtherSymbol ||
                 unicodeInfo == UnicodeCategory.MathSymbol ||
                  unicodeInfo == UnicodeCategory.EnclosingMark ||
                   unicodeInfo == UnicodeCategory.NonSpacingMark ||
                  unicodeInfo == UnicodeCategory.ModifierLetter;
        }

        class CacheFont
        {
            public CacheFont(string _text, bool _emoji, int _width)
            {
                text = _text;
                emoji = _emoji;
                width = _width;
            }

            public string text { get; set; }
            public Rectangle rect { get; set; }
            public bool emoji { get; set; }
            public int width { get; set; }
            public int x { get; set; }
        }

        #endregion

        void CalculateRect()
        {
            if (cache_font == null) return;
            CalculateRect(cache_font);
        }

        Rectangle rect_text;
        void CalculateRect(CacheFont[] cache_font)
        {
            var rect = ClientRectangle;
            int sps = (int)(CaretHeight * .4F), sps2 = sps * 2;
            rect_text = new Rectangle(rect.X + sps, rect.Y + (rect.Height - CaretHeight) / 2, rect.Width - sps2, CaretHeight);
            CurrentPos.Y = rect_text.Y;
            foreach (var it in cache_font)
            {
                it.rect = new Rectangle(rect_text.X + it.x, rect_text.Y, it.width, CaretHeight);
            }
            var last = cache_font[cache_font.Length - 1];
            ScrollMaxX = last.rect.Right - rect.Width + sps;
            ScrollShow = last.rect.Right > rect.Right;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            CalculateRect();
            base.OnSizeChanged(e);
        }

        #endregion

        #region 键盘

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Back:
                    ProcessBackSpaceKey();
                    return true;
                //========================================================
                case Keys.Left:
                case Keys.Up:
                    ProcessLeftKey();
                    return true;
                case Keys.Right:
                case Keys.Down:
                    ProcessRightKey();
                    return true;
                case Keys.Home:
                    ProcessHomeKey();
                    return true;
                case Keys.End:
                    ProcessEndKey();
                    return true;
                //========================================================
                //case Keys.Shift | Keys.Tab:
                //    this.ProcessShiftTabKey();
                //    return true;
                //case Keys.Shift | Keys.Up:
                //    this.ProcessUpKey(true);
                //    return true;
                //case Keys.Shift | Keys.Down:
                //    this.ProcessDownKey(true);
                //    return true;
                //case Keys.Shift | Keys.Left:
                //    this.ProcessLeftKey(true);
                //    return true;
                //case Keys.Shift | Keys.Right:
                //    this.ProcessRightKey(true);
                //    return true;
                //case Keys.Shift | Keys.Home:
                //    this.ProcessHomeKey(true);
                //    return true;
                //case Keys.Shift | Keys.End:
                //    this.ProcessEndKey(true);
                //    return true;
                //case Keys.Shift | Keys.PageUp:
                //    this.ProcessPageUpKey(true);
                //    return true;
                //case Keys.Shift | Keys.PageDown:
                //    this.ProcessPageDownKey(true);
                //    return true;
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

        /// <summary>
        /// 删除文本
        /// </summary>
        void ProcessBackSpaceKey()
        {
            if (cache_font == null) return;
            if (selectionLength > 0)
            {
                int start = selectionStart, end = selectionLength;
                m_arr.Add(new TextHistoryRecord(this));
                int end_temp = start + end;
                var texts = new List<string>();
                for (int i = 0; i < cache_font.Length; i++)
                {
                    if (i < start || i >= end_temp)
                        texts.Add(cache_font[i].text);
                }
                Text = string.Join("", texts);
                SelectionLength = 0;
            }
            else if (selectionStart > 0)
            {
                int start = selectionStart - 1, end = selectionLength;
                var texts = new List<string>();
                for (int i = 0; i < cache_font.Length; i++)
                {
                    if (start != i) texts.Add(cache_font[i].text);
                }
                Text = string.Join("", texts);
                SelectionStart = start;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar < 32) return;
            EnterText(e.KeyChar.ToString());
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

        #region 基础

        /// <summary>
        /// 将文本追加到当前文本中
        /// </summary>
        /// <param name="text">追加的文本</param>
        public void AppendText(string text)
        {
            EnterText(text);
        }

        /// <summary>
        /// 清除所有文本
        /// </summary>
        public void Clear()
        {
            Text = null;
        }

        /// <summary>
        /// 清除撤消缓冲区信息
        /// </summary>
        public void ClearUndo()
        {
            m_arr.Clear();
        }

        /// <summary>
        /// 复制
        /// </summary>
        public void Copy()
        {
            var text = GetSelectionText();
            if (text == null) return;
            Clipboard.SetText(text);
        }

        /// <summary>
        /// 剪贴
        /// </summary>
        public void Cut()
        {
            var text = GetSelectionText();
            if (text == null) return;
            Clipboard.SetText(text);
            ProcessBackSpaceKey();
        }

        /// <summary>
        /// 粘贴
        /// </summary>
        public void Paste()
        {
            string strText = Clipboard.GetText();
            if (string.IsNullOrEmpty(strText)) return;
            EnterText(strText);
        }

        /// <summary>
        /// 取消全部选中
        /// </summary>
        public void DeselectAll()
        {
            SelectionLength = 0;
        }

        /// <summary>
        /// 撤消
        /// </summary>
        public void Undo()
        {
            if (m_arr.Count > 0)
            {
                var index = m_arr.Count - 1;
                var it = m_arr[index];
                m_arr.RemoveAt(index);
                Text = it.Text;
                SelectionStart = it.SelectionStart;
                SelectionLength = it.SelectionLength;
            }
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
        }

        /// <summary>
        /// 文本选择范围
        /// </summary>
        /// <param name="start">第一个字符的位置</param>
        /// <param name="length">字符长度</param>
        public void Select(int start, int length)
        {
            SelectionStart = start;
            SelectionLength = length;
        }

        /// <summary>
        /// 选择所有文本
        /// </summary>
        public void SelectAll()
        {
            if (cache_font != null)
            {
                SelectionStart = 0;
                SelectionLength = cache_font.Length;
            }
        }

        void EnterText(string text)
        {
            m_arr.Add(new TextHistoryRecord(this));

            int countSurrogate = 0;
            foreach (var it in text)
            {
                var unicodeInfo = CharUnicodeInfo.GetUnicodeCategory(it);
                if (unicodeInfo == UnicodeCategory.Surrogate) countSurrogate++;
            }
            int len = (text.Length - countSurrogate) + countSurrogate / 2;
            if (cache_font == null)
            {
                Text = text;
                SelectionStart = selectionStart + len;
            }
            else
            {
                if (selectionLength > 0)
                {
                    int start = selectionStart, end = selectionLength;
                    m_arr.Add(new TextHistoryRecord(this));
                    int end_temp = start + end;
                    var texts = new List<string>();
                    for (int i = 0; i < cache_font.Length; i++)
                    {
                        if (i < start || i >= end_temp)
                            texts.Add(cache_font[i].text);
                    }
                    texts.Insert(start, text);
                    Text = string.Join("", texts);
                    SelectionLength = 0;
                    SelectionStart = start + len;
                }
                else
                {
                    int start = selectionStart - 1, end = selectionLength;
                    var texts = new List<string>();
                    for (int i = 0; i < cache_font.Length; i++)
                    {
                        texts.Add(cache_font[i].text);
                    }
                    texts.Insert(start + 1, text);
                    Text = string.Join("", texts);
                    SelectionStart = start + 1 + len;
                }
            }
        }

        string? GetSelectionText()
        {
            if (cache_font == null) return null;
            else
            {
                if (selectionLength > 0)
                {
                    int start = selectionStart, end = selectionLength;
                    int end_temp = start + end;
                    var texts = new List<string>();
                    for (int i = 0; i < cache_font.Length; i++)
                    {
                        if (i >= start && end_temp > i)
                            texts.Add(cache_font[i].text);
                    }
                    return string.Join("", texts);
                }
                return null;
            }
        }

        #region 历史

        List<TextHistoryRecord> m_arr = new List<TextHistoryRecord>();
        internal class TextHistoryRecord
        {
            public TextHistoryRecord(Input input)
            {
                SelectionStart = input.SelectionStart;
                SelectionLength = input.SelectionLength;
                Text = input.Text;
            }
            public int SelectionStart { get; set; }
            public int SelectionLength { get; set; }
            public string? Text { get; set; }
        }

        #endregion

        #endregion

        #endregion

        #region 鼠标

        bool mouseDown = false, mouseDownMove = false;
        int oldMouseDownX = 0;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouseDownMove = false;
            Select();
            SelectionLength = 0;
            oldMouseDownX = e.Location.X;
            SelectionStart = GetCaretPostion(e.Location.X + scrollx, e.Location.Y);
            if (e.Button == MouseButtons.Left && cache_font != null) mouseDown = true;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (mouseDown && cache_font != null)
            {
                mouseDownMove = true;
                Cursor = Cursors.IBeam;
                var index = GetCaretPostion(oldMouseDownX + scrollx + (e.Location.X - oldMouseDownX), e.Location.Y);
                SetCaretPostion(index);
                SelectionLength = Math.Abs(index - selectionStart);
                if (index > selectionStart) selectionStartTemp = selectionStart;
                else selectionStartTemp = index;
                ScrollToX(index);
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
                var index = GetCaretPostion(e.Location.X + scrollx, e.Location.Y);
                if (selectionStart == index) SelectionLength = 0;
                else if (index > selectionStart)
                {
                    SelectionLength = Math.Abs(index - selectionStart);
                    SelectionStart = selectionStart;
                }
                else
                {
                    SelectionLength = Math.Abs(index - selectionStart);
                    SelectionStart = index;
                }
            }
            mouseDown = false;
        }

        #endregion

        #region 重写

        #region DPI

        int CaretWidth = 1;
        int CaretHeight = 0;
        float Dpi = 1F;
        protected override void CreateHandle()
        {
            Dpi = DPI();
            CaretWidth = (int)(1 * Dpi);
            base.CreateHandle();
        }

        public float DPI()
        {
            float dpi = 1F;
#if NET40 || NET46
            using (var bmp = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    dpi = Dpi(g);
                }
            }
#else
            dpi = DeviceDpi / 96F;
#endif
            return dpi;
        }

        #endregion

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            FixFontWidth();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            ShowCaret = true;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            ShowCaret = false;
        }

        #region 系统消息

        IntPtr m_hIMC;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Win32.WM_IME_STARTCOMPOSITION:
                    m_hIMC = Win32.ImmGetContext(Handle);
                    OnImeStartPrivate(m_hIMC);
                    break;
                case Win32.WM_IME_ENDCOMPOSITION:
                    OnImeEndPrivate(m_hIMC);
                    break;
                case Win32.WM_IME_COMPOSITION:
                    if (((int)m.LParam & Win32.GCS_RESULTSTR) == Win32.GCS_RESULTSTR)
                    {
                        m.Result = (IntPtr)1;
                        OnImeResultStrPrivate(m_hIMC, Win32.ImmGetCompositionString(m_hIMC, Win32.GCS_RESULTSTR));
                        return;//Interrupt, WM_CHAR, WM_IME_CHAR messages will not be generated.
                    }
                    if (((int)m.LParam & Win32.GCS_COMPSTR) == Win32.GCS_COMPSTR)
                    {
                        OnImeCompStr(m_hIMC, Win32.ImmGetCompositionString(m_hIMC, Win32.GCS_COMPSTR));
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        #endregion

        #endregion

        #region 光标

        bool showCaret = false;
        bool ShowCaret
        {
            get => showCaret;
            set
            {
                if (showCaret == value) return;
                showCaret = value;
                if (showCaret)
                {
                    Win32.CreateCaret(Handle, IntPtr.Zero, CaretWidth, CaretHeight);
                    Win32.ShowCaret(Handle);
                    SetCaretPostion(selectionStart);
                }
                else
                {
                    Win32.HideCaret(Handle);
                    Win32.DestroyCaret();
                    Invalidate();
                }
            }
        }

        Point CurrentPos = new Point(0, 0);
        int GetCaretPostion(int x, int y)
        {
            if (cache_font == null) return 0;
            else
            {
                for (int i = 0; i < cache_font.Length; i++)
                {
                    var it = cache_font[i];
                    if (it.rect.X <= x && it.rect.Right >= x)
                    {
                        if (x > it.rect.X + it.rect.Width / 2) return i + 1;
                        else return i;
                    }
                }
                if (x > cache_font[cache_font.Length - 1].rect.Right) return cache_font.Length;
                else return 0;
            }
        }

        void SetCaretPostion(int index)
        {
            if (cache_font != null)
            {
                if (index >= cache_font.Length) CurrentPos.X = cache_font[cache_font.Length - 1].rect.Right - 1 - ScrollX;
                else CurrentPos.X = cache_font[index].rect.X - ScrollX;
                if (showCaret) Win32.SetCaretPos(CurrentPos.X, CurrentPos.Y);
            }
        }

        void OnImeStartPrivate(IntPtr hIMC)
        {
            var CandidateForm = new Win32.CANDIDATEFORM()
            {
                dwStyle = Win32.CFS_CANDIDATEPOS,
                ptCurrentPos = CurrentPos,
            };
            Win32.ImmSetCandidateWindow(hIMC, ref CandidateForm);
            var CompositionForm = new Win32.COMPOSITIONFORM()
            {
                dwStyle = Win32.CFS_FORCE_POSITION,
                ptCurrentPos = CurrentPos,
            };
            Win32.ImmSetCompositionWindow(hIMC, ref CompositionForm);
            var logFont = new Win32.LOGFONT()
            {
                lfHeight = CaretHeight,
                lfFaceName = Font.Name + "\0"
            };
            Win32.ImmSetCompositionFont(hIMC, ref logFont);
            OnImeStart(hIMC);
        }
        void OnImeEndPrivate(IntPtr hIMC)
        {
            Win32.ImmReleaseContext(Handle, hIMC);
            OnImeEnd(hIMC);
        }
        void OnImeResultStrPrivate(IntPtr hIMC, string strResult)
        {
            var CompositionForm = new Win32.COMPOSITIONFORM()
            {
                dwStyle = Win32.CFS_FORCE_POSITION,
                ptCurrentPos = CurrentPos
            };
            Win32.ImmSetCompositionWindow(hIMC, ref CompositionForm);
            OnImeResultStr(hIMC, strResult);
        }

        #endregion

        #region protected

        protected virtual void OnImeStart(IntPtr hIMC) { }
        protected virtual void OnImeResultStr(IntPtr hIMC, string strResult) { EnterText(strResult); }
        protected virtual void OnImeCompStr(IntPtr hIMC, string strComp) { }
        protected virtual void OnImeEnd(IntPtr hIMC) { }

        #endregion
    }
}