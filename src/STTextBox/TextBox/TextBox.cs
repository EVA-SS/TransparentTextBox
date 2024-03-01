using System.ComponentModel;
using System.Globalization;

namespace AntDesign
{
    /// <summary>
    /// 透明文本框
    /// </summary>
    public partial class TextBox : Control
    {
        /// <summary>
        /// 透明文本框
        /// </summary>
        public TextBox()
        {
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
                if (value != null && value.Length > MaxLength) value = value.Substring(0, MaxLength);
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
                Invalidate();
            }
        }

        #endregion

        /// <summary>
        /// 多行编辑是否允许输入回车
        /// </summary>
        [Description("多行编辑是否允许输入回车"), Category("行为"), DefaultValue(false)]
        public bool AcceptsReturn { get; set; } = false;

        /// <summary>
        /// 多行编辑是否允许输入制表符
        /// </summary>
        [Description("多行编辑是否允许输入制表符"), Category("行为"), DefaultValue(false)]
        public bool AcceptsTab { get; set; } = false;

        /// <summary>
        /// 只读
        /// </summary>
        [Description("只读"), Category("行为"), DefaultValue(false)]
        public bool ReadOnly { get; set; } = false;

        bool multiline = false;
        /// <summary>
        /// 多行文本
        /// </summary>
        [Description("多行文本"), Category("行为"), DefaultValue(false)]
        public bool Multiline
        {
            get => multiline;
            set
            {
                if (multiline == value) return;
                multiline = value;
                CalculateRect();
                Invalidate();
            }
        }

        HorizontalAlignment textalign = HorizontalAlignment.Left;
        /// <summary>
        /// 文本对齐方向
        /// </summary>
        [Description("文本对齐方向"), Category("外观"), DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment TextAlign
        {
            get => textalign;
            set
            {
                if (textalign == value) return;
                textalign = value;
                CalculateRect();
                Invalidate();
            }
        }

        string? placeholderText = null;
        /// <summary>
        /// 水印文本
        /// </summary>
        [Description("水印文本"), Category("行为"), DefaultValue(null)]
        public string? PlaceholderText
        {
            get => placeholderText;
            set
            {
                if (placeholderText == value) return;
                placeholderText = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 文本最大长度
        /// </summary>
        [Description("文本最大长度"), Category("行为"), DefaultValue(32767)]
        public int MaxLength { get; set; } = 32767;

        #region 密码框

        bool IsPassWord = false;
        string PassWordChar = "●";

        bool useSystemPasswordChar = false;
        /// <summary>
        /// 使用密码框
        /// </summary>
        [Description("使用密码框"), Category("行为"), DefaultValue(false)]
        public bool UseSystemPasswordChar
        {
            get => useSystemPasswordChar;
            set
            {
                if (useSystemPasswordChar == value) return;
                useSystemPasswordChar = value;
                SetPassWord();
            }
        }

        char passwordChar = '\0';
        /// <summary>
        /// 自定义密码字符
        /// </summary>
        [Description("自定义密码字符"), Category("行为"), DefaultValue((char)0)]
        public char PasswordChar
        {
            get => passwordChar;
            set
            {
                if (passwordChar == value) return;
                passwordChar = value;
                SetPassWord();
            }
        }

        void SetPassWord()
        {
            if (passwordChar != '\0')
            {
                PassWordChar = passwordChar.ToString();
                IsPassWord = true;
            }
            else if (useSystemPasswordChar)
            {
                PassWordChar = "●";
                IsPassWord = true;
            }
            else IsPassWord = false;
            FixFontWidth();
            Invalidate();
        }

        #endregion

        #endregion

        #region 方法

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
            history_Log.Clear();
        }

        /// <summary>
        /// 复制
        /// </summary>
        public void Copy()
        {
            if (IsPassWord) return;
            var text = GetSelectionText();
            if (text == null) return;
            Clipboard.SetText(text);
        }

        /// <summary>
        /// 剪贴
        /// </summary>
        public void Cut()
        {
            if (IsPassWord) return;
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
            if (IsPassWord) return;
            if (history_Log.Count > 0)
            {
                int index;
                if (history_I == -1)
                {
                    index = history_Log.Count - 1;
                    AddHistoryRecord();
                }
                else index = history_I - 1;
                if (index > -1)
                {
                    var it = history_Log[index];
                    history_I = index;
                    Text = it.Text;
                    SelectionStart = it.SelectionStart;
                    SelectionLength = it.SelectionLength;
                }
            }
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            if (IsPassWord) return;
            if (history_Log.Count > 0 && history_I > -1)
            {
                int index = history_I + 1;
                if (history_Log.Count > index)
                {
                    var it = history_Log[index];
                    history_I = index;
                    Text = it.Text;
                    SelectionStart = it.SelectionStart;
                    SelectionLength = it.SelectionLength;
                }
            }
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
            if (ReadOnly) return;
            AddHistoryRecord();
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
                    AddHistoryRecord();
                    int end_temp = start + end;
                    var texts = new List<string>();
                    foreach (var it in cache_font)
                    {
                        if (it.i < start || it.i >= end_temp)
                            texts.Add(it.text);
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
                    foreach (var it in cache_font) texts.Add(it.text);
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
                    foreach (var it in cache_font)
                    {
                        if (it.i >= start && end_temp > it.i) texts.Add(it.text);
                    }
                    return string.Join("", texts);
                }
                return null;
            }
        }

        #endregion

        #region 重写

        #region DPI

        float Dpi = 1F;
        protected override void CreateHandle()
        {
            Dpi = DPI();
            CurrentCaret.Width = (int)(1 * Dpi);
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

        #region 焦点

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

        #endregion

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
                    Win32.CreateCaret(Handle, IntPtr.Zero, CurrentCaret.Width, CurrentCaret.Height);
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

        Rectangle CurrentCaret = new Rectangle(0, 0, 1, 0);

        #region 得到光标位置

        /// <summary>
        /// 通过坐标系查找光标位置
        /// </summary>
        int GetCaretPostion(int x, int y)
        {
            if (cache_font == null) return 0;
            else
            {
                foreach (var it in cache_font)
                {
                    if (it.rect.X <= x && it.rect.Right >= x && it.rect.Y <= y && it.rect.Bottom >= y)
                    {
                        if (x > it.rect.X + it.rect.Width / 2) return it.i + 1;
                        else return it.i;
                    }
                }
                var nearest = FindNearestFont(x, y, cache_font);
                if (nearest == null)
                {
                    if (x > cache_font[cache_font.Length - 1].rect.Right) return cache_font.Length;
                    else return 0;
                }
                else
                {
                    if (x > nearest.rect.X + nearest.rect.Width / 2) return nearest.i + 1;
                    else return nearest.i;
                }
            }
        }

        /// <summary>
        /// 寻找最近的矩形和距离的辅助方法
        /// </summary>
        CacheFont? FindNearestFont(int x, int y, CacheFont[] cache_font)
        {
            double minDistance = int.MaxValue;
            CacheFont? result = null;
            for (int i = 0; i < cache_font.Length; i++)
            {
                var it = cache_font[i];
                // 计算点到矩形四个边的最近距离，取最小值作为当前矩形的最近距离
                int distanceToLeft = Math.Abs(x - (it.rect.Left + it.rect.Width / 2)),
                    distanceToTop = Math.Abs(y - (it.rect.Top + it.rect.Height / 2));
                double currentMinDistance = new int[] { distanceToLeft, distanceToTop }.Average();

                // 如果当前矩形的最近距离比之前找到的最近距离小，更新最近距离和最近矩形信息
                if (currentMinDistance < minDistance)
                {
                    minDistance = currentMinDistance;
                    result = it;
                }
            }
            return result;
        }

        #endregion

        int CurrentPosIndex = 0;
        void SetCaretPostion()
        {
            SetCaretPostion(CurrentPosIndex);
        }
        void SetCaretPostion(int selectionStart)
        {
            CurrentPosIndex = selectionStart;
            if (showCaret)
            {
                if (cache_font == null) Win32.SetCaretPos(CurrentCaret.X - scrollx, CurrentCaret.Y - scrolly);
                else
                {
                    Rectangle r;
                    if (selectionStart >= cache_font.Length)
                    {
                        r = cache_font[cache_font.Length - 1].rect;
                        CurrentCaret.X = r.Right - 1;
                        CurrentCaret.Y = r.Y;
                    }
                    else
                    {
                        r = cache_font[selectionStart].rect;
                        CurrentCaret.X = r.X;
                        CurrentCaret.Y = r.Y;
                    }
                    Win32.SetCaretPos(CurrentCaret.X - scrollx, CurrentCaret.Y - scrolly);
                    ScrollTo(r);
                }
            }
        }

        void OnImeStartPrivate(IntPtr hIMC)
        {
            var point = CurrentCaret.Location;
            point.Offset(0, -scrolly);
            var CandidateForm = new Win32.CANDIDATEFORM()
            {
                dwStyle = Win32.CFS_CANDIDATEPOS,
                ptCurrentPos = point,
            };
            Win32.ImmSetCandidateWindow(hIMC, ref CandidateForm);
            var CompositionForm = new Win32.COMPOSITIONFORM()
            {
                dwStyle = Win32.CFS_FORCE_POSITION,
                ptCurrentPos = point,
            };
            Win32.ImmSetCompositionWindow(hIMC, ref CompositionForm);
            var logFont = new Win32.LOGFONT()
            {
                lfHeight = CurrentCaret.Height,
                lfFaceName = Font.Name + "\0"
            };
            Win32.ImmSetCompositionFont(hIMC, ref logFont);
        }
        void OnImeEndPrivate(IntPtr hIMC)
        {
            Win32.ImmReleaseContext(Handle, hIMC);
        }
        void OnImeResultStrPrivate(IntPtr hIMC, string strResult)
        {
            var CompositionForm = new Win32.COMPOSITIONFORM()
            {
                dwStyle = Win32.CFS_FORCE_POSITION,
                ptCurrentPos = CurrentCaret.Location
            };
            Win32.ImmSetCompositionWindow(hIMC, ref CompositionForm);
            EnterText(strResult);
        }

        #endregion
    }
}